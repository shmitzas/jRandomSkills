using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class LifeSwap : ISkill
    {
        private const Skills skillName = Skills.LifeSwap;

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

            SwapHealth(player, enemy);
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("lifeswap_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("lifeswap_enemy_info"));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("lifeswap") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("lifeswap_select_info")}");
            var enemies = Utilities.GetPlayers().Where(p => p.Team != player.Team && p.IsValid && !p.IsBot && !p.IsHLTV && p.Team != CsTeam.Spectator).ToArray();
            if (enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                    player.PrintToChat($" {ChatColors.Green}⠀⠀⠀[{ChatColors.Red}{enemy.Index}{ChatColors.Green}] {enemy.PlayerName}");
            }
            else
                player.PrintToChat($" {ChatColors.Red}⠀⠀⠀{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("selectplayerskill_command")} {ChatColors.Red}index");
        }

        private static void SwapHealth(CCSPlayerController player, CCSPlayerController enemy)
        {
            var playerPawn = player.PlayerPawn.Value;
            var enemyPawn = enemy.PlayerPawn.Value;

            if (playerPawn == null || !playerPawn.IsValid || enemyPawn == null || !enemyPawn.IsValid) return;
            if (playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE || enemyPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;

            (enemyPawn.Health, playerPawn.Health) = (playerPawn.Health, enemyPawn.Health);
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(enemyPawn, "CBaseEntity", "m_iHealth");
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#a3651a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}