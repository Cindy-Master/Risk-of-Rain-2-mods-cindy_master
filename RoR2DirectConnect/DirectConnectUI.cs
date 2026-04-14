using System;
using System.Collections.Generic;
using HarmonyLib;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2DirectConnect
{
    public class DirectConnectUI : MonoBehaviour
    {
        // ── State ──
        private bool _showWindow;
        private int _activeTab; // 0=Host, 1=Join, 2=Settings

        // Identity
        private string _platformId = "";

        // Host
        private string _maxPlayers = "4";
        private string _hostPort = "7777";
        private string _serverPassword = "";

        // Join
        private string _connectAddress = "";
        private string _connectPort = "7777";
        private string _clientPassword = "";
        private List<string> _recentServers = new List<string>();

        // Settings
        private bool _bypassSteam = true;
        private bool _bypassLobby = true;

        // Status
        private string _statusMessage = "";
        private Color _statusColor = Color.white;
        private float _statusClearTime;

        // Scroll
        private Vector2 _playerListScroll;

        // ── Textures ──
        private Texture2D _texBg;
        private Texture2D _texCard;
        private Texture2D _texInput;
        private Texture2D _texInputFocus;
        private Texture2D _texBtnBlue;
        private Texture2D _texBtnBlueHover;
        private Texture2D _texBtnGreen;
        private Texture2D _texBtnGreenHover;
        private Texture2D _texBtnRed;
        private Texture2D _texBtnRedHover;
        private Texture2D _texBtnGray;
        private Texture2D _texBtnGrayHover;
        private Texture2D _texTabActive;
        private Texture2D _texTabInactive;
        private Texture2D _texTabHover;
        private Texture2D _texOverlay;
        private Texture2D _texStatusBar;

        // ── Styles ──
        private GUIStyle _sWindow, _sCard, _sTitle, _sHeader, _sLabel, _sSmall;
        private GUIStyle _sInput, _sPassword;
        private GUIStyle _sBtnBlue, _sBtnGreen, _sBtnRed, _sBtnGray;
        private GUIStyle _sTabActive, _sTabInactive;
        private GUIStyle _sStatus, _sToggle, _sIndicator;
        private bool _init;

        private KeyCode _toggleKey = KeyCode.F6;

        // ── Layout Constants ──
        private const float WIN_W = 480f;
        private const float WIN_H = 560f;

        private void Start()
        {
            _platformId = Plugin.ConfigPlatformId.Value;
            _bypassSteam = Plugin.ConfigBypassSteamAuth.Value;
            _bypassLobby = Plugin.ConfigBypassLobbyCheck.Value;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
                _showWindow = !_showWindow;

            if (_statusClearTime > 0 && Time.time > _statusClearTime)
            {
                _statusMessage = "";
                _statusClearTime = 0;
            }
        }

        // ══════════════════════════════════════
        //  Styles & Textures
        // ══════════════════════════════════════
        private void InitOnce()
        {
            if (_init) return;

            // Colors
            Color bgCol       = new Color(0.08f, 0.08f, 0.11f, 0.97f);
            Color cardCol     = new Color(0.14f, 0.14f, 0.18f, 0.95f);
            Color inputCol    = new Color(0.10f, 0.10f, 0.14f, 1f);
            Color inputFocus  = new Color(0.16f, 0.16f, 0.22f, 1f);
            Color blue        = new Color(0.22f, 0.45f, 0.78f, 1f);
            Color blueHover   = new Color(0.28f, 0.52f, 0.88f, 1f);
            Color green       = new Color(0.18f, 0.58f, 0.28f, 1f);
            Color greenHover  = new Color(0.22f, 0.68f, 0.32f, 1f);
            Color red         = new Color(0.70f, 0.18f, 0.18f, 1f);
            Color redHover    = new Color(0.82f, 0.22f, 0.22f, 1f);
            Color gray        = new Color(0.28f, 0.28f, 0.32f, 1f);
            Color grayHover   = new Color(0.35f, 0.35f, 0.40f, 1f);
            Color tabActive   = new Color(0.22f, 0.45f, 0.78f, 1f);
            Color tabInactive = new Color(0.18f, 0.18f, 0.22f, 1f);
            Color tabHover    = new Color(0.24f, 0.24f, 0.30f, 1f);
            Color overlay     = new Color(0f, 0f, 0f, 0.55f);
            Color statusBar   = new Color(0.10f, 0.10f, 0.14f, 0.9f);

            _texBg           = Tex(bgCol);
            _texCard         = Tex(cardCol);
            _texInput        = Tex(inputCol);
            _texInputFocus   = Tex(inputFocus);
            _texBtnBlue      = Tex(blue);
            _texBtnBlueHover = Tex(blueHover);
            _texBtnGreen     = Tex(green);
            _texBtnGreenHover= Tex(greenHover);
            _texBtnRed       = Tex(red);
            _texBtnRedHover  = Tex(redHover);
            _texBtnGray      = Tex(gray);
            _texBtnGrayHover = Tex(grayHover);
            _texTabActive    = Tex(tabActive);
            _texTabInactive  = Tex(tabInactive);
            _texTabHover     = Tex(tabHover);
            _texOverlay      = Tex(overlay);
            _texStatusBar    = Tex(statusBar);

            // Window
            _sWindow = new GUIStyle
            {
                normal = { background = _texBg },
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0)
            };

            // Card
            _sCard = new GUIStyle
            {
                normal = { background = _texCard },
                padding = new RectOffset(14, 14, 10, 10),
                margin = new RectOffset(16, 16, 6, 6)
            };

            // Title
            _sTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            // Header
            _sHeader = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 0.82f, 1f) },
                margin = new RectOffset(0, 0, 2, 4)
            };

            // Label
            _sLabel = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.80f, 0.80f, 0.82f) }
            };

            // Small
            _sSmall = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.50f, 0.50f, 0.55f) }
            };

            // Input
            _sInput = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
                normal = { background = _texInput, textColor = Color.white },
                focused = { background = _texInputFocus, textColor = Color.white },
                hover = { background = _texInputFocus, textColor = Color.white },
                padding = new RectOffset(10, 10, 7, 7),
                fixedHeight = 32,
                margin = new RectOffset(0, 0, 2, 2)
            };

            _sPassword = new GUIStyle(_sInput);

            // Buttons
            _sBtnBlue  = MakeBtn(_texBtnBlue, _texBtnBlueHover, 36);
            _sBtnGreen = MakeBtn(_texBtnGreen, _texBtnGreenHover, 40);
            _sBtnRed   = MakeBtn(_texBtnRed, _texBtnRedHover, 40);
            _sBtnGray  = MakeBtn(_texBtnGray, _texBtnGrayHover, 30);

            // Tabs
            _sTabActive = MakeBtn(_texTabActive, _texTabActive, 34);
            _sTabActive.fontSize = 14;
            _sTabInactive = MakeBtn(_texTabInactive, _texTabHover, 34);
            _sTabInactive.fontSize = 14;
            _sTabInactive.normal.textColor = new Color(0.65f, 0.65f, 0.70f);

            // Status
            _sStatus = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                padding = new RectOffset(8, 8, 4, 4)
            };

            // Toggle
            _sToggle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.80f, 0.80f, 0.82f) },
                onNormal = { textColor = new Color(0.45f, 0.95f, 0.50f) }
            };

            // Indicator
            _sIndicator = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _init = true;
        }

        // ══════════════════════════════════════
        //  OnGUI
        // ══════════════════════════════════════
        private void OnGUI()
        {
            if (!_showWindow) return;
            InitOnce();

            // Dark overlay behind panel
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texOverlay);

            // Center the window
            float x = (Screen.width - WIN_W) / 2f;
            float y = (Screen.height - WIN_H) / 2f;
            var winRect = new Rect(x, y, WIN_W, WIN_H);

            GUI.Box(winRect, GUIContent.none, _sWindow);
            GUILayout.BeginArea(winRect);
            DrawPanel();
            GUILayout.EndArea();
        }

        private void DrawPanel()
        {
            // ── Title Bar ──
            GUILayout.Space(12);
            GUILayout.Label("DIRECT CONNECT", _sTitle);
            GUILayout.Space(4);

            // ── Tabs ──
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Host", _activeTab == 0 ? _sTabActive : _sTabInactive, GUILayout.Width(120)))
                _activeTab = 0;
            if (GUILayout.Button("Join", _activeTab == 1 ? _sTabActive : _sTabInactive, GUILayout.Width(120)))
                _activeTab = 1;
            if (GUILayout.Button("Settings", _activeTab == 2 ? _sTabActive : _sTabInactive, GUILayout.Width(120)))
                _activeTab = 2;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // ── Tab Content ──
            switch (_activeTab)
            {
                case 0: DrawHostTab(); break;
                case 1: DrawJoinTab(); break;
                case 2: DrawSettingsTab(); break;
            }

            // ── Status Message ──
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.BeginVertical(new GUIStyle { normal = { background = _texStatusBar }, padding = new RectOffset(8, 8, 4, 4) });
                _sStatus.normal.textColor = _statusColor;
                GUILayout.Label(_statusMessage, _sStatus);
                GUILayout.EndVertical();
                GUILayout.Space(16);
                GUILayout.EndHorizontal();
            }

            // ── Connection Indicator ──
            GUILayout.FlexibleSpace();
            DrawIndicator();
            GUILayout.Space(8);

            // ── Close hint ──
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Press F6 to close", _sSmall);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        // ── Host Tab ──
        private void DrawHostTab()
        {
            GUILayout.BeginVertical(_sCard);
            GUILayout.Label("SERVER SETTINGS", _sHeader);

            Row("Port", ref _hostPort, 80);
            Row("Max Players", ref _maxPlayers, 80);
            PasswordRow("Password", ref _serverPassword);
            GUILayout.Label("Leave empty for no password", _sSmall);

            GUILayout.Space(6);

            bool serverActive = NetworkServer.active;
            if (!serverActive)
            {
                if (GUILayout.Button("START HOST", _sBtnGreen))
                    DoHost();
            }
            else
            {
                if (GUILayout.Button("STOP HOST", _sBtnRed))
                    DoDisconnect();

                GUILayout.Space(4);
                int port = NetworkManagerSystem.singleton != null ? NetworkManagerSystem.singleton.networkPort : 7777;
                int conns = NetworkServer.connections.Count;
                GUILayout.Label($"Listening on port {port}  —  {conns} connection(s)", _sSmall);
            }

            GUILayout.EndVertical();

            // Player list
            if (NetworkServer.active || NetworkClient.active)
                DrawPlayerList();
        }

        // ── Join Tab ──
        private void DrawJoinTab()
        {
            GUILayout.BeginVertical(_sCard);
            GUILayout.Label("CONNECT TO SERVER", _sHeader);

            GUILayout.Label("IPv4: 192.168.1.100    IPv6: [::1] or [2001:db8::1]", _sSmall);
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Address", _sLabel, GUILayout.Width(65));
            _connectAddress = GUILayout.TextField(_connectAddress, _sInput);
            GUILayout.Label(":", _sLabel, GUILayout.Width(8));
            _connectPort = GUILayout.TextField(_connectPort, _sInput, GUILayout.Width(65));
            GUILayout.EndHorizontal();

            PasswordRow("Password", ref _clientPassword);

            GUILayout.Space(6);

            bool active = NetworkClient.active || NetworkServer.active;
            if (!active)
            {
                if (GUILayout.Button("CONNECT", _sBtnGreen))
                    DoConnect();
            }
            else
            {
                if (GUILayout.Button("DISCONNECT", _sBtnRed))
                    DoDisconnect();
            }

            GUILayout.EndVertical();

            // Recent servers
            if (_recentServers.Count > 0)
            {
                GUILayout.BeginVertical(_sCard);
                GUILayout.Label("RECENT", _sHeader);
                foreach (var srv in _recentServers)
                {
                    if (GUILayout.Button(srv, _sBtnGray))
                    {
                        ParseRecent(srv);
                    }
                }
                GUILayout.EndVertical();
            }

            // Player list (when connected)
            if (NetworkClient.active)
                DrawPlayerList();
        }

        // ── Settings Tab ──
        private void DrawSettingsTab()
        {
            GUILayout.BeginVertical(_sCard);
            GUILayout.Label("IDENTITY", _sHeader);
            GUILayout.Label("Platform ID — each player must use a different number", _sSmall);

            GUILayout.BeginHorizontal();
            _platformId = GUILayout.TextField(_platformId, _sInput);
            if (GUILayout.Button("SAVE", _sBtnBlue, GUILayout.Width(70)))
            {
                ulong p;
                if (ulong.TryParse(_platformId, out p))
                {
                    Plugin.ConfigPlatformId.Value = _platformId;
                    SetStatus("ID saved!", Color.green);
                }
                else
                    SetStatus("Invalid! Must be a number.", Color.red);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(_sCard);
            GUILayout.Label("STEAM BYPASS", _sHeader);

            bool s = GUILayout.Toggle(_bypassSteam, "  Bypass Steam Authentication", _sToggle);
            if (s != _bypassSteam) { _bypassSteam = s; Plugin.ConfigBypassSteamAuth.Value = s; }

            bool l = GUILayout.Toggle(_bypassLobby, "  Bypass Steam Lobby", _sToggle);
            if (l != _bypassLobby) { _bypassLobby = l; Plugin.ConfigBypassLobbyCheck.Value = l; }

            GUILayout.Space(4);
            GUILayout.Label("Both must be ON for direct connect to work without Steam.", _sSmall);
            GUILayout.EndVertical();
        }

        // ── Player List Card ──
        private void DrawPlayerList()
        {
            GUILayout.BeginVertical(_sCard);
            GUILayout.Label("PLAYERS", _sHeader);

            _playerListScroll = GUILayout.BeginScrollView(_playerListScroll, GUILayout.Height(60));
            var users = NetworkUser.readOnlyInstancesList;
            if (users.Count == 0)
                GUILayout.Label("No players connected yet.", _sSmall);
            else
            {
                foreach (var u in users)
                {
                    string name = u.userName ?? "Unknown";
                    string tag = u.isLocalPlayer ? "  (You)" : "";
                    GUILayout.Label($"  {name}{tag}", _sLabel);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // ── Connection Indicator ──
        private void DrawIndicator()
        {
            string state; Color c;
            if (NetworkServer.active && NetworkClient.active)
                { state = "HOST  (Server + Client)"; c = new Color(0.35f, 0.95f, 0.45f); }
            else if (NetworkServer.active)
                { state = "SERVER"; c = new Color(0.40f, 0.75f, 0.95f); }
            else if (NetworkClient.active)
                { state = "CONNECTED"; c = new Color(0.35f, 0.95f, 0.45f); }
            else
                { state = "OFFLINE"; c = new Color(0.55f, 0.55f, 0.60f); }

            _sIndicator.normal.textColor = c;
            GUILayout.Label(state, _sIndicator);
        }

        // ══════════════════════════════════════
        //  Actions
        // ══════════════════════════════════════
        private void DoHost()
        {
            if (!NetworkManagerSystem.singleton) { SetStatus("NetworkManager not ready.", Color.red); return; }
            if (NetworkServer.active) { SetStatus("Server already running.", Color.yellow); return; }

            int max; if (!int.TryParse(_maxPlayers, out max) || max < 1 || max > 16) max = 4;

            if (!string.IsNullOrEmpty(_platformId)) Plugin.ConfigPlatformId.Value = _platformId;
            SetConVar("sv_password", _serverPassword);
            ushort port; if (ushort.TryParse(_hostPort, out port)) SetConVar("sv_port", port.ToString());

            NetworkManagerSystem.singleton.desiredHost = new HostDescription(
                new HostDescription.HostingParameters { listen = true, maxPlayers = max });

            string pw = string.IsNullOrEmpty(_serverPassword) ? "no password" : "password set";
            SetStatus($"Hosting on port {_hostPort}, {max} players, {pw}", Color.green);
        }

        private void DoConnect()
        {
            if (!NetworkManagerSystem.singleton) { SetStatus("NetworkManager not ready.", Color.red); return; }

            string addr = _connectAddress.Trim();
            if (string.IsNullOrEmpty(addr)) { SetStatus("Enter an address.", Color.red); return; }

            if (!string.IsNullOrEmpty(_platformId)) Plugin.ConfigPlatformId.Value = _platformId;
            SetConVar("cl_password", _clientPassword);

            string cs;
            if (addr.Contains(":") && !addr.StartsWith("[")) cs = $"[{addr}]:{_connectPort}";
            else if (addr.StartsWith("["))
            {
                cs = addr.Contains("]:") ? addr : addr.TrimEnd(']') + $"]:{_connectPort}";
            }
            else cs = $"{addr}:{_connectPort}";

            AddressPortPair pair;
            if (!AddressPortPair.TryParse(cs, out pair) || !pair.isValid)
            { SetStatus($"Cannot parse: {cs}", Color.red); return; }

            // Save to recent
            string entry = $"{addr}:{_connectPort}";
            _recentServers.Remove(entry);
            _recentServers.Insert(0, entry);
            if (_recentServers.Count > 5) _recentServers.RemoveAt(5);

            NetworkManagerSystem.singleton.desiredHost = new HostDescription(pair);
            SetStatus($"Connecting to {pair.address}:{pair.port} ...", Color.cyan);
        }

        private void DoDisconnect()
        {
            if (!NetworkManagerSystem.singleton) return;
            NetworkManagerSystem.singleton.desiredHost = HostDescription.none;
            if (NetworkServer.active || NetworkClient.active) NetworkManagerSystem.singleton.StopHost();
            SetStatus("Disconnected.", Color.yellow);
        }

        private void ParseRecent(string entry)
        {
            int lastColon = entry.LastIndexOf(':');
            if (lastColon > 0)
            {
                _connectAddress = entry.Substring(0, lastColon);
                _connectPort = entry.Substring(lastColon + 1);
            }
            _activeTab = 1; // switch to Join tab
        }

        // ══════════════════════════════════════
        //  Helpers
        // ══════════════════════════════════════
        private void Row(string label, ref string val, float fieldW)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _sLabel, GUILayout.Width(90));
            val = GUILayout.TextField(val, _sInput, GUILayout.Width(fieldW));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void PasswordRow(string label, ref string val)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _sLabel, GUILayout.Width(90));
            val = GUILayout.PasswordField(val, '*', _sInput);
            GUILayout.EndHorizontal();
        }

        private static void SetConVar(string name, string value)
        {
            try
            {
                var console = RoR2.Console.instance;
                if (console != null)
                    console.SubmitCmd(null, $"{name} \"{value}\"");
            }
            catch (Exception ex) { Plugin.Log.LogWarning($"SetConVar {name}: {ex.Message}"); }
        }

        private void SetStatus(string msg, Color color)
        {
            _statusMessage = msg;
            _statusColor = color;
            _statusClearTime = Time.time + 6f;
            Plugin.Log.LogInfo("[UI] " + msg);
        }

        private GUIStyle MakeBtn(Texture2D normal, Texture2D hover, float h)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = normal, textColor = Color.white },
                hover = { background = hover, textColor = Color.white },
                active = { background = normal, textColor = new Color(0.8f, 0.8f, 0.8f) },
                fixedHeight = h,
                margin = new RectOffset(0, 0, 3, 3)
            };
        }

        private static Texture2D Tex(Color c)
        {
            var t = new Texture2D(2, 2);
            var p = new[] { c, c, c, c };
            t.SetPixels(p);
            t.Apply();
            return t;
        }
    }
}
