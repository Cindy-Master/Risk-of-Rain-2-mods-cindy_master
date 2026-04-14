using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Registers new console commands for direct connect functionality:
    ///   dc_connect [ip]:port  — connect to a host via IPv4 or IPv6
    ///   dc_host [maxplayers]  — start a direct-connect host (no lobby)
    ///   dc_id [newid]         — get or set your PlatformID
    ///   dc_status             — show current connection info
    /// </summary>
    public static class DirectConnectCommands
    {
        [ConCommand(commandName = "dc_connect", flags = ConVarFlags.None,
            helpText = "Direct connect to IP. Usage: dc_connect ip:port OR dc_connect [ipv6]:port")]
        private static void CCDirectConnect(ConCommandArgs args)
        {
            string target = args.GetArgString(0);
            if (string.IsNullOrEmpty(target))
            {
                Debug.Log("[DirectConnect] Usage: dc_connect ip:port  OR  dc_connect [::1]:7777");
                return;
            }

            if (!NetworkManagerSystem.singleton)
            {
                Debug.Log("[DirectConnect] NetworkManager not ready.");
                return;
            }

            AddressPortPair pair;
            if (!AddressPortPair.TryParse(target, out pair) || !pair.isValid)
            {
                Debug.Log($"[DirectConnect] Failed to parse address: {target}");
                Debug.Log("[DirectConnect] IPv4 format: 192.168.1.1:7777");
                Debug.Log("[DirectConnect] IPv6 format: [::1]:7777 or [2001:db8::1]:7777");
                return;
            }

            Plugin.DirectConnectActive = true;
            AccessTools.Method(typeof(NetworkManagerSystem), "EnsureNetworkManagerNotBusy")
                .Invoke(null, null);
            NetworkManagerSystem.singleton.desiredHost = new HostDescription(pair);

            Debug.Log($"[DirectConnect] Connecting to {pair.address}:{pair.port} ...");
        }

        [ConCommand(commandName = "dc_host", flags = ConVarFlags.None,
            helpText = "Start a direct-connect host. Usage: dc_host [maxplayers]")]
        private static void CCDirectHost(ConCommandArgs args)
        {
            if (!NetworkManagerSystem.singleton)
            {
                Debug.Log("[DirectConnect] NetworkManager not ready.");
                return;
            }

            if (UnityEngine.Networking.NetworkServer.active)
            {
                Debug.Log("[DirectConnect] Server is already running.");
                return;
            }

            int maxPlayers = 4;
            if (args.Count > 0)
            {
                int parsed = args.GetArgInt(0);
                if (parsed > 0 && parsed <= 16)
                    maxPlayers = parsed;
            }

            Plugin.DirectConnectActive = true;
            NetworkManagerSystem.singleton.desiredHost = new HostDescription(
                new HostDescription.HostingParameters
                {
                    listen = true,
                    maxPlayers = maxPlayers
                }
            );

            Debug.Log($"[DirectConnect] Hosting with maxPlayers={maxPlayers}. " +
                       "Clients can connect with: dc_connect YOUR_IP:7777");
        }

        [ConCommand(commandName = "dc_id", flags = ConVarFlags.None,
            helpText = "Get or set your PlatformID. Usage: dc_id [newid]")]
        private static void CCDirectId(ConCommandArgs args)
        {
            if (args.Count > 0)
            {
                string newId = args.GetArgString(0);
                ulong parsed;
                if (ulong.TryParse(newId, out parsed))
                {
                    Plugin.ConfigPlatformId.Value = newId;
                    Debug.Log($"[DirectConnect] PlatformID set to: {newId}");
                }
                else
                {
                    Debug.Log($"[DirectConnect] Invalid ID. Must be a number. Example: 76561198012345678");
                }
            }
            else
            {
                Debug.Log($"[DirectConnect] Current PlatformID: {Plugin.ConfigPlatformId.Value}");
            }
        }

        [ConCommand(commandName = "dc_status", flags = ConVarFlags.None,
            helpText = "Show direct connect status info.")]
        private static void CCDirectStatus(ConCommandArgs args)
        {
            Debug.Log("=== DirectConnect Status ===");
            Debug.Log($"  PlatformID:        {Plugin.ConfigPlatformId.Value}");
            Debug.Log($"  BypassSteamAuth:   {Plugin.ConfigBypassSteamAuth.Value}");
            Debug.Log($"  BypassLobbyCheck:  {Plugin.ConfigBypassLobbyCheck.Value}");
            Debug.Log($"  Server active:     {UnityEngine.Networking.NetworkServer.active}");
            Debug.Log($"  Client active:     {UnityEngine.Networking.NetworkClient.active}");

            if (NetworkManagerSystem.singleton)
            {
                var host = NetworkManagerSystem.singleton.desiredHost;
                Debug.Log($"  DesiredHost type:  {host.hostType}");
                if (host.hostType == HostDescription.HostType.IPv4)
                {
                    Debug.Log($"  Target address:    {host.addressPortPair.address}:{host.addressPortPair.port}");
                }
            }

            if (UnityEngine.Networking.NetworkServer.active)
            {
                Debug.Log($"  Connections:       {UnityEngine.Networking.NetworkServer.connections.Count}");
                Debug.Log($"  Listen port:       {NetworkManagerSystem.singleton.networkPort}");
            }
            Debug.Log("============================");
        }
    }
}
