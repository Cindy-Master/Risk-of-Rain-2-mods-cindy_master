using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;

namespace RoR2DirectConnect
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.ror2.directconnect";
        public const string PluginName = "RoR2DirectConnect";
        public const string PluginVersion = "2.0.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        public static ConfigEntry<string> ConfigPlatformId;
        public static ConfigEntry<string> ConfigDisplayName;
        public static ConfigEntry<bool> ConfigBypassSteamAuth;
        public static ConfigEntry<bool> ConfigBypassLobbyCheck;

        // Saved UI fields
        public static ConfigEntry<string> ConfigLastAddress;
        public static ConfigEntry<string> ConfigLastPort;
        public static ConfigEntry<string> ConfigLastMaxPlayers;
        public static ConfigEntry<string> ConfigLastHostPassword;
        public static ConfigEntry<string> ConfigLastClientPassword;

        /// <summary>
        /// Runtime flag — only true when user clicks Host/Connect in the Direct Connect panel.
        /// All Steam bypass patches check this. Normal game modes work when this is false.
        /// </summary>
        public static bool DirectConnectActive { get; set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            ConfigPlatformId = Config.Bind(
                "General", "PlatformId", "76561198000000000",
                "Custom Platform ID for direct connect. Each player must use a unique ID."
            );

            ConfigDisplayName = Config.Bind(
                "General", "DisplayName", "",
                "Your in-game display name for direct connect. If empty, falls back to profile name."
            );

            ConfigBypassSteamAuth = Config.Bind(
                "General", "BypassSteamAuth", true,
                "Bypass Steam authentication. Required for LAN/tunneling play."
            );

            ConfigBypassLobbyCheck = Config.Bind(
                "General", "BypassLobbyCheck", true,
                "Bypass Steam lobby check when hosting. Required for direct IP hosting."
            );

            ConfigLastAddress = Config.Bind("SavedUI", "LastAddress", "",
                "Last used server address.");
            ConfigLastPort = Config.Bind("SavedUI", "LastPort", "7777",
                "Last used port.");
            ConfigLastMaxPlayers = Config.Bind("SavedUI", "LastMaxPlayers", "4",
                "Last used max players.");
            ConfigLastHostPassword = Config.Bind("SavedUI", "LastHostPassword", "",
                "Last used host password.");
            ConfigLastClientPassword = Config.Bind("SavedUI", "LastClientPassword", "",
                "Last used client password.");

            _harmony = new Harmony(PluginGUID);

            // IPv6 address parsing
            _harmony.PatchAll(typeof(Patches.AddressPortPairPatch));

            // Auth & host bypass
            _harmony.PatchAll(typeof(Patches.PlatformAuthPatch));
            _harmony.PatchAll(typeof(Patches.PlatformHostPatch));
            _harmony.PatchAll(typeof(Patches.UpdateServerPatch));

            // Full Steam bypass
            _harmony.PatchAll(typeof(Patches.InitPlatformServerPatch));
            _harmony.PatchAll(typeof(Patches.InitP2PPatch));
            // OnStartClientSteamPatch removed — InitP2PPatch handles it
            Patches.SteamServerPatches.Apply(_harmony);
            _harmony.PatchAll(typeof(Patches.CreateLobbyPatch));
            _harmony.PatchAll(typeof(Patches.JoinLobbyPatch));
            _harmony.PatchAll(typeof(Patches.OnLobbyJoinedPatch));
            _harmony.PatchAll(typeof(Patches.IsInLobbyPatch));
            _harmony.PatchAll(typeof(Patches.OwnsLobbyPatch));
            _harmony.PatchAll(typeof(Patches.SteamLobbyOnStopClientPatch));
            _harmony.PatchAll(typeof(Patches.EnsureDesiredHostPatch));
            _harmony.PatchAll(typeof(Patches.SteamLobbyFinderUpdatePatch));

            // Reset DirectConnectActive on disconnect
            _harmony.PatchAll(typeof(Patches.ResetOnStopClient));
            _harmony.PatchAll(typeof(Patches.ResetOnStopHost));

            // Display name
            _harmony.PatchAll(typeof(Patches.DisplayNamePatch));
            _harmony.PatchAll(typeof(Patches.ResolveNamePatch));

            // Localization (must be before UI injection)
            RoR2.Language.onCurrentLanguageChanged += Localization.RegisterAll;
            RoR2Application.onLoad += Localization.RegisterAll;

            // Native UI injection into multiplayer menu
            _harmony.PatchAll(typeof(UI.MenuInjector));

            // Console commands (kept as fallback)
            _harmony.PatchAll(typeof(Patches.DirectConnectCommands));

            // Keep IMGUI as F6 fallback overlay
            gameObject.AddComponent<DirectConnectUI>();

            Log.LogInfo("RoR2DirectConnect v2.0 loaded! Native UI injected into Multiplayer menu.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
