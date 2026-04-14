using System;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using RoR2.UI.MainMenu;
using TMPro;
using UnityEngine;

namespace RoR2DirectConnect.UI
{
    [HarmonyPatch]
    public static class MenuInjector
    {
        private static GameObject _directConnectMenu;
        private static GameObject _directConnectButton;

        public static GameObject ButtonTemplate { get; private set; }

        [HarmonyPatch(typeof(MultiplayerMenuController), "OnEnable")]
        [HarmonyPostfix]
        static void OnEnablePostfix(MultiplayerMenuController __instance)
        {
            try
            {
                Inject(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"MenuInjector: {ex}");
            }
        }

        [HarmonyPatch(typeof(MultiplayerMenuController), "SetSubview")]
        [HarmonyPostfix]
        static void SetSubviewPostfix()
        {
            if (_directConnectMenu != null)
                _directConnectMenu.SetActive(false);
        }

        private static void Inject(MultiplayerMenuController ctrl)
        {
            var hostBtnObj = ctrl.hostGame?.gameObject;
            if (hostBtnObj == null) return;

            ButtonTemplate = hostBtnObj;

            // === Ensure button exists (recreate if destroyed by scene change) ===
            if (_directConnectButton == null || !_directConnectButton)
            {
                _directConnectButton = UnityEngine.Object.Instantiate(hostBtnObj, hostBtnObj.transform.parent);
                _directConnectButton.name = "DirectConnectButton";
                _directConnectButton.transform.SetSiblingIndex(hostBtnObj.transform.GetSiblingIndex() + 1);

                foreach (var lc in _directConnectButton.GetComponentsInChildren<LanguageTextMeshController>(true))
                    UnityEngine.Object.DestroyImmediate(lc);

                foreach (var tmp in _directConnectButton.GetComponentsInChildren<TextMeshProUGUI>(true))
                    tmp.text = Localization.Get(Localization.BUTTON_DIRECT_CONNECT);
            }

            // Always refresh onClick to use current controller reference
            var hgBtn = _directConnectButton.GetComponent<HGButton>();
            if (hgBtn != null)
            {
                hgBtn.hoverLanguageTextMeshController = null;
                hgBtn.onClick.RemoveAllListeners();
                hgBtn.onClick.AddListener(() => ShowPanel(ctrl));
            }

            // === Ensure panel exists (recreate if destroyed by scene change) ===
            if (_directConnectMenu == null || !_directConnectMenu)
            {
                var hostMenu = ctrl.HostGameMenu;
                if (hostMenu == null) return;

                _directConnectMenu = UnityEngine.Object.Instantiate(hostMenu, hostMenu.transform.parent);
                _directConnectMenu.name = "DirectConnectMenu";
                _directConnectMenu.SetActive(false);

                var hgpc = _directConnectMenu.GetComponent<HostGamePanelController>();
                if (hgpc != null) UnityEngine.Object.DestroyImmediate(hgpc);

                // Strip all cloned children immediately — the cloned HostGameMenu
                // contains TMP input fields whose sub-mesh materials become stale.
                // If destroyed lazily (Destroy), TMP_SubMeshUI.OnDisable fires with
                // null materials → NullReferenceException spam on scene transitions.
                for (int i = _directConnectMenu.transform.childCount - 1; i >= 0; i--)
                    UnityEngine.Object.DestroyImmediate(_directConnectMenu.transform.GetChild(i).gameObject);

                _directConnectMenu.AddComponent<DirectConnectPanel>();
                Plugin.Log.LogInfo("Injected 'IP Direct Connect' button + panel.");
            }

            // Always update panel's controller reference (may change after scene reload)
            var dcPanel = _directConnectMenu.GetComponent<DirectConnectPanel>();
            if (dcPanel != null)
                dcPanel.menuController = ctrl;
        }

        public static void ShowPanel(MultiplayerMenuController ctrl)
        {
            // If panel was destroyed, try to recreate before hiding menus
            if (_directConnectMenu == null || !_directConnectMenu)
            {
                Plugin.Log.LogWarning("DirectConnect panel missing — recreating...");
                try { Inject(ctrl); }
                catch (Exception ex) { Plugin.Log.LogError($"Panel recreation failed: {ex}"); }
            }

            // Don't hide menus if panel is still unavailable
            if (_directConnectMenu == null || !_directConnectMenu)
            {
                Plugin.Log.LogError("Cannot show DirectConnect panel.");
                return;
            }

            ctrl.MainMultiplayerMenu.SetActive(false);
            ctrl.HostGameMenu.SetActive(false);
            ctrl.JoinGameMenu.SetActive(false);
            _directConnectMenu.SetActive(true);
        }

        public static void HidePanel(MultiplayerMenuController ctrl)
        {
            if (_directConnectMenu != null)
                _directConnectMenu.SetActive(false);

            Plugin.DirectConnectActive = false;
            ctrl.SetSubview(MultiplayerMenuController.Subview.Main);
        }

        public static void DismissPanel(MultiplayerMenuController ctrl)
        {
            if (_directConnectMenu != null)
                _directConnectMenu.SetActive(false);
        }

        public static MPButton CloneButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
        {
            if (ButtonTemplate == null) return null;

            var go = UnityEngine.Object.Instantiate(ButtonTemplate, parent);
            go.name = "Btn_" + text.Replace(" ", "");

            foreach (var lc in go.GetComponentsInChildren<LanguageTextMeshController>(true))
                UnityEngine.Object.DestroyImmediate(lc);

            foreach (var tmp in go.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.text = text;

            var hg = go.GetComponent<HGButton>();
            if (hg != null) hg.hoverLanguageTextMeshController = null;

            var btn = go.GetComponent<MPButton>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(onClick);
            }

            go.SetActive(true);
            return btn;
        }
    }
}
