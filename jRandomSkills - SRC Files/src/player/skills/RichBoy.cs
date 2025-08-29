using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RichBoy : ISkill
    {
        private const Skills skillName = Skills.RichBoy;
        private static int minMoney = Config.GetValue<int>(skillName, "minMoney");
        private static int maxMoney = Config.GetValue<int>(skillName, "maxMoney");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                        if (playerInfo?.Skill == skillName)
                        {
                            int moneyBonus = Instance.Random.Next(minMoney, maxMoney);
                            playerInfo.SkillChance = moneyBonus;
                            AddMoney(player, moneyBonus);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            int moneyBonus = Instance.Random.Next(minMoney, maxMoney);
            playerInfo.SkillChance = moneyBonus;
            AddMoney(player, moneyBonus);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            AddMoney(player, -(int)playerInfo.SkillChance);
        }

        private static void AddMoney(CCSPlayerController player, int money)
        {
            var moneyServices = player?.InGameMoneyServices;

            if (moneyServices == null) return;

            moneyServices.Account = Math.Max(moneyServices.Account + money, 0);
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int MinMoney { get; set; }
            public int MaxMoney { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#D4AF37", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int minMoney = 5000, int maxMoney = 15000) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MinMoney = minMoney;
                MaxMoney = maxMoney;
            }
        }
    }
}