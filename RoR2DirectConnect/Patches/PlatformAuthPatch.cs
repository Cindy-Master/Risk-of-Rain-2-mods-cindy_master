using System;
using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// 使用持久化数字 PlatformID 进行认证，昵称通过独立载荷同步。
    /// 数字 ID 即使昵称相同也不会冲突，同时原版玩家创建逻辑可安全回退。
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "PlatformAuth")]
    public static class PlatformAuthPatch
    {
        static bool Prefix(ref ClientAuthData authData, NetworkConnection conn)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            if (conn is SteamNetworkConnection)
                return true;

            ulong platformId = DirectConnectIdentity.GetOrCreatePlatformId();
            string displayName = DirectConnectIdentity.GetLocalDisplayName(platformId);

            authData.platformId = new RoR2.PlatformID(platformId);
            authData.authTicket = DirectConnectIdentity.CreateAuthPayload(displayName);

            try
            {
                authData.entitlements = RoR2.PlatformSystems.entitlementsSystem
                    .BuildEntitlements();
            }
            catch
            {
                authData.entitlements = Array.Empty<string>();
            }

            DirectConnectIdentity.RegisterClientHandler(NetworkManagerSystem.singleton?.client);
            DirectConnectIdentity.Remember(platformId, displayName, broadcast: false);

            Plugin.Log.LogInfo(
                $"Direct connect auth: platformId={platformId}, name=\"{displayName}\", " +
                $"connection={conn?.connectionId}");
            return false;
        }
    }

    /// <summary>
    /// 保证服务端生成有效的 NetworkUserId，并从认证载荷中提取昵称。
    /// 即使后续原版实现再次调整，也不再把字符串身份退化成数值 0。
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "AddPlayerIdFromPlatform")]
    public static class AddPlayerIdFromPlatformPatch
    {
        static bool Prefix(NetworkConnection conn, byte playerControllerId,
            ref NetworkUserId __result)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            ClientAuthData authData = ServerAuthManager.FindAuthData(conn);
            if (authData != null && authData.platformId.ID != 0UL)
            {
                ulong platformId = authData.platformId.ID;
                __result = NetworkUserId.FromId(platformId, playerControllerId);

                string displayName;
                if (!DirectConnectIdentity.TryReadDisplayName(authData.authTicket, out displayName))
                    displayName = "Player_" + (platformId % 10000UL).ToString("D4");

                DirectConnectIdentity.Remember(platformId, displayName, broadcast: true);
                Plugin.Log.LogInfo(
                    $"Direct connect player identity: connection={conn?.connectionId}, " +
                    $"platformId={platformId}, controller={playerControllerId}, name=\"{displayName}\"");
            }
            else
            {
                __result = NetworkUserId.FromIp(conn.address, playerControllerId);
                Plugin.Log.LogError(
                    $"Missing direct-connect auth identity for connection {conn?.connectionId}; " +
                    "falling back to address-based ID.");
            }
            return false;
        }
    }
}
