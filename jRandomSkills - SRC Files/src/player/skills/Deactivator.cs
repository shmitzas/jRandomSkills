using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Deactivator : ISkill
    {
        private static Skills skillName = Skills.Deactivator;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#919191", false);

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

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == skillName)
                        DisableSkill(player);
                }
                return HookResult.Continue;
            });
        }

        public static void TypeSkill(CCSPlayerController player, string[] commands)
        {
            if (player == null) return;

            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;
            if (!player.IsValid || !player.PawnIsAlive) return;

            string enemyId = commands[0];
            var enemy = Utilities.GetPlayers().FirstOrDefault(p => p.Team != player.Team && p.Index.ToString() == enemyId);

            if (enemy == null)
            {
                player.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("selectplayerskill_incorrect_enemy_index"));
                return;
            }

            DeacitvateSkill(player, enemy);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.PrintToChat(player, Localization.GetTranslation("deactivator") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("deactivator_select_info")}");
            var enemies = Utilities.GetPlayers().Where(p => p.Team != player.Team && p.IsValid && !p.IsBot).ToArray();
            if (enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                {
                    var enemyInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == enemy.SteamID);
                    if (enemyInfo == null) continue;
                    var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == enemyInfo.Skill);
                    player.PrintToChat($" {ChatColors.Green}⠀⠀⠀{ChatColors.Red}{enemy.Index}{ChatColors.Green}] {enemy.PlayerName}: {ChatColors.Red}{skillData.Name}");
                }
            }
            else
                player.PrintToChat($" {ChatColors.Red}⠀⠀⠀{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("selectplayerskill_command")} {ChatColors.Red}index");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            playerInfo.SpecialSkill = Skills.None;
        }

        private static void DeacitvateSkill(CCSPlayerController player, CCSPlayerController enemy)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            var enemyInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == enemy.SteamID);

            if (playerInfo != null)
            {
                playerInfo.Skill = Skills.None;
                playerInfo.SpecialSkill = skillName;
                player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("deactivator_player_info", enemy.PlayerName));
            }

            if (enemyInfo != null)
            {
                Instance.SkillAction(enemyInfo.Skill.ToString(), "DisableSkill", new object[] { enemy });
                enemyInfo.SpecialSkill = enemyInfo.Skill;
                enemyInfo.Skill = Skills.None;
                enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("deactivator_enemy_info"));
            }
        }
    }
}