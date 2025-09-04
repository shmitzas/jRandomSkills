using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class MoneySwap : ISkill
    {
        private const Skills skillName = Skills.MoneySwap;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);

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
        }

        public static void TypeSkill(CCSPlayerController player, string[] commands)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            if (playerInfo.SkillChance == 1)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_used_info")}");
                return;
            }

            string enemyId = commands[0];
            var enemy = Utilities.GetPlayers().FirstOrDefault(p => p.Team != player.Team && p.Index.ToString() == enemyId);

            if (enemy == null)
            {
                player.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("selectplayerskill_incorrect_enemy_index"));
                return;
            }

            SwapMoney(player, enemy);
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("moneyswap_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("moneyswap_enemy_info"));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("moneyswap") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("moneyswap_select_info")}");
            var enemies = Utilities.GetPlayers().Where(p => p.Team != player.Team && p.IsValid && !p.IsBot).ToArray();
            if (enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                    if (enemy != null && enemy.IsValid && enemy.InGameMoneyServices != null)
                        player.PrintToChat($" {ChatColors.Green}⠀⠀⠀[{ChatColors.Red}{enemy.Index}{ChatColors.Green}] {enemy.PlayerName}: ${enemy.InGameMoneyServices.Account}");
            }
            else
                player.PrintToChat($" {ChatColors.Red}⠀⠀⠀{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("selectplayerskill_command")} {ChatColors.Red}index");
        }

        private static void SwapMoney(CCSPlayerController player, CCSPlayerController enemy)
        {
            if (player == null || !player.IsValid || enemy == null || !enemy.IsValid) return;

            var playerMoneyServices = player.InGameMoneyServices;
            var enemyMoneyServices = enemy.InGameMoneyServices;
            if (playerMoneyServices == null || enemyMoneyServices == null) return;

            int playerMoney = playerMoneyServices.Account;
            playerMoneyServices.Account = enemyMoneyServices.Account;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");

            enemyMoneyServices.Account = playerMoney;
            Utilities.SetStateChanged(enemy, "CCSPlayerController", "m_pInGameMoneyServices");
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#52f54c", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}