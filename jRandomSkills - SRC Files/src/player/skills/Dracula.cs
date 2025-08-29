using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Dracula : ISkill
    {
        private const Skills skillName = Skills.Dracula;
        private static float healthRegainScale = Config.GetValue<float>(skillName, "healthRegainScale");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim) || attacker == victim) return HookResult.Continue;

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

            int newHealth = (int)(attackerPawn.Health + (damage * healthRegainScale));

            attackerPawn.MaxHealth = Math.Max(newHealth, 100);
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iMaxHealth");

            attackerPawn.Health = newHealth;
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iHealth");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float HealthRegainScale { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#FA050D", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float healthRegainScale = .3f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                HealthRegainScale = healthRegainScale;
            }
        }
    }
}