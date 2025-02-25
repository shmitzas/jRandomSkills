using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class KillerFlash : ISkill
    {
        private static Skills skillName = Skills.KillerFlash;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#57bcff");

            Instance.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;
                var attacker = @event.Attacker;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == skillName && playerInfo?.Skill != Skills.AntyFlash && player?.PlayerPawn.Value.FlashDuration >= 1)
                   player?.PlayerPawn?.Value?.CommitSuicide(false, true);

                return HookResult.Continue;
            });
        }
    }
}