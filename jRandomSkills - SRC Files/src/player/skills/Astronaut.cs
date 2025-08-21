using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Astronaut : ISkill
    {
        private static Skills skillName = Skills.Astronaut;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#7E10AD", false);
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        var playerPawn = player.PlayerPawn.Value;

                        if (playerInfo?.Skill == skillName && playerPawn != null)
                        {
                            EnableSkill(player);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            ApplyGravityModifier(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            player.Pawn.Value.GravityScale = 1;
        }

        private static void ApplyGravityModifier(CCSPlayerController player)
        {
            var skillConfig = Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString());
            if (skillConfig == null) return;

            float gravityModifier = (float)Math.Round(Instance.Random.NextDouble() * (skillConfig.ChanceTo - skillConfig.ChanceFrom) + skillConfig.ChanceFrom, 1);
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("astronaut")}{ChatColors.Lime}: " + Localization.GetTranslation("astronaut_desc2", gravityModifier), false);
            player.Pawn.Value.GravityScale = gravityModifier;
        }
    }
}