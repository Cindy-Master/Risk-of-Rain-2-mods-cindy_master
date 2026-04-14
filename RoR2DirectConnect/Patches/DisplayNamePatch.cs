using HarmonyLib;
using RoR2;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Overrides GetNetworkPlayerName for the local player to inject
    /// the display name from config.
    ///
    /// MUST be a Prefix returning a complete NetworkPlayerName, NOT a Postfix
    /// that modifies nameOverride alone. Reason: when nameOverride is set and
    /// playerId is default(PlatformID), NetworkPlayerName.Serialize crashes
    /// because default(PlatformID).stringID is null.
    ///
    /// By constructing a full struct with PlatformID(ulong) as playerId,
    /// serialization works correctly (stringID = "" not null).
    /// </summary>
    [HarmonyPatch(typeof(NetworkUser), nameof(NetworkUser.GetNetworkPlayerName))]
    public static class DisplayNamePatch
    {
        static bool Prefix(NetworkUser __instance, ref NetworkPlayerName __result)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            string displayName = Plugin.ConfigDisplayName.Value;
            if (string.IsNullOrEmpty(displayName))
                return true;

            // Build a complete, serialization-safe NetworkPlayerName.
            // PlatformID(ulong) constructor sets stringID = "" (not null),
            // so Serialize won't NullRef.
            ulong idVal = 0UL;
            try { idVal = (ulong)__instance.id.value; } catch { }
            if (idVal == 0UL)
            {
                ulong.TryParse(Plugin.ConfigPlatformId.Value, out idVal);
            }

            __result = new NetworkPlayerName
            {
                nameOverride = displayName,
                playerId = new PlatformID(idVal)
            };
            return false;
        }
    }

    /// <summary>
    /// Patches GetResolvedName to return the display name directly.
    /// Without this, the original code calls EOS/Steam API → crash or "???".
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
