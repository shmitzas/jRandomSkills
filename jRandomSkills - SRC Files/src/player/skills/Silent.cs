using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Silent : ISkill
    {
        private static Skills skillName = Skills.Silent;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#333333");

            Instance.HookUserMessage(208, um =>
            {
                var soundevent = um.ReadUInt("soundevent_hash");
                var userIndex = um.ReadUInt("source_entity_index");

                if (userIndex == 0) return HookResult.Continue;

                if (!Instance.footstepSoundEvents.Contains(soundevent) && !Instance.silentSoundEvents.Contains(soundevent))
                    return HookResult.Continue;

                var player = Utilities.GetPlayers().FirstOrDefault(p => p.Pawn.Value.Index == userIndex);
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                um.Recipients.Clear();
                return HookResult.Handled;
            }, HookMode.Pre);
        }
    }
}