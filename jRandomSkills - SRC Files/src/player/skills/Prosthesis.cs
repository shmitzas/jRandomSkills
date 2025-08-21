using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Prosthesis : ISkill
    {
        private static Skills skillName = Skills.Prosthesis;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#9c9c9c");

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                int damage = @event.DmgHealth;
                var hitgroup = (HitGroup_t)@event.Hitgroup;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim)) return HookResult.Continue;
                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                if (victimInfo == null || victimInfo.Skill != skillName) return HookResult.Continue;

                HitGroup_t[] disabledHitbox = { HitGroup_t.HITGROUP_LEFTARM, HitGroup_t.HITGROUP_RIGHTARM, HitGroup_t.HITGROUP_LEFTLEG, HitGroup_t.HITGROUP_RIGHTLEG };
                if (disabledHitbox.Contains(hitgroup))
                    RestoreHealth(victim, damage);
                return HookResult.Stop;
            });
        }

        private static void RestoreHealth(CCSPlayerController victim, float damage)
        {
            var playerPawn = victim.PlayerPawn.Value;
            var newHealth = playerPawn.Health + damage;

            if (newHealth > 100)
                newHealth = 100;

            playerPawn.Health = (int)newHealth;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
        }
    }
}