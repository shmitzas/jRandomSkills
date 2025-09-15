using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using MaxMind.Db;
using Newtonsoft.Json;
using System.Net;

namespace jRandomSkills.src.utils
{
    public static class Localization
    {
        private static readonly string languagesFolderPath = Path.Combine(jRandomSkills.Instance.ModuleDirectory, "languages");
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = [];

        private static readonly string playersLanguageFileName = "playersLanguage.json";
        private static readonly string configsFolderPath = Path.Combine(jRandomSkills.Instance.ModuleDirectory, "configs");
        private static Dictionary<ulong, string> _playersLanguage = [];
        private static readonly string geoliteFilePath = Path.Combine(jRandomSkills.Instance.ModuleDirectory, "packages", "GeoLite2-Country.mmdb");
        public static readonly string[] chinaIsoCodes = ["CN", "TW", "HK", "MO", "SG"];
        public static readonly string[] portugalIsoCodes = ["PT", "BR", "AO", "CV", "GW", "MZ", "ST", "TL"];

        private static string defaultLangCode = "en";

        public static void Load()
        {
            _translations.Clear();
            _playersLanguage.Clear();
            SetLangCode();
            LoadAllLanguages();
            LoadPlayersLanguage();
        }
        
        private static void SetLangCode()
        {
            defaultLangCode = Config.LoadedConfig.Settings.LangCode;
        }

        private static void LoadAllLanguages()
        {
            if (!Directory.Exists(languagesFolderPath))
                return;

            foreach (var file in Directory.GetFiles(languagesFolderPath, "*.json"))
                LoadLanguage(file);
        }

        private static void LoadLanguage(string langPath)
        {
            if (!File.Exists(langPath))
                return;

            var code = Path.GetFileNameWithoutExtension(langPath);
            var jsonText = File.ReadAllText(langPath);
            var translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);

            if (translations != null)
                _translations[code] = translations;
        }

        public static bool HasTranslation(string code)
        {
            return _translations.ContainsKey(code);
        }

        public static string GetSkillName(this CCSPlayerController player, Skills skill)
        {
            string langCode = GetLangCode(player);
            return GetTranslation(skill.ToString().ToLower(), langCode);
        }

        public static string GetSkillDescription(this CCSPlayerController player, Skills skill)
        {
            string langCode = GetLangCode(player);
            return GetTranslation($"{skill.ToString().ToLower()}_desc", langCode);
        }

        public static void PrintTranslationToChatAll(string message, string[]? key, params object[][]? args)
        {
            foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
            {
                string langCode = GetLangCode(player);
                if (key == null)
                {
                    player.PrintToChat(message);
                    return;
                }

                List<string> translations = [];
                for (int i = 0; i < key.Length; i++)
                {
                    object[] currentArgs = (args != null && i < args.Length) ? args[i] : [];
                    string translation = GetTranslation(key[i], langCode, currentArgs);
                    translations.Add(translation);
                }
                player.PrintToChat(string.Format(message, translations.ToArray()));
            }
        }

        public static string GetTranslation(this CCSPlayerController player, string key, params object[] args)
        {
            string langCode = GetLangCode(player);
            return GetTranslation(key, langCode, args);
        }

        public static string GetTranslation(string key, string? langCode = null, params object[] args)
        {
            langCode ??= defaultLangCode;
            if (_translations.TryGetValue(langCode, out var langDict) && langDict.TryGetValue(key, out var translation))
                if (args.Length != 0 && args[0].ToString() == "welcome")
                    return translation;
                else
                {
                    string output = string.Format(translation, args).Replace("CHATCOLORS.RED", ChatColors.Red.ToString());
                    if (Config.LoadedConfig.Settings.AlternativeSkillButton != null)
                        output = output.Replace("css_useSkill", $"css_useSkill/{Config.LoadedConfig.Settings.AlternativeSkillButton}");
                    return output;
                }

            return key;
        }

        private static string GetLangCode(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid) return defaultLangCode;

            string? fileLangCode = GetLangCodeFromFile(player.SteamID);
            if (!string.IsNullOrEmpty(fileLangCode))
                return fileLangCode;

            string? geoliteLandCode = GetLangCodeFromDatabase(GetPlayerIP(player)) ?? defaultLangCode;
            ChangePlayerLanguage(player, geoliteLandCode);
            return geoliteLandCode;
        }

        private static string? GetPlayerIP(CCSPlayerController? player)
        {
            if (player == null) return null;
            var playerIP = player.IpAddress;
            if (playerIP == null) return null;
            string[] parts = playerIP.Split(':');
            return parts.Length > 1 ? parts[0] : playerIP;
        }

        private static string? GetLangCodeFromDatabase(string? playerIP)
        {
            if (string.IsNullOrEmpty(playerIP)) return null;
            if (!File.Exists(geoliteFilePath)) return null;
            using var reader = new Reader(geoliteFilePath);
            var ip = IPAddress.Parse(playerIP);
            var data = reader.Find<Dictionary<string, object>>(ip);
            if (data == null || data.Count == 0) return null;

            if (data.TryGetValue("country", out var _country) && _country is Dictionary<string, object> country)
                if (country.TryGetValue("iso_code", out var _isoCode) && _isoCode is string isoCode)
                {
                    if (portugalIsoCodes.Contains(isoCode))
                        isoCode = "pt-br";
                    if (chinaIsoCodes.Contains(isoCode))
                        isoCode = "zh";
                    isoCode = isoCode.ToLower();
                    if (_translations.ContainsKey(isoCode))
                        return isoCode;
                }
            return null;
        }

        private static void LoadPlayersLanguage()
        {
            string filePath = Path.Combine(configsFolderPath, playersLanguageFileName);
            if (!File.Exists(filePath))
                return;

            var jsonText = File.ReadAllText(filePath);
            _playersLanguage = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(jsonText) ?? [];
        }

        private static void SavePlayersLanguage()
        {
            Directory.CreateDirectory(configsFolderPath);
            string filePath = Path.Combine(configsFolderPath, playersLanguageFileName);
            var json = JsonConvert.SerializeObject(_playersLanguage);
            if (json != null)
                File.WriteAllText(filePath, json);
        }

        private static string? GetLangCodeFromFile(ulong? playerSteamID)
        {
            if (playerSteamID == null || playerSteamID == 0) return null;
            if (_playersLanguage.TryGetValue((ulong)playerSteamID, out var langCode))
                return langCode;
            return null;
        }

        public static void ChangePlayerLanguage(CCSPlayerController? player, string language)
        {
            if (player == null || !player.IsValid) return;
            _playersLanguage[player.SteamID] = language;
            SavePlayersLanguage();
        }
    }
}
