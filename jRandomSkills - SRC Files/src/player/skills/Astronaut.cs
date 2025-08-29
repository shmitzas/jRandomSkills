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
        private const Skills skillName = Skills.Astronaut;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);

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

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                    DisableSkill(player);
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            ApplyGravityModifier(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            player.Pawn.Value.ActualGravityScale = 1;
        }

        private static void ApplyGravityModifier(CCSPlayerController player)
        {
            float gravityModifier = (float)Math.Round(Instance.Random.NextDouble() * (Config.GetValue<float>(skillName, "ChanceTo") - Config.GetValue<float>(skillName, "chanceFrom")) + Config.GetValue<float>(skillName, "chanceFrom"), 1);
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("astronaut")}{ChatColors.Lime}: " + Localization.GetTranslation("astronaut_desc2", gravityModifier), false);
            player.Pawn.Value.ActualGravityScale = gravityModifier;
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float ChanceFrom { get; set; }
            public float ChanceTo { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#7E10AD", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float chanceFrom = .1f, float chanceTo = .7f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                ChanceFrom = chanceFrom;
                ChanceTo = chanceTo;
            }
        }
    }
}