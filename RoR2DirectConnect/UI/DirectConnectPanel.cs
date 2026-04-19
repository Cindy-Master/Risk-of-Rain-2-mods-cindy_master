using System;
using System.Collections;
using RoR2;
using RoR2.Networking;
using RoR2.UI;
using RoR2.UI.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2DirectConnect.UI
{
    public class DirectConnectPanel : MonoBehaviour
    {
        public MultiplayerMenuController menuController;

        // HOST section
        private TMP_InputField _hostNicknameInput;
        private TMP_InputField _hostMaxPlayersInput;
        private TMP_InputField _hostPasswordInput;

        // JOIN section
        private TMP_InputField _joinNicknameInput;
        private TMP_InputField _joinAddressInput;
        private TMP_InputField _joinPortInput;
        private TMP_InputField _joinPasswordInput;

        private TextMeshProUGUI _statusLabel;

        private const float PANEL_WIDTH = 620f;

        private void OnEnable()
        {
            // DestroyImmediate to avoid TMP_SubMeshUI.OnDisable NullRef —
            // deferred Destroy lets TMP access already-null materials.
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            BuildPanel();
        }

        private void BuildPanel()
        {
            // Fill parent
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // ── Centered fixed-width container ──
            var center = new GameObject("Center", typeof(RectTransform));
            center.transform.SetParent(transform, false);
            var cRT = center.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.5f, 0f);
            cRT.anchorMax = new Vector2(0.5f, 1f);
            cRT.sizeDelta = new Vector2(PANEL_WIDTH, 0);
            cRT.anchoredPosition = Vector2.zero;

            // ── Background card (raycastTarget OFF to not block other UI) ──
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(center.transform, false);
            StretchRT(bg);
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.06f, 0.06f, 0.09f, 0.82f);
            bgImg.raycastTarget = false; // CRITICAL: don't intercept clicks

            // ── Content layout ──
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(center.transform, false);
            StretchRT(content);

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(35, 35, 25, 20);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childAlignment = TextAnchor.UpperCenter;

            var root = content.transform;

            // ═══════ TITLE ═══════
            AddLabel(root, L(Localization.TITLE), 26, FontStyles.Bold,
                TextAlignmentOptions.Center, Color.white, 42);
            AddDivider(root);
            AddSpacer(root, 4);

            // ═══════ HOST A GAME ═══════
            AddLabel(root, L(Localization.HOST_SECTION), 16, FontStyles.Bold,
                TextAlignmentOptions.Left, new Color(0.55f, 0.78f, 1f), 26);

            AddLabel(root, L(Localization.LABEL_DISPLAY_NAME), 13, FontStyles.Normal,
                TextAlignmentOptions.Left, new Color(0.90f, 0.90f, 0.95f), 20);
            _hostNicknameInput = AddInputField(root, L(Localization.PLACEHOLDER_NICKNAME),
                Plugin.ConfigDisplayName.Value);

            AddSpacer(root, 4);

            var hostRow = AddRow(root);
            AddLabel(hostRow, L(Localization.LABEL_MAX_PLAYERS), 13, FontStyles.Normal,
                TextAlignmentOptions.MidlineRight, new Color(0.90f, 0.90f, 0.95f), 32, fixedW: 90);
            _hostMaxPlayersInput = AddInputField(hostRow, "4", Plugin.ConfigLastMaxPlayers.Value, fixedW: 50);
            AddLabel(hostRow, L(Localization.LABEL_PASSWORD), 13, FontStyles.Normal,
                TextAlignmentOptions.MidlineRight, new Color(0.90f, 0.90f, 0.95f), 32, fixedW: 70);
            _hostPasswordInput = AddInputField(hostRow, L(Localization.PLACEHOLDER_PASSWORD),
                Plugin.ConfigLastHostPassword.Value, isPassword: true);

            AddSpacer(root, 4);
            CloneBtn(root, L(Localization.BTN_HOST), DoHost);

            AddSpacer(root, 8);
            AddDivider(root);
            AddSpacer(root, 8);

            // ═══════ JOIN A GAME ═══════
            AddLabel(root, L(Localization.JOIN_SECTION), 16, FontStyles.Bold,
                TextAlignmentOptions.Left, new Color(0.55f, 0.78f, 1f), 26);

            AddLabel(root, L(Localization.LABEL_DISPLAY_NAME), 13, FontStyles.Normal,
                TextAlignmentOptions.Left, new Color(0.90f, 0.90f, 0.95f), 20);
            _joinNicknameInput = AddInputField(root, L(Localization.PLACEHOLDER_NICKNAME),
                Plugin.ConfigDisplayName.Value);

            AddSpacer(root, 4);

            AddLabel(root, L(Localization.IPV_HINT), 11,
                FontStyles.Italic, TextAlignmentOptions.Left, new Color(0.70f, 0.70f, 0.75f), 16);
            var addrRow = AddRow(root);
            AddLabel(addrRow, L(Localization.LABEL_ADDRESS), 13, FontStyles.Normal,
                TextAlignmentOptions.MidlineRight, new Color(0.90f, 0.90f, 0.95f), 32, fixedW: 60);
            _joinAddressInput = AddInputField(addrRow, L(Localization.PLACEHOLDER_ADDRESS),
                Plugin.ConfigLastAddress.Value);
            AddLabel(addrRow, ":", 13, FontStyles.Normal, TextAlignmentOptions.Center,
                new Color(0.90f, 0.90f, 0.95f), 32, fixedW: 10);
            _joinPortInput = AddInputField(addrRow, "7777", Plugin.ConfigLastPort.Value, fixedW: 70);

            var jpwRow = AddRow(root);
            AddLabel(jpwRow, L(Localization.LABEL_PASSWORD), 13, FontStyles.Normal,
                TextAlignmentOptions.MidlineRight, new Color(0.90f, 0.90f, 0.95f), 32, fixedW: 60);
            _joinPasswordInput = AddInputField(jpwRow, L(Localization.PLACEHOLDER_PASSWORD),
                Plugin.ConfigLastClientPassword.Value, isPassword: true);

            AddSpacer(root, 4);
            CloneBtn(root, L(Localization.BTN_CONNECT), DoConnect);

            AddSpacer(root, 8);
            AddDivider(root);
            AddSpacer(root, 6);

            // ═══════ BACK + STATUS ═══════
            CloneBtn(root, L(Localization.BTN_BACK), () => MenuInjector.HidePanel(menuController));

            AddSpacer(root, 6);

            // Status
            _statusLabel = AddLabel(root, "", 14, FontStyles.Bold,
                TextAlignmentOptions.Center, new Color(0.5f, 0.9f, 0.6f), 22);

            // ── Flexible spacer at bottom to push content UP ──
            var bottomSpacer = new GameObject("BottomSpacer", typeof(RectTransform),
                typeof(LayoutElement));
            bottomSpacer.transform.SetParent(root, false);
            var bsLE = bottomSpacer.GetComponent<LayoutElement>();
            bsLE.flexibleHeight = 1; // eats all remaining vertical space
        }

        // ═══════ Actions ═══════

        private void DoHost()
        {
            if (!NetworkManagerSystem.singleton)
            { SetStatus("NetworkManager not ready.", Color.red); return; }

            Plugin.DirectConnectActive = true;
            SaveNickname(_hostNicknameInput);
            SetConVar("sv_password", _hostPasswordInput?.text ?? "");

            int maxPlayers = 4;
            if (_hostMaxPlayersInput != null)
            {
                int parsed;
                if (int.TryParse(_hostMaxPlayersInput.text, out parsed) && parsed > 0 && parsed <= 16)
                    maxPlayers = parsed;
            }

            SetConVar("sv_maxplayers", maxPlayers.ToString());

            // Save UI fields for next session
            Plugin.ConfigLastMaxPlayers.Value = maxPlayers.ToString();
            Plugin.ConfigLastHostPassword.Value = _hostPasswordInput?.text ?? "";

            // Fake lobby state BEFORE setting desiredHost — the game needs
            // isInLobby/ownsLobby to be true for the transition to work
            Patches.CreateLobbyPatch.FakeLobbyState();

            NetworkManagerSystem.singleton.desiredHost = new HostDescription(
                new HostDescription.HostingParameters { listen = true, maxPlayers = maxPlayers });

            Plugin.Log.LogInfo($"Direct connect host: {maxPlayers} players");

            // Dismiss panel — EnsureDesiredHost will call StartHost() next frame,
            // which triggers scene change to lobby (character select).
            // Use a coroutine as safety net in case server fails to start.
            MenuInjector.DismissPanel(menuController);
            Plugin.Instance.StartCoroutine(WaitForServerOrRestore());
        }

        private IEnumerator WaitForServerOrRestore()
        {
            float t = 0f;
            while (!NetworkServer.active && t < 5f)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (!NetworkServer.active)
            {
                Plugin.Log.LogError("Server failed to start within 5s.");
                Plugin.DirectConnectActive = false;
                if (menuController != null)
                    MenuInjector.ShowPanel(menuController);
            }
            else
            {
                Plugin.Log.LogInfo("Server started successfully.");
            }
        }

        private void DoConnect()
        {
            if (!NetworkManagerSystem.singleton)
            { SetStatus("NetworkManager not ready.", Color.red); EnsurePanelVisible(); return; }

            string addr = _joinAddressInput?.text?.Trim() ?? "";
            if (string.IsNullOrEmpty(addr))
            { SetStatus("Enter an address!", Color.red); EnsurePanelVisible(); return; }

            Plugin.DirectConnectActive = true;
            SaveNickname(_joinNicknameInput);
            SetConVar("cl_password", _joinPasswordInput?.text ?? "");

            // Save UI fields for next session
            Plugin.ConfigLastAddress.Value = addr;
            Plugin.ConfigLastPort.Value = _joinPortInput?.text ?? "7777";
            Plugin.ConfigLastClientPassword.Value = _joinPasswordInput?.text ?? "";

            // Fake lobby state — needed for the game's flow to work
            Patches.CreateLobbyPatch.FakeLobbyState();

            string port = _joinPortInput?.text ?? "7777";
            string cs;
            if (addr.Contains(":") && !addr.StartsWith("["))
                cs = $"[{addr}]:{port}";
            else if (addr.StartsWith("["))
                cs = addr.Contains("]:") ? addr : $"{addr.TrimEnd(']')}]:{port}";
            else
                cs = $"{addr}:{port}";

            AddressPortPair pair;
            if (!AddressPortPair.TryParse(cs, out pair) || !pair.isValid)
            { SetStatus($"Invalid address: {cs}", Color.red); Plugin.DirectConnectActive = false; EnsurePanelVisible(); return; }

            NetworkManagerSystem.singleton.desiredHost = new HostDescription(pair);
            Plugin.Log.LogInfo($"Connecting to {pair.address}:{pair.port}...");

            MenuInjector.DismissPanel(menuController);
            Plugin.Instance.StartCoroutine(WaitForConnectionOrRestore(pair));
        }

        private IEnumerator WaitForConnectionOrRestore(AddressPortPair pair)
        {
            float t = 0f;
            while (t < 10f)
            {
                // Client connected and scene is loading/loaded
                if (NetworkClient.active && NetworkClient.allClients.Count > 0)
                {
                    var client = NetworkClient.allClients[0];
                    if (client != null && client.isConnected)
                    {
                        Plugin.Log.LogInfo($"Connected to {pair.address}:{pair.port}");
                        yield break;
                    }
                }

                // desiredHost was reset (connection aborted by game)
                if (NetworkManagerSystem.singleton != null
                    && NetworkManagerSystem.singleton.desiredHost.hostType == HostDescription.HostType.None)
                {
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            // Connection failed
            Plugin.Log.LogError($"Connection to {pair.address}:{pair.port} failed (timeout 10s).");
            Plugin.DirectConnectActive = false;
            Patches.CreateLobbyPatch.CleanupFakeLobbyState();

            if (menuController != null)
                MenuInjector.ShowPanel(menuController);

            SetStatus($"Connection failed: {pair.address}:{pair.port}", Color.red);
        }

        private void SaveNickname(TMP_InputField nicknameInput)
        {
            string nick = nicknameInput?.text ?? "";
            if (string.IsNullOrEmpty(nick)) return;
            Plugin.ConfigDisplayName.Value = nick;
            ulong id = (ulong)(nick.GetHashCode() & 0x7FFFFFFF) + 76561198000000000UL;
            Plugin.ConfigPlatformId.Value = id.ToString();
        }

        private void SetStatus(string msg, Color c)
        {
            if (_statusLabel != null) { _statusLabel.text = msg; _statusLabel.color = c; }
            Plugin.Log.LogInfo($"[Panel] {msg}");
        }

        private void EnsurePanelVisible()
        {
            if (menuController != null)
                MenuInjector.ShowPanel(menuController);
        }

        private static void SetConVar(string n, string v)
        {
            try { RoR2.Console.instance?.SubmitCmd(null, $"{n} \"{v}\""); } catch { }
        }

        private static string L(string token) => Localization.Get(token);

        // ═══════ UI Builders ═══════

        private static TextMeshProUGUI AddLabel(Transform parent, string text, float size,
            FontStyles style, TextAlignmentOptions align, Color color, float height,
            float fixedW = -1)
        {
            var go = new GameObject("Lbl", typeof(RectTransform), typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false; // labels don't need raycast

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = height;
            if (fixedW > 0) { le.preferredWidth = fixedW; le.minWidth = fixedW; }
            else le.flexibleWidth = 1;

            return tmp;
        }

        private static TMP_InputField AddInputField(Transform parent, string placeholder,
            string defaultVal, bool isPassword = false, float fixedW = -1)
        {
            var go = new GameObject("Input", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement), typeof(MPEventSystemLocator));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.16f, 0.16f, 0.22f, 0.95f);
            img.raycastTarget = true; // inputs NEED raycast to receive clicks

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 34;
            le.minHeight = 34;
            le.flexibleHeight = 0; // don't stretch vertically
            if (fixedW > 0) { le.preferredWidth = fixedW; le.minWidth = fixedW; }
            else le.flexibleWidth = 1;

            // Border outline for visibility
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.40f, 0.55f, 0.6f);
            outline.effectDistance = new Vector2(1, -1);

            // Viewport
            var vp = new GameObject("VP", typeof(RectTransform), typeof(RectMask2D));
            vp.transform.SetParent(go.transform, false);
            var vpRT = vp.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = new Vector2(8, 2);
            vpRT.offsetMax = new Vector2(-8, -2);

            // Placeholder
            var phGo = new GameObject("PH", typeof(RectTransform), typeof(TextMeshProUGUI));
            phGo.transform.SetParent(vp.transform, false);
            StretchRT(phGo);
            var phT = phGo.GetComponent<TextMeshProUGUI>();
            phT.text = placeholder;
            phT.fontSize = 14;
            phT.fontStyle = FontStyles.Italic;
            phT.color = new Color(0.55f, 0.55f, 0.62f);
            phT.alignment = TextAlignmentOptions.MidlineLeft;
            phT.raycastTarget = false;

            // Text
            var txGo = new GameObject("TX", typeof(RectTransform), typeof(TextMeshProUGUI));
            txGo.transform.SetParent(vp.transform, false);
            StretchRT(txGo);
            var txT = txGo.GetComponent<TextMeshProUGUI>();
            txT.fontSize = 14;
            txT.color = Color.white;
            txT.alignment = TextAlignmentOptions.MidlineLeft;
            txT.raycastTarget = false;

            // Caret object — explicit creation ensures blinking cursor works
            var caretGo = new GameObject("Caret", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(LayoutElement));
            caretGo.transform.SetParent(vp.transform, false);
            StretchRT(caretGo);
            caretGo.GetComponent<LayoutElement>().ignoreLayout = true;

            // Input component
            var inp = go.AddComponent<TMP_InputField>();
            inp.textViewport = vpRT;
            inp.textComponent = txT;
            inp.placeholder = phT;
            inp.text = defaultVal;
            inp.pointSize = 14;
            inp.customCaretColor = true;
            inp.caretColor = Color.white;
            inp.caretWidth = 2;
            inp.caretBlinkRate = 0.85f;
            inp.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
            inp.onFocusSelectAll = false;
            inp.richText = false;
            inp.restoreOriginalTextOnEscape = true;

            if (isPassword)
            {
                inp.contentType = TMP_InputField.ContentType.Password;
                inp.asteriskChar = '*';
            }

            return inp;
        }

        private static Transform AddRow(Transform parent, int spacing = 8, float height = 34)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            go.GetComponent<LayoutElement>().preferredHeight = height;

            return go.transform;
        }

        private static void CloneBtn(Transform parent, string text,
            UnityEngine.Events.UnityAction onClick)
        {
            var btn = MenuInjector.CloneButton(parent, text, onClick);
            if (btn == null) return;

            var le = btn.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.flexibleHeight = 0; // NEVER stretch vertically
            le.preferredHeight = 45;
            le.minHeight = 45;
            le.minWidth = 100;
        }

        private static void AddSpacer(Transform parent, float h)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = h;
        }

        private static void AddDivider(Transform parent)
        {
            var go = new GameObject("Div", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.35f, 0.40f, 0.50f, 0.6f);
            img.raycastTarget = false; // CRITICAL: divider must not block clicks
            go.GetComponent<LayoutElement>().preferredHeight = 1;
        }

        private static void StretchRT(GameObject go)
        {
            var r = go.GetComponent<RectTransform>();
            if (r == null) return;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }
    }
}
