using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RichBoy : ISkill
    {
        private static Skills skillName = Skills.RichBoy;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#D4AF37");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                        if (playerInfo?.Skill == skillName)
                        {
                            int moneyBonus = Instance.Random.Next(5000, 15000);
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
            int moneyBonus = Instance.Random.Next(5000, 15000);
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
    }
}