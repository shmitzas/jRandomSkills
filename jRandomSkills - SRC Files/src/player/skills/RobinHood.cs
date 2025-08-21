using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RobinHood : ISkill
    {
        private static Skills skillName = Skills.RobinHood;
        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#119125");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;
                var damage = @event.DmgHealth;
                if (!Instance.IsPlayerValid(victim) || !Instance.IsPlayerValid(attacker)) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                if (attackerInfo?.Skill != skillName) return HookResult.Continue;

                int moneyToSteal = damage * 35;
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
    }
}