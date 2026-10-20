using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace RoR2DirectConnect
{
    /// <summary>
    /// 管理直连模式的稳定身份和昵称同步。
    ///
    /// 网络身份必须与昵称分离：身份用于认证、玩家对象匹配和控制权分配，
    /// 昵称只用于显示。旧版把昵称直接作为 PlatformID，重名或字符串 ID
    /// 丢失时会造成重复账号、选角不同步和角色控制权丢失。
    /// </summary>
    internal static class DirectConnectIdentity
    {
        private const ulong LegacySharedId = 76561198000000000UL;
        private const int CurrentIdentitySchemaVersion = 2;
        private const short DisplayNameMessageType = 1973;
        private const string AuthPayloadPrefix = "R2DC_NAME:";
        private const int MaxDisplayNameLength = 32;

        private static readonly Dictionary<ulong, string> DisplayNames =
            new Dictionary<ulong, string>();

        private static NetworkClient _registeredClient;

        private sealed class DisplayNameMessage : MessageBase
        {
            public ulong platformId;
            public string displayName;

            public override void Serialize(NetworkWriter writer)
            {
                writer.WritePackedUInt64(platformId);
                writer.Write(displayName ?? string.Empty);
            }

            public override void Deserialize(NetworkReader reader)
            {
                platformId = reader.ReadPackedUInt64();
                displayName = reader.ReadString();
            }
        }

        public static bool IsSupportedNetworkMode(out string reason)
        {
            NetworkManagerSystem manager = NetworkManagerSystem.singleton;
            if (!manager)
            {
                reason = "网络管理器尚未初始化。";
                return false;
            }

            if (!(manager is NetworkManagerSystemSteam))
            {
                reason = "IP 直连暂不支持 EOS/跨平台模式。请关闭跨平台联机，或使用 --disableCrossplay 启动游戏。";
                return false;
            }

            reason = null;
            return true;
        }

        public static ulong GetOrCreatePlatformId()
        {
            ulong platformId;
            string configured = Plugin.ConfigPlatformId?.Value;
            ulong legacyNicknameId = 0UL;
            string configuredName = Plugin.ConfigDisplayName?.Value;
            if (!string.IsNullOrEmpty(configuredName))
            {
                legacyNicknameId = (ulong)(configuredName.GetHashCode() & 0x7FFFFFFF)
                    + LegacySharedId;
            }

            // 旧版所有安装共用同一个默认值，必须自动迁移为真正唯一的 ID。
            if (ulong.TryParse(configured, out platformId)
                && Plugin.ConfigIdentitySchemaVersion?.Value >= CurrentIdentitySchemaVersion
                && platformId != 0UL
                && platformId != LegacySharedId
                && platformId != legacyNicknameId)
            {
                return platformId;
            }

            byte[] bytes = new byte[8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // 使用独立的高位命名空间，避免 0 和旧版共享默认值。
            platformId = 0xD100000000000000UL
                | (BitConverter.ToUInt64(bytes, 0) & 0x00FFFFFFFFFFFFFFUL);

            if (platformId == LegacySharedId)
                platformId++;

            Plugin.ConfigPlatformId.Value = platformId.ToString();
            Plugin.ConfigIdentitySchemaVersion.Value = CurrentIdentitySchemaVersion;
            Plugin.Log?.LogInfo($"Generated persistent direct-connect ID: {platformId}");
            return platformId;
        }

        public static string GetLocalDisplayName(ulong platformId)
        {
            string displayName = Plugin.ConfigDisplayName?.Value;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                try
                {
                    if (LocalUserManager.readOnlyLocalUsersList.Count > 0)
                        displayName = LocalUserManager.readOnlyLocalUsersList[0].userProfile?.name;
                }
                catch
                {
                    // 用户资料尚未初始化时使用稳定回退名。
                }
            }

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = "Player_" + (platformId % 10000UL).ToString("D4");

            return SanitizeDisplayName(displayName);
        }

        public static byte[] CreateAuthPayload(string displayName)
        {
            return Encoding.UTF8.GetBytes(AuthPayloadPrefix + SanitizeDisplayName(displayName));
        }

        public static bool TryReadDisplayName(byte[] authTicket, out string displayName)
        {
            displayName = null;
            if (authTicket == null || authTicket.Length == 0 || authTicket.Length > 512)
                return false;

            try
            {
                string payload = Encoding.UTF8.GetString(authTicket);
                if (!payload.StartsWith(AuthPayloadPrefix, StringComparison.Ordinal))
                    return false;

                displayName = SanitizeDisplayName(payload.Substring(AuthPayloadPrefix.Length));
                return !string.IsNullOrEmpty(displayName);
            }
            catch
            {
                return false;
            }
        }

        public static void RegisterClientHandler(NetworkClient client)
        {
            if (client == null && NetworkClient.allClients.Count > 0)
                client = NetworkClient.allClients[0];

            if (client == null || ReferenceEquals(client, _registeredClient))
                return;

            client.RegisterHandler(DisplayNameMessageType, OnDisplayNameMessage);
            _registeredClient = client;
            Plugin.Log?.LogInfo("Registered direct-connect display-name message handler.");
        }

        public static void Remember(ulong platformId, string displayName, bool broadcast)
        {
            if (platformId == 0UL)
                return;

            displayName = SanitizeDisplayName(displayName);
            if (string.IsNullOrEmpty(displayName))
                displayName = "Player_" + (platformId % 10000UL).ToString("D4");

            DisplayNames[platformId] = displayName;

            if (broadcast && NetworkServer.active)
                BroadcastAll();
        }

        public static bool TryGetDisplayName(NetworkUserId id, out string displayName)
        {
            return DisplayNames.TryGetValue(id.value, out displayName);
        }

        public static void BroadcastAll()
        {
            if (!NetworkServer.active)
                return;

            foreach (NetworkConnection connection in NetworkServer.connections)
            {
                if (connection == null || !connection.isConnected)
                    continue;

                foreach (KeyValuePair<ulong, string> pair in DisplayNames)
                {
                    connection.Send(DisplayNameMessageType, new DisplayNameMessage
                    {
                        platformId = pair.Key,
                        displayName = pair.Value
                    });
                }
            }
        }

        public static void Clear()
        {
            DisplayNames.Clear();
            _registeredClient = null;
        }

        private static void OnDisplayNameMessage(NetworkMessage netMessage)
        {
            DisplayNameMessage message = netMessage.ReadMessage<DisplayNameMessage>();
            Remember(message.platformId, message.displayName, broadcast: false);

            foreach (NetworkUser networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (networkUser.id.value == message.platformId)
                    networkUser.UpdateUserName();
            }
        }

        private static string SanitizeDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder builder = new StringBuilder(MaxDisplayNameLength);
            foreach (char character in value.Trim())
            {
                if (!char.IsControl(character))
                    builder.Append(character);

                if (builder.Length >= MaxDisplayNameLength)
                    break;
            }

            return builder.ToString();
        }
    }
}

