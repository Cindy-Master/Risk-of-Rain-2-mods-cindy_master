using HarmonyLib;
using RoR2;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// For local player only: override nameOverride with the freshest
    /// config value (user may change nickname between sessions).
    /// Remote players get their name from auth → NetworkUserId.strValue
    /// → GetNetworkPlayerName().nameOverride automatically.
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

            // Safety: if nameOverride is set, ensure playerId is serialization-safe.
            // Original code returns playerId = default(PlatformID) for string-mode IDs,
            // whose stringID = null. PlatformID(0UL) has stringID = "" → safe.
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
    /// Returns the display name directly, bypassing EOS/Steam API lookup.
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
}
