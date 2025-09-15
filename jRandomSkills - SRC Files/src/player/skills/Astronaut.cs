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
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            ApplyGravityModifier(player);
        }

        public static void NewRound()
        {
            foreach (var player in Utilities.GetPlayers())
                DisableSkill(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;

            player.PlayerPawn.Value.ActualGravityScale = 1;
            playerInfo.SkillChance = 1;
        }

        private static void ApplyGravityModifier(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;

            float gravityModifier = (float)Math.Round(Instance.Random.NextDouble() * (Config.GetValue<float>(skillName, "ChanceTo") - Config.GetValue<float>(skillName, "chanceFrom")) + Config.GetValue<float>(skillName, "chanceFrom"), 1);
            playerInfo.SkillChance = gravityModifier;
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{player.GetTranslation("astronaut")}{ChatColors.Lime}: " + player.GetTranslation("astronaut_desc2", gravityModifier), false);
            player.PlayerPawn.Value.ActualGravityScale = gravityModifier;
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#7E10AD", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float chanceFrom = .1f, float chanceTo = .7f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float ChanceFrom { get; set; } = chanceFrom;
            public float ChanceTo { get; set; } = chanceTo;
        }
    }
}