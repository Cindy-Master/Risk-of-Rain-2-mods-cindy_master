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
            object val = __instance.playerId.value;

            if (!Plugin.DirectConnectActive)
            {
                // Not in DC mode: only intervene to prevent a hard crash.
                // The original GetResolvedName does (ulong)playerId.value style
                // access and throws on a nil PlatformID (value == null). This
                // happens after OnStopHost resets the flag while UI still renders
                // our leftover names → NullRef → black screen.
                if (val == null)
                {
                    __result = !string.IsNullOrEmpty(__instance.nameOverride)
                        ? __instance.nameOverride : "Player";
                    return false;
                }
                return true; // real Steam/EOS id — let original handle it
            }

            // DC mode: our display name lives in nameOverride.
            if (!string.IsNullOrEmpty(__instance.nameOverride))
            {
                __result = __instance.nameOverride;
                return false;
            }

            if (val is ulong)
            {
                ulong idVal = (ulong)val;
                __result = idVal != 0 ? "Player_" + (idVal % 10000) : "Player";
                return false;
            }
            if (val is string && !string.IsNullOrEmpty((string)val))
            {
                __result = (string)val;
                return false;
            }
            __result = "Player";
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
