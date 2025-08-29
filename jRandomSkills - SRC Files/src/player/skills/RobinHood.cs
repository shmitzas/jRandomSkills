using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RobinHood : ISkill
    {
        private const Skills skillName = Skills.RobinHood;
        private static int moneyMultiplier = Config.GetValue<int>(skillName, "moneyMultiplier");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;
                var damage = @event.DmgHealth;
                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim) || attacker == victim) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                if (attackerInfo?.Skill != skillName) return HookResult.Continue;

                int moneyToSteal = damage * moneyMultiplier;
                StealMoney(victim, attacker, moneyToSteal);

                return HookResult.Continue;
            });
        }

        private static void StealMoney(CCSPlayerController victim, CCSPlayerController attacker, int money)
        {
            var victimMoneyServices = victim?.InGameMoneyServices;
            var attackerMoneyServices = attacker?.InGameMoneyServices;
            if (victimMoneyServices == null || attackerMoneyServices == null) return;

            var moneyToAdd = victimMoneyServices.Account < money ? victimMoneyServices.Account : money;
            victimMoneyServices.Account = Math.Max(victimMoneyServices.Account - money, 0);
            Utilities.SetStateChanged(victim, "CCSPlayerController", "m_pInGameMoneyServices");

            attackerMoneyServices.Account = Math.Min(attackerMoneyServices.Account + moneyToAdd, 16000);
            Utilities.SetStateChanged(attacker, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int MoneyMultiplier { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#119125", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int moneyMultiplier = 35) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MoneyMultiplier = moneyMultiplier;
            }
        }
    }
}