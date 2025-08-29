using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public static class Config
    {
        private static readonly string configPath = Path.Combine(Instance.ModuleDirectory, "Config.json");
        public static ConfigModel config;
        private static FileSystemWatcher fileWatcher;

        public static ConfigModel LoadedConfig => config;

        public static void Initialize()
        {
            config = LoadConfig();
            SetupFileWatcher();
        }

        private static ConfigModel LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                Instance.Logger.LogInformation("Config file does not exist. Create a new config file...");
                var defaultConfig = new ConfigModel();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            string json = File.ReadAllText(configPath);
            var config = new ConfigModel();

            try
            {
                var root = JsonConvert.DeserializeObject<JObject>(json);

                var settings = root["Settings"];
                if (settings != null) JsonConvert.PopulateObject(settings.ToString(), config.Settings);

                var skillsArray = (JArray)root["SkillsInfo"];
                foreach (var skillObj in skillsArray)
                {
                    string name = skillObj["Name"]?.ToString();
                    var instance = config.SkillsInfo.FirstOrDefault(x => x.Name == name);
                    if (instance != null) JsonConvert.PopulateObject(skillObj.ToString(), instance);
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Error when loading the config file: {ex.Message}");
            }

            return config;
        }

        public static void SaveConfig(ConfigModel config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Error when saving the config file: {ex.Message}");
            }
        }

        private static void SetupFileWatcher()
        {
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(configPath))
            {
                Filter = Path.GetFileName(configPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            fileWatcher.Changed += (sender, e) => config = LoadConfig();
            fileWatcher.EnableRaisingEvents = true;
        }

        public static T GetValue<T>(object skill, string key)
        {
            var skillConfig = config.SkillsInfo.FirstOrDefault(s => s.Name == skill.ToString());

            var prop = skillConfig.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                var value = prop.GetValue(skillConfig);
                if (value == null) return default!;
                else return (T)Convert.ChangeType(value, typeof(T));
            }

            var field = skillConfig.GetType().GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                var value = field.GetValue(skillConfig);
                if (value == null) return default!;
                else return (T)Convert.ChangeType(value, typeof(T));
            }

            return default!;
        }

        public class ConfigModel
        {
            public Settings Settings { get; set; } = new Settings();
            public DefaultSkillInfo[] SkillsInfo { get; set; }

            public ConfigModel()
            {
                SkillsInfo = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(DefaultSkillInfo).IsAssignableFrom(t) && t.Name == "SkillConfig")
                    .Select(t =>
                    {
                        var ctor = t.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
                        if (ctor == null) return null;
                        var args = ctor.GetParameters().Select(p => Type.Missing).ToArray();
                        return ctor.Invoke(args) as DefaultSkillInfo;
                    })
                    .Where(instance => instance != null)
                    .ToArray()!;
            }
        }

        public class Settings
        {
            public string LangCode { get; set; }
            public int GameMode { get; set; }
            public bool KillerSkillInfo { get; set; }
            public bool TeamMateSkillInfo { get; set; }
            public bool SummaryAfterTheRound { get; set; }
            public bool DebugMode { get; set; }
            public string SetSkillCommands { get; set; }
            public string SkillsListCommands { get; set; }
            public string UseSkillCommands { get; set; }
            public string ChangeMapCommands { get; set; }
            public string StartGameCommands { get; set; }
            public string ConsoleCommands { get; set; }
            public string SwapCommands { get; set; }
            public string ShuffleCommands { get; set; }
            public string PauseCommands { get; set; }
            public string HealCommands { get; set; }
            public string SetScoreCommands { get; set; }

            public Settings()
            {
                LangCode = "en";
                GameMode = (int)GameModes.Normal;
                KillerSkillInfo = true;
                TeamMateSkillInfo = true;
                SummaryAfterTheRound = true;
                DebugMode = true;

                SetSkillCommands = "ustawskill, setskill, definirhabilidade, configurarhabilidade, 设置技能, 配置技能";
                SkillsListCommands = "supermoc, skille, listamocy, supermoce, skills, listaHabilidades, habilidades, 技能列表, 超能力列表";
                UseSkillCommands = "t, useSkill, usarHabilidade, 技能使用, 使用技能";
                ChangeMapCommands = "map, mapa, changemap, zmienmape, zmienmape, mudarMapa, trocarMapa, 更换地图, 更改地图";
                StartGameCommands = "start, go, começar, iniciar, 开始, 启动";
                ConsoleCommands = "console, sv, 控制台, 服务器";
                SwapCommands = "swap, zmiana, trocar, 交换, 切换";
                ShuffleCommands = "shuffle, embaralhar, 随机排序, 洗牌";
                PauseCommands = "pause, unpause, pausar, despausar, 暂停, 恢复";
                HealCommands = "heal, ulecz, curar, tratar, 治疗, 治愈";
                SetScoreCommands = "setscore, wynik, definirPontuacao, configurarPontos, 设置分数, 调整分数";
            }

        }

        public class DefaultSkillInfo
        {
            public bool NeedsTeammates { get; set; }
            public int OnlyTeam { get; set; }
            public string Color { get; set; }
            public bool Active { get; set; }
            public string Name { get; set; }

            public DefaultSkillInfo(Skills skill, bool active = true, string color = "#ffffff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false)
            {
                Name = skill.ToString();
                Active = active;
                Color = color;
                OnlyTeam = (int)onlyTeam;
                NeedsTeammates = needsTeammates;
            }
        }

        public enum GameModes
        {
            Normal = 0,
            TeamSkills = 1,
            SameSkills = 2,
            Debug = 3,
        }
    }
}