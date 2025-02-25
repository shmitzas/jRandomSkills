using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Dracula : ISkill
    {
        private static Skills skillName = Skills.Dracula;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#FA050D");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!Instance.IsPlayerValid(attacker) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == skillName && victim.PawnIsAlive)
                {
                    HealAttacker(attacker, @event.DmgHealth);
                }
                return HookResult.Continue;
            });
        }

        private static void HealAttacker(CCSPlayerController attacker, float damage)
        {
            var attackerPawn = attacker.PlayerPawn.Value;
            if (attackerPawn == null) return;

            int newHealth = (int)(attackerPawn.Health + (damage * 0.3));

            attackerPawn.MaxHealth = Math.Max(newHealth, 100);
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iMaxHealth");

            attackerPawn.Health = newHealth;
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iHealth");
        }
    }
}