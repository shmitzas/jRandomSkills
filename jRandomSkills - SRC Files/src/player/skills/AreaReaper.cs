using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class AreaReaper : ISkill
    {
        private const Skills skillName = Skills.AreaReaper;
        private static readonly string[] bombsiteA = ["A", "a"];
        private static readonly string[] bombsiteB = ["B", "b"];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    EnableBombsite();
                });

                return HookResult.Continue;
            });
        }

        public static void TypeSkill(CCSPlayerController player, string[] commands)
        {
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            if (playerInfo.SkillChance == 1)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_used_info")}");
                return;
            }

            int site = bombsiteA.Contains(commands[0]) ? 0 : bombsiteB.Contains(commands[0]) ? 1 : -1;
            if (site == -1) {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_incorrect_site")}");
                return;
            }
            
            var bombTargets = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target").ToArray();
            if (bombTargets.Length == 2)
            {
                bombTargets[site].AcceptInput("Disable");
                playerInfo.SkillChance = 1;
                player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("areareaper_site_disabled", (site == 0 ? 'A' : 'B'))}");
            }
            else
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_no_site")}");
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("areareaper") + ":", false);
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("areareaper_select_info")}");
            player.PrintToChat($" {ChatColors.Green}· {ChatColors.Red}/t {ChatColors.Green}A");
            player.PrintToChat($" {ChatColors.Green}· {ChatColors.Red}/t {ChatColors.Green}B");
        }

#pragma warning disable IDE0060 // Usuń nieużywany parametr
        public static void DisableSkill(CCSPlayerController player)
#pragma warning restore IDE0060 // Usuń nieużywany parametr
        {
            if (Instance.SkillPlayer.FirstOrDefault(p => p.Skill == skillName) != null)
                return;
            EnableBombsite();
        }

        private static void EnableBombsite()
        {
            var bombTargets = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target");
            foreach (var bombTarget in bombTargets)
                bombTarget.AcceptInput("Enable");
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#edf5b5", CsTeam onlyTeam = CsTeam.CounterTerrorist, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}