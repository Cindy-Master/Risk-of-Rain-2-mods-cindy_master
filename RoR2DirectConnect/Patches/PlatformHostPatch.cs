using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Patches PlatformHost to bypass Steam lobby checks.
    /// Allows hosting a game server without being in a Steam lobby.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "PlatformHost")]
    public static class PlatformHostPatch
    {
        static bool Prefix(ConCommandArgs args)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            if (!NetworkManagerSystem.singleton)
                return false;

            bool listen = args.GetArgBool(0);

            // Skip the lobby ownership check entirely
            if (NetworkServer.active)
            {
                Plugin.Log.LogWarning("Server is already active.");
                return false;
            }

            int maxPlayers = NetworkManagerSystem.SvMaxPlayersConVar.instance.intValue;

            NetworkManagerSystem.singleton.desiredHost = new HostDescription(
                new HostDescription.HostingParameters
                {
                    listen = listen,
                    maxPlayers = maxPlayers
                }
            );

            Plugin.Log.LogInfo($"Direct host started: listen={listen}, maxPlayers={maxPlayers}");
            return false;
        }
    }

    /// <summary>
    /// Patches UpdateServer to not kick IPv4/IPv6 connections
    /// that don't have Steam P2P session state.
    /// </summary>
    [HarmonyPatch(typeof(NetworkManagerSystemSteam), "UpdateServer")]
    public static class UpdateServerPatch
    {
        static bool Prefix(NetworkManagerSystemSteam __instance)
        {
            if (!Plugin.DirectConnectActive)
                return true;

            // Custom UpdateServer that doesn't check Steam P2P state
            // for non-Steam connections
            if (!NetworkServer.active)
                return false;

            var connections = NetworkServer.connections;
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                var conn = connections[i];
                if (conn == null) continue;

                // Only check P2P state for actual Steam connections
                var steamConn = conn as SteamNetworkConnection;
                if (steamConn != null)
                {
                    // Let original logic handle Steam connections
                    var p2pState = default(Facepunch.Steamworks.Networking.P2PSessionState);
                    if (Facepunch.Steamworks.Client.Instance.Networking.GetP2PSessionState(
                            steamConn.steamId.ID, ref p2pState)
                        && p2pState.Connecting == 0
                        && p2pState.ConnectionActive == 0)
                    {
                        // Connection lost
                        AccessTools.Method(typeof(NetworkManagerSystemSteam), "ServerHandleClientDisconnect")
                            .Invoke(__instance, new object[] { steamConn });
                    }
                }
                // Non-Steam connections (IPv4/IPv6) are left alone — Unity handles them
            }

            return false;
        }
    }
}
