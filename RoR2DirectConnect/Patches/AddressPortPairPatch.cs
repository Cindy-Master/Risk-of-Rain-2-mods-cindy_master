using HarmonyLib;
using RoR2.Networking;

namespace RoR2DirectConnect.Patches
{
    /// <summary>
    /// Patches AddressPortPair.TryParse to support IPv6 addresses.
    /// IPv6 format: [::1]:7777 or [2001:db8::1]:7777
    /// IPv4 format: 192.168.1.1:7777 (unchanged)
    /// </summary>
    [HarmonyPatch(typeof(AddressPortPair), nameof(AddressPortPair.TryParse))]
    public static class AddressPortPairPatch
    {
        static bool Prefix(string str, out AddressPortPair addressPortPair, ref bool __result)
        {
            addressPortPair = default;

            if (string.IsNullOrEmpty(str))
            {
                __result = false;
                return false;
            }

            // Handle [IPv6]:port format
            if (str.StartsWith("["))
            {
                int bracketEnd = str.IndexOf(']');
                if (bracketEnd < 0)
                {
                    __result = false;
                    return false;
                }

                // Extract IPv6 address without brackets
                string ipv6Address = str.Substring(1, bracketEnd - 1);

                // Default port
                ushort port = 7777;

                // Check for :port after the closing bracket
                if (bracketEnd + 1 < str.Length && str[bracketEnd + 1] == ':')
                {
                    string portStr = str.Substring(bracketEnd + 2);
                    if (!ushort.TryParse(portStr, out port))
                    {
                        port = 7777;
                    }
                }

                addressPortPair.address = ipv6Address;
                addressPortPair.port = port;
                __result = true;

                Plugin.Log.LogInfo($"Parsed IPv6 address: [{ipv6Address}]:{port}");
                return false;
            }

            // For non-bracketed addresses, count colons to detect bare IPv6
            int colonCount = 0;
            foreach (char c in str)
            {
                if (c == ':') colonCount++;
            }

            // More than 1 colon and no brackets = bare IPv6 (no port specified)
            if (colonCount > 1)
            {
                addressPortPair.address = str;
                addressPortPair.port = 7777;
                __result = true;

                Plugin.Log.LogInfo($"Parsed bare IPv6 address: {str}:7777 (default port)");
                return false;
            }

            // IPv4 or hostname:port — let original method handle it
            return true;
        }
    }
}
