using System;
using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Uses string-based PlatformID to carry the display name through auth.
    /// PlatformID(string) → server receives it in ClientAuthData.
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

            string displayName = Plugin.ConfigDisplayName.Value;
            if (string.IsNullOrEmpty(displayName))
                displayName = "Player_" + UnityEngine.Random.Range(1000, 9999);

            authData.platformId = new RoR2.PlatformID(displayName);
            authData.authTicket = new byte[] { 0 };
            authData.entitlements = Array.Empty<string>();

            Plugin.Log.LogInfo($"Direct connect auth: name=\"{displayName}\"");
            return false;
        }
    }

    /// <summary>
    /// Fixes AddPlayerIdFromPlatform to preserve string-based PlatformIDs.
    ///
    /// The original code does:
    ///   NetworkUserId.FromId(platformID.ID, subId)
    /// which ONLY keeps the ulong, discarding stringID entirely.
    /// For PlatformID(string), ID=0, so the name is lost and value=0
    /// causes PlatformID.get_value() to return null → Serialize NullRef.
    ///
    /// Fix: use new NetworkUserId(platformId, subId) which preserves
    /// both ulong (Steam) and string (DC) PlatformIDs.
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
            if (authData != null && authData.platformId != RoR2.PlatformID.nil)
            {
                // Use the full constructor that preserves string PlatformIDs
                __result = new NetworkUserId(authData.platformId, playerControllerId);
            }
            else
            {
                __result = NetworkUserId.FromIp(conn.address, playerControllerId);
            }
            return false;
        }
    }
}
