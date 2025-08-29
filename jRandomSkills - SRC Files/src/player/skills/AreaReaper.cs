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
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
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
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            if (playerInfo.SkillChance == 1)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_used_info")}");
                return;
            }

            int site = new string[]{ "A", "a" }.Contains(commands[0]) ? 0 : new string[] { "B", "b" }.Contains(commands[0]) ? 1 : -1;
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
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("areareaper") + ":", false);
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("areareaper_select_info")}");
            player.PrintToChat($" {ChatColors.Green}· {ChatColors.Red}/t {ChatColors.Green}A");
            player.PrintToChat($" {ChatColors.Green}· {ChatColors.Red}/t {ChatColors.Green}B");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (Instance.skillPlayer.FirstOrDefault(p => p.Skill == skillName) != null)
                return;
            EnableBombsite();
        }

        private static void EnableBombsite()
        {
            var bombTargets = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target");
            foreach (var bombTarget in bombTargets)
                bombTarget.AcceptInput("Enable");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#edf5b5", CsTeam onlyTeam = CsTeam.CounterTerrorist, bool needsTeammates = false) : base(skill, active, color, onlyTeam, needsTeammates)
            {
            }
        }
    }
}