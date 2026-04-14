using System.Collections.Generic;
using RoR2;

namespace RoR2DirectConnect
{
    public static class Localization
    {
        // Token constants used throughout UI
        public const string BUTTON_DIRECT_CONNECT = "DC_BUTTON_DIRECT_CONNECT";
        public const string TITLE = "DC_TITLE";
        public const string HOST_SECTION = "DC_HOST_SECTION";
        public const string JOIN_SECTION = "DC_JOIN_SECTION";
        public const string LABEL_DISPLAY_NAME = "DC_LABEL_DISPLAY_NAME";
        public const string LABEL_MAX_PLAYERS = "DC_LABEL_MAX_PLAYERS";
        public const string LABEL_PASSWORD = "DC_LABEL_PASSWORD";
        public const string LABEL_ADDRESS = "DC_LABEL_ADDRESS";
        public const string LABEL_PORT = "DC_LABEL_PORT";
        public const string PLACEHOLDER_NICKNAME = "DC_PH_NICKNAME";
        public const string PLACEHOLDER_ADDRESS = "DC_PH_ADDRESS";
        public const string PLACEHOLDER_PASSWORD = "DC_PH_PASSWORD";
        public const string IPV_HINT = "DC_IPV_HINT";
        public const string PW_HINT = "DC_PW_HINT";
        public const string BTN_HOST = "DC_BTN_HOST";
        public const string BTN_CONNECT = "DC_BTN_CONNECT";
        public const string BTN_BACK = "DC_BTN_BACK";

        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP Direct Connect",
                [TITLE] = "IP DIRECT CONNECT",
                [HOST_SECTION] = "HOST A GAME",
                [JOIN_SECTION] = "JOIN A GAME",
                [LABEL_DISPLAY_NAME] = "Display Name",
                [LABEL_MAX_PLAYERS] = "Max Players",
                [LABEL_PASSWORD] = "Password",
                [LABEL_ADDRESS] = "Address",
                [LABEL_PORT] = "Port",
                [PLACEHOLDER_NICKNAME] = "Enter your nickname...",
                [PLACEHOLDER_ADDRESS] = "IP address...",
                [PLACEHOLDER_PASSWORD] = "optional",
                [IPV_HINT] = "Supports IPv4 and IPv6 — Example: 192.168.1.100 or [::1]",
                [PW_HINT] = "Leave password empty for no password",
                [BTN_HOST] = "HOST GAME",
                [BTN_CONNECT] = "CONNECT",
                [BTN_BACK] = "BACK",
            },
            ["zh-CN"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP 直连",
                [TITLE] = "IP 直连",
                [HOST_SECTION] = "创建房间",
                [JOIN_SECTION] = "加入房间",
                [LABEL_DISPLAY_NAME] = "显示名称",
                [LABEL_MAX_PLAYERS] = "最大人数",
                [LABEL_PASSWORD] = "密码",
                [LABEL_ADDRESS] = "地址",
                [LABEL_PORT] = "端口",
                [PLACEHOLDER_NICKNAME] = "输入你的昵称...",
                [PLACEHOLDER_ADDRESS] = "IP 地址...",
                [PLACEHOLDER_PASSWORD] = "可选",
                [IPV_HINT] = "支持 IPv4 和 IPv6 — 示例: 192.168.1.100 或 [::1]",
                [PW_HINT] = "留空表示不设密码",
                [BTN_HOST] = "创建房间",
                [BTN_CONNECT] = "连接",
                [BTN_BACK] = "返回",
            },
            ["zh-TW"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP 直連",
                [TITLE] = "IP 直連",
                [HOST_SECTION] = "建立房間",
                [JOIN_SECTION] = "加入房間",
                [LABEL_DISPLAY_NAME] = "顯示名稱",
                [LABEL_MAX_PLAYERS] = "最大人數",
                [LABEL_PASSWORD] = "密碼",
                [LABEL_ADDRESS] = "地址",
                [LABEL_PORT] = "端口",
                [PLACEHOLDER_NICKNAME] = "輸入你的暱稱...",
                [PLACEHOLDER_ADDRESS] = "IP 地址...",
                [PLACEHOLDER_PASSWORD] = "可選",
                [IPV_HINT] = "支援 IPv4 和 IPv6 — 範例: 192.168.1.100 或 [::1]",
                [PW_HINT] = "留空表示不設密碼",
                [BTN_HOST] = "建立房間",
                [BTN_CONNECT] = "連線",
                [BTN_BACK] = "返回",
            },
            ["ja"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP直接接続",
                [TITLE] = "IP直接接続",
                [HOST_SECTION] = "ゲームをホスト",
                [JOIN_SECTION] = "ゲームに参加",
                [LABEL_DISPLAY_NAME] = "表示名",
                [LABEL_MAX_PLAYERS] = "最大人数",
                [LABEL_PASSWORD] = "パスワード",
                [LABEL_ADDRESS] = "アドレス",
                [LABEL_PORT] = "ポート",
                [PLACEHOLDER_NICKNAME] = "ニックネームを入力...",
                [PLACEHOLDER_ADDRESS] = "IPアドレス...",
                [PLACEHOLDER_PASSWORD] = "任意",
                [IPV_HINT] = "IPv4とIPv6に対応 — 例: 192.168.1.100 または [::1]",
                [PW_HINT] = "空欄でパスワードなし",
                [BTN_HOST] = "ホスト開始",
                [BTN_CONNECT] = "接続",
                [BTN_BACK] = "戻る",
            },
            ["ko"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP 직접 연결",
                [TITLE] = "IP 직접 연결",
                [HOST_SECTION] = "게임 호스트",
                [JOIN_SECTION] = "게임 참가",
                [LABEL_DISPLAY_NAME] = "표시 이름",
                [LABEL_MAX_PLAYERS] = "최대 인원",
                [LABEL_PASSWORD] = "비밀번호",
                [LABEL_ADDRESS] = "주소",
                [LABEL_PORT] = "포트",
                [PLACEHOLDER_NICKNAME] = "닉네임 입력...",
                [PLACEHOLDER_ADDRESS] = "IP 주소...",
                [PLACEHOLDER_PASSWORD] = "선택사항",
                [IPV_HINT] = "IPv4 및 IPv6 지원 — 예: 192.168.1.100 또는 [::1]",
                [PW_HINT] = "비워두면 비밀번호 없음",
                [BTN_HOST] = "호스트 시작",
                [BTN_CONNECT] = "연결",
                [BTN_BACK] = "뒤로",
            },
            ["ru"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "Прямое подключение",
                [TITLE] = "ПРЯМОЕ ПОДКЛЮЧЕНИЕ",
                [HOST_SECTION] = "СОЗДАТЬ ИГРУ",
                [JOIN_SECTION] = "ПРИСОЕДИНИТЬСЯ",
                [LABEL_DISPLAY_NAME] = "Имя",
                [LABEL_MAX_PLAYERS] = "Макс. игроков",
                [LABEL_PASSWORD] = "Пароль",
                [LABEL_ADDRESS] = "Адрес",
                [LABEL_PORT] = "Порт",
                [PLACEHOLDER_NICKNAME] = "Введите никнейм...",
                [PLACEHOLDER_ADDRESS] = "IP адрес...",
                [PLACEHOLDER_PASSWORD] = "необязательно",
                [IPV_HINT] = "Поддержка IPv4 и IPv6 — Пример: 192.168.1.100 или [::1]",
                [PW_HINT] = "Оставьте пустым для игры без пароля",
                [BTN_HOST] = "СОЗДАТЬ",
                [BTN_CONNECT] = "ПОДКЛЮЧИТЬСЯ",
                [BTN_BACK] = "НАЗАД",
            },
            ["de"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP-Direktverbindung",
                [TITLE] = "IP-DIREKTVERBINDUNG",
                [HOST_SECTION] = "SPIEL HOSTEN",
                [JOIN_SECTION] = "SPIEL BEITRETEN",
                [LABEL_DISPLAY_NAME] = "Anzeigename",
                [LABEL_MAX_PLAYERS] = "Max. Spieler",
                [LABEL_PASSWORD] = "Passwort",
                [LABEL_ADDRESS] = "Adresse",
                [LABEL_PORT] = "Port",
                [PLACEHOLDER_NICKNAME] = "Nickname eingeben...",
                [PLACEHOLDER_ADDRESS] = "IP-Adresse...",
                [PLACEHOLDER_PASSWORD] = "optional",
                [IPV_HINT] = "IPv4 und IPv6 — Beispiel: 192.168.1.100 oder [::1]",
                [PW_HINT] = "Leer lassen für kein Passwort",
                [BTN_HOST] = "HOSTEN",
                [BTN_CONNECT] = "VERBINDEN",
                [BTN_BACK] = "ZURÜCK",
            },
            ["fr"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "Connexion directe IP",
                [TITLE] = "CONNEXION DIRECTE IP",
                [HOST_SECTION] = "HÉBERGER",
                [JOIN_SECTION] = "REJOINDRE",
                [LABEL_DISPLAY_NAME] = "Pseudo",
                [LABEL_MAX_PLAYERS] = "Joueurs max",
                [LABEL_PASSWORD] = "Mot de passe",
                [LABEL_ADDRESS] = "Adresse",
                [LABEL_PORT] = "Port",
                [PLACEHOLDER_NICKNAME] = "Entrez votre pseudo...",
                [PLACEHOLDER_ADDRESS] = "Adresse IP...",
                [PLACEHOLDER_PASSWORD] = "facultatif",
                [IPV_HINT] = "IPv4 et IPv6 — Exemple : 192.168.1.100 ou [::1]",
                [PW_HINT] = "Laisser vide = pas de mot de passe",
                [BTN_HOST] = "HÉBERGER",
                [BTN_CONNECT] = "CONNECTER",
                [BTN_BACK] = "RETOUR",
            },
            ["es"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "Conexión directa IP",
                [TITLE] = "CONEXIÓN DIRECTA IP",
                [HOST_SECTION] = "CREAR PARTIDA",
                [JOIN_SECTION] = "UNIRSE",
                [LABEL_DISPLAY_NAME] = "Nombre",
                [LABEL_MAX_PLAYERS] = "Máx. jugadores",
                [LABEL_PASSWORD] = "Contraseña",
                [LABEL_ADDRESS] = "Dirección",
                [LABEL_PORT] = "Puerto",
                [PLACEHOLDER_NICKNAME] = "Introduce tu apodo...",
                [PLACEHOLDER_ADDRESS] = "Dirección IP...",
                [PLACEHOLDER_PASSWORD] = "opcional",
                [IPV_HINT] = "IPv4 e IPv6 — Ejemplo: 192.168.1.100 o [::1]",
                [PW_HINT] = "Dejar vacío = sin contraseña",
                [BTN_HOST] = "CREAR",
                [BTN_CONNECT] = "CONECTAR",
                [BTN_BACK] = "VOLVER",
            },
            ["pt-BR"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "Conexão direta IP",
                [TITLE] = "CONEXÃO DIRETA IP",
                [HOST_SECTION] = "HOSPEDAR JOGO",
                [JOIN_SECTION] = "ENTRAR EM JOGO",
                [LABEL_DISPLAY_NAME] = "Nome",
                [LABEL_MAX_PLAYERS] = "Máx. jogadores",
                [LABEL_PASSWORD] = "Senha",
                [LABEL_ADDRESS] = "Endereço",
                [LABEL_PORT] = "Porta",
                [PLACEHOLDER_NICKNAME] = "Digite seu apelido...",
                [PLACEHOLDER_ADDRESS] = "Endereço IP...",
                [PLACEHOLDER_PASSWORD] = "opcional",
                [IPV_HINT] = "IPv4 e IPv6 — Exemplo: 192.168.1.100 ou [::1]",
                [PW_HINT] = "Deixe vazio para sem senha",
                [BTN_HOST] = "HOSPEDAR",
                [BTN_CONNECT] = "CONECTAR",
                [BTN_BACK] = "VOLTAR",
            },
            ["tr"] = new Dictionary<string, string>
            {
                [BUTTON_DIRECT_CONNECT] = "IP Doğrudan Bağlantı",
                [TITLE] = "IP DOĞRUDAN BAĞLANTI",
                [HOST_SECTION] = "OYUN OLUŞTUR",
                [JOIN_SECTION] = "OYUNA KATIL",
                [LABEL_DISPLAY_NAME] = "Görünen Ad",
                [LABEL_MAX_PLAYERS] = "Maks. Oyuncu",
                [LABEL_PASSWORD] = "Şifre",
                [LABEL_ADDRESS] = "Adres",
                [LABEL_PORT] = "Port",
                [PLACEHOLDER_NICKNAME] = "Takma adınızı girin...",
                [PLACEHOLDER_ADDRESS] = "IP adresi...",
                [PLACEHOLDER_PASSWORD] = "isteğe bağlı",
                [IPV_HINT] = "IPv4 ve IPv6 — Örnek: 192.168.1.100 veya [::1]",
                [PW_HINT] = "Boş bırakırsanız şifre yok",
                [BTN_HOST] = "OLUŞTUR",
                [BTN_CONNECT] = "BAĞLAN",
                [BTN_BACK] = "GERİ",
            },
        };

        /// <summary>
        /// Register all tokens for every supported language.
        /// Call this once during plugin init.
        /// </summary>
        public static void RegisterAll()
        {
            foreach (var langPair in _translations)
            {
                var lang = Language.FindLanguageByName(langPair.Key);
                if (lang == null) continue;

                foreach (var token in langPair.Value)
                    lang.SetStringByToken(token.Key, token.Value);
            }

            // Also register English tokens as fallback on current language
            // in case current language isn't in our list
            var current = Language.currentLanguage;
            if (current != null && !_translations.ContainsKey(current.name))
            {
                foreach (var token in _translations["en"])
                    current.SetStringByToken(token.Key, token.Value);
            }

            Plugin.Log.LogInfo($"Localization registered for {_translations.Count} languages.");
        }

        /// <summary>Get a localized string for the current language.</summary>
        public static string Get(string token)
        {
            return Language.GetString(token);
        }
    }
}
