using System;
using Facepunch.Steamworks;
using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Bypasses InitPlatformServer to skip SteamworksServerManager creation.
    /// Without this, hosting creates a Steamworks game server that requires
    /// Steam backend connectivity and will hang/crash without Steam.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "InitPlatformServer")]
    public static class InitPlatformServerPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            Plugin.Log.LogInfo("Skipped SteamworksServerManager.StartServer() and InitP2P() — direct connect mode.");
            return false;
        }
    }

    /// <summary>
    /// Bypasses InitP2P to skip Steam P2P callback registration.
    /// InitP2P sets up Steamworks P2P networking callbacks and requires
    /// Client.Instance to be valid. For direct IP this is unnecessary.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "InitP2P")]
    public static class InitP2PPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            Plugin.Log.LogInfo("Skipped Steam P2P initialization — direct connect mode.");
            return false;
        }
    }

    // OnStartClient patch REMOVED — was causing StackOverflowException due to
    // MethodInfo.Invoke using virtual dispatch → infinite recursion.
    // Not needed: NetworkManagerSystemSteam.OnStartClient just calls
    // base.OnStartClient() + InitP2P(). InitP2P is already skipped by InitP2PPatch.

    /// <summary>
    /// Patches internal SteamworksServerManager and ServerManagerBase via reflection.
    /// These are internal classes so we can't use typeof() — must use TargetMethod().
    /// Blocks: Steam server creation, auth callbacks, auth data processing.
    /// </summary>
    public static class SteamServerPatches
    {
        private static readonly Type _steamServerMgrType =
            AccessTools.TypeByName("RoR2.SteamworksServerManager");

        private static readonly Type _serverManagerBaseType =
            _steamServerMgrType?.BaseType; // ServerManagerBase<SteamworksServerManager>

        public static void Apply(Harmony harmony)
        {
            if (_steamServerMgrType == null)
            {
                Plugin.Log.LogWarning("SteamworksServerManager type not found — skipping patches.");
                return;
            }

            // Patch ServerManagerBase<SteamworksServerManager>.StartServer
            if (_serverManagerBaseType != null)
            {
                var startServer = AccessTools.Method(_serverManagerBaseType, "StartServer");
                if (startServer != null)
                {
                    harmony.Patch(startServer,
                        prefix: new HarmonyMethod(typeof(SteamServerPatches), nameof(SkipIfBypassed)));
                }
            }

            // Patch SteamworksServerManager.OnAuthChange
            var onAuthChange = AccessTools.Method(_steamServerMgrType, "OnAuthChange");
            if (onAuthChange != null)
            {
                harmony.Patch(onAuthChange,
                    prefix: new HarmonyMethod(typeof(SteamServerPatches), nameof(SkipIfBypassed)));
            }

            // Patch SteamworksServerManager.OnAuthDataReceivedFromClient
            var onAuthData = AccessTools.Method(_steamServerMgrType, "OnAuthDataReceivedFromClient");
            if (onAuthData != null)
            {
                harmony.Patch(onAuthData,
                    prefix: new HarmonyMethod(typeof(SteamServerPatches), nameof(SkipIfBypassed)));
            }

            Plugin.Log.LogInfo("Steam server patches applied via reflection.");
        }

        public static bool SkipIfBypassed()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Prevents SteamworksLobbyManager.CreateLobby from calling Steam API.
    /// Instead of calling OnLobbyCreated (which reads client.Lobby.IsValid → NullRef),
    /// we directly set internal fields via reflection.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "CreateLobby")]
    public static class CreateLobbyPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            FakeLobbyState();
            Plugin.Log.LogInfo("Faked lobby state for direct connect (skipped Steam CreateLobby).");
            return false;
        }

        /// <summary>
        /// Sets internal lobby fields via reflection so the game thinks we're
        /// in a valid lobby owned by us. Avoids ALL Steam API calls.
        /// Called from CreateLobbyPatch and DirectConnectPanel.
        /// </summary>
        public static void FakeLobbyState()
        {
            var lobbyMgr = PlatformSystems.lobbyManager;
            if (lobbyMgr == null) return;

            var actualType = lobbyMgr.GetType(); // SteamworksLobbyManager

            // isInLobbyDelayed — internal field, some UI code reads this directly
            AccessTools.Field(actualType, "isInLobbyDelayed")
                ?.SetValue(lobbyMgr, true);

            // _ownsLobby — internal field (don't use set_ownsLobby to avoid
            // OnLobbyOwnershipGained which calls SetStartingIfOwner → Steam API)
            AccessTools.Field(actualType, "_ownsLobby")
                ?.SetValue(lobbyMgr, true);

            // awaitingCreate / awaitingJoin — clear waiting flags
            AccessTools.Property(typeof(LobbyManager), "awaitingCreate")
                ?.SetValue(lobbyMgr, false);
            AccessTools.Property(typeof(LobbyManager), "awaitingJoin")
                ?.SetValue(lobbyMgr, false);

            Plugin.Log.LogInfo("FakeLobbyState: isInLobbyDelayed=true, _ownsLobby=true");
        }

        /// <summary>
        /// Resets the fake lobby state after a DC session ends.
        /// Without this, stale isInLobbyDelayed/ownsLobby flags break
        /// normal hosting — the lobby state machine thinks we're already
        /// in an owned lobby and won't start a new one.
        /// </summary>
        public static void CleanupFakeLobbyState()
        {
            var lobbyMgr = PlatformSystems.lobbyManager;
            if (lobbyMgr == null) return;

            var actualType = lobbyMgr.GetType();
            AccessTools.Field(actualType, "isInLobbyDelayed")
                ?.SetValue(lobbyMgr, false);
            AccessTools.Field(actualType, "_ownsLobby")
                ?.SetValue(lobbyMgr, false);

            Plugin.Log.LogInfo("Cleaned up fake lobby state.");
        }
    }

    /// <summary>
    /// Blocks all Steam lobby join attempts in direct connect mode.
    /// After IP connect, the game tries to join the host's Steam lobby →
    /// fails → shows "server full" popup in a loop. Block at the source.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "JoinLobby", new Type[] { typeof(RoR2.PlatformID) })]
    public static class JoinLobbyPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            Plugin.Log.LogInfo("Blocked Steam lobby join (direct connect mode).");
            return false;
        }
    }

    /// <summary>
    /// Safety net: if OnLobbyJoined fires with failure in DC mode,
    /// suppress the "server full" popup dialog.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "OnLobbyJoined", new Type[] { typeof(bool) })]
    public static class OnLobbyJoinedPatch
    {
        static bool Prefix(bool success)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            if (!success)
            {
                Plugin.Log.LogInfo("Suppressed lobby join failure (direct connect mode).");
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Replaces SteamworksLobbyManager.OnStopClient in direct-connect mode.
    /// The original method calls client.Lobby.CurrentLobbyData.RemoveData()
    /// which NullRefs without a real lobby session. But we MUST still call
    /// LeaveLobby() — otherwise the real Steam lobby (created during menu
    /// init) is never left, and the game detects the user is still in a
    /// lobby → forces them back to the multiplayer menu on every "Back" click.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "OnStopClient")]
    public static class SteamLobbyOnStopClientPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            try
            {
                var lobbyMgr = PlatformSystems.lobbyManager;
                if (lobbyMgr != null)
                {
                    AccessTools.Method(lobbyMgr.GetType(), "LeaveLobby")
                        ?.Invoke(lobbyMgr, null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"LeaveLobby failed: {ex.Message}");
            }

            Plugin.Log.LogInfo("Left Steam lobby, skipped rest of OnStopClient (direct connect).");
            return false;
        }
    }

    /// <summary>
    /// In direct connect mode, always report as "in lobby" so the
    /// game's UI state machine doesn't get stuck.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "get_isInLobby")]
    public static class IsInLobbyPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            __result = true;
            return false;
        }
    }

    /// <summary>
    /// In direct connect mode, always report as lobby owner.
    /// </summary>
    [HarmonyPatch(typeof(SteamworksLobbyManager), "get_ownsLobby")]
    public static class OwnsLobbyPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            __result = true;
            return false;
        }
    }

    /// <summary>
    /// In direct connect mode, the original EnsureDesiredHost handles HostType.Self
    /// (hosting) and HostType.IPv4 (IP connect) correctly — no Steam API involved.
    /// We only need to block HostType.Steam (P2P) connections.
    /// All Steam-specific init (InitPlatformServer, InitP2P, OnStartClient) is
    /// already handled by our other patches.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "EnsureDesiredHost")]
    public static class EnsureDesiredHostPatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            // Block Steam P2P connections in direct connect mode.
            // Self (hosting) and IPv4 (IP connect) are safe to let through.
            var singleton = NetworkManagerSystem.singleton;
            if (singleton != null
                && singleton.desiredHost.hostType == HostDescription.HostType.Steam)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// OnStartPrivateGame calls CreateLobby — we let it through.
    /// CreateLobbyPatch will fake the lobby creation.
    /// No need to block this anymore.
    /// </summary>

    /// <summary>
    /// Guards SteamLobbyFinder.Update from running when Steam is bypassed.
    /// SteamLobbyFinder continuously queries Steam for lobby lists.
    /// </summary>
    [HarmonyPatch(typeof(SteamLobbyFinder), "Update")]
    public static class SteamLobbyFinderUpdatePatch
    {
        static bool Prefix()
        {
            if (!Plugin.DirectConnectActive)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Resets DirectConnectActive when the client stops, so normal game
    /// functions are restored after leaving a direct-connect session.
    /// Also cleans up fake lobby state — without this, the stale
    /// isInLobbyDelayed/ownsLobby flags break normal hosting afterwards.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystem), "OnStopClient")]
    public static class ResetOnStopClient
    {
        static void Postfix()
        {
            if (Plugin.DirectConnectActive)
            {
                Plugin.DirectConnectActive = false;
                CreateLobbyPatch.CleanupFakeLobbyState();
                Plugin.Log.LogInfo("DirectConnectActive reset (OnStopClient).");
            }
        }
    }

    /// <summary>
    /// Resets DirectConnectActive when the host stops, so normal game
    /// functions are restored after shutting down a direct-connect server.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystem), "OnStopHost")]
    public static class ResetOnStopHost
    {
        static void Postfix()
        {
            if (Plugin.DirectConnectActive)
            {
                Plugin.DirectConnectActive = false;
                CreateLobbyPatch.CleanupFakeLobbyState();
                Plugin.Log.LogInfo("DirectConnectActive reset (OnStopHost).");
            }
        }
    }

}
