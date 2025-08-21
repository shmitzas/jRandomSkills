using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class AntyFlash : ISkill
    {
        private static Skills skillName = Skills.AntyFlash;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#D6E6FF");
            
            Instance.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;
                var attacker = @event.Attacker;
                
                var playerPawn = player.PlayerPawn.Value;

                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == skillName)
                    playerPawn.FlashDuration = 0.0f;
                else if (attackerInfo?.Skill == skillName)
                    playerPawn.FlashDuration = 7.0f;

                return HookResult.Continue;
            });
        }
    }
}