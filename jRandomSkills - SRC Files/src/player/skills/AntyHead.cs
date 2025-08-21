using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class AntyHead : ISkill
    {
        private static Skills skillName = Skills.AntyHead;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#8B4513");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                int hitgroup = @event.Hitgroup;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);

                if (playerInfo?.Skill == skillName && hitgroup == (int)HitGroup_t.HITGROUP_HEAD)
                {
                    ApplyIronHeadEffect(victim, @event.DmgHealth);
                    return HookResult.Stop;
                }

                return HookResult.Continue;
            });
        }

        private static void ApplyIronHeadEffect(CCSPlayerController victim, float damage)
        {
            var playerPawn = victim.PlayerPawn.Value;
            var newHealth = playerPawn.Health + damage;

            if (newHealth > 100)
                newHealth = 100;

            playerPawn.Health = (int)newHealth;
        }
    }
}