using HarmonyLib;
using RoR2;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// For local player: override nameOverride with config value.
    /// For all DC players: ensure playerId is serialization-safe.
    /// </summary>
    [HarmonyPatch(typeof(NetworkUser), nameof(NetworkUser.GetNetworkPlayerName))]
    public static class DisplayNamePatch
    {
        static void Postfix(NetworkUser __instance, ref NetworkPlayerName __result)
        {
            if (!Plugin.DirectConnectActive)
                return;

            if (__instance.isLocalPlayer)
            {
                string localName = Plugin.ConfigDisplayName.Value;
                if (!string.IsNullOrEmpty(localName))
                    __result.nameOverride = localName;
            }

            if (__result.nameOverride != null)
            {
                __result = new NetworkPlayerName
                {
                    nameOverride = __result.nameOverride,
                    playerId = new PlatformID(0UL)
                };
            }
        }
    }

    /// <summary>
    /// Patches GetResolvedName to return nameOverride directly.
    /// Without this, the original calls EOSLobbyManager.GetUserDisplayNameFromProductIdString
    /// which treats our display name as an EOS Product User ID → returns hex GUID.
    /// </summary>
    [HarmonyPatch(typeof(NetworkPlayerName), nameof(NetworkPlayerName.GetResolvedName))]
    public static class ResolveNamePatch
    {
        static bool Prefix(ref NetworkPlayerName __instance, ref string __result)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            if (!string.IsNullOrEmpty(__instance.nameOverride))
            {
                __result = __instance.nameOverride;
                return false;
            }

            ulong idVal = (ulong)__instance.playerId.value;
            __result = idVal != 0 ? "Player_" + (idVal % 10000) : "Player";
            return false;
        }
    }

    /// <summary>
    /// Belt-and-suspenders: directly set NetworkUser.userName after UpdateUserName runs.
    ///
    /// UpdateUserName does: userName = GetNetworkPlayerName().GetResolvedName()
    /// Our ResolveNamePatch should intercept GetResolvedName, but if it fails
    /// (struct method patching edge case, timing issue, etc.), the original
    /// GetResolvedName calls EOS lookup → returns hex GUID instead of the name.
    ///
    /// This Postfix guarantees userName is set from nameOverride directly,
    /// bypassing the entire GetResolvedName → EOS chain.
    /// </summary>
    [HarmonyPatch(typeof(NetworkUser), "UpdateUserName")]
    public static class UpdateUserNamePatch
    {
        static void Postfix(NetworkUser __instance)
        {
            if (!Plugin.DirectConnectActive)
                return;

            var playerName = __instance.GetNetworkPlayerName();
            if (!string.IsNullOrEmpty(playerName.nameOverride))
            {
                __instance.userName = playerName.nameOverride;
            }
        }
    }
}
