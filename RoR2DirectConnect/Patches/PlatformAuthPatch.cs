using System;
using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Patches PlatformAuth to use a custom PlatformID instead of Steam auth.
    /// This allows direct IP connections without Steam authentication.
    ///
    /// MUST use PlatformID(ulong), NOT PlatformID(string).
    /// PlatformID(string) causes GetNetworkPlayerName to return
    /// playerId = default(PlatformID) whose stringID is null →
    /// NetworkPlayerName.Serialize crashes with NullReferenceException.
    /// </summary>
    [HarmonyPatch]
    public static class PlatformAuthPatch
    {
        static Type TargetType()
        {
            return typeof(NetworkManagerSystemSteam);
        }

        [HarmonyPatch(typeof(NetworkManagerSystemSteam), "PlatformAuth")]
        [HarmonyPrefix]
        static bool PlatformAuthPrefix(ref ClientAuthData authData, NetworkConnection conn)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            if (conn is SteamNetworkConnection)
                return true;

            ulong customId = 76561198000000000UL
                + (ulong)(UnityEngine.Random.Range(1, int.MaxValue));
            Plugin.ConfigPlatformId.Value = customId.ToString();

            authData.platformId = new RoR2.PlatformID(customId);
            authData.authTicket = new byte[] { 0 };
            authData.entitlements = Array.Empty<string>();

            Plugin.Log.LogInfo($"Direct connect auth: PlatformID {customId}");
            return false;
        }
    }
}
