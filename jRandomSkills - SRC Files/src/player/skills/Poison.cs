using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Poison : ISkill
    {
        private const Skills skillName = Skills.Poison;
        private static readonly float cooldown = Config.GetValue<float>(skillName, "Cooldown");
        private static readonly int healthToDamage = Config.GetValue<int>(skillName, "Damage");
        private static readonly HashSet<CCSPlayerController> poisonedPlayers = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);
        }

        public static void NewRound()
        {
            poisonedPlayers.Clear();
        }

        public static void OnTick()
        {
            if (Server.TickCount % (int)(64 * cooldown) == 0)
            {
                foreach (var player in poisonedPlayers)
                {
                    var pawn = player.PlayerPawn.Value;
                    if (pawn == null || !pawn.IsValid) continue;
                    SkillUtils.TakeHealth(pawn, healthToDamage);
                }
            }

            if (Server.TickCount % 32 != 0) return;
            foreach (var player in Utilities.GetPlayers())
            {
                if (!SkillUtils.HasMenu(player)) continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo == null || playerInfo.Skill != skillName) continue;
                var enemies = Utilities.GetPlayers().Where(p => p.PawnIsAlive && p.Team != player.Team && p.IsValid && !p.IsBot && !p.IsHLTV && p.Team != CsTeam.Spectator && p.Team != CsTeam.None).ToArray();

                HashSet<(string, string)> menuItems = enemies.Select(e => (e.PlayerName, e.Index.ToString())).ToHashSet();
                SkillUtils.UpdateMenu(player, menuItems);
            }
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

            poisonedPlayers.Add(enemy);
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("poison_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("poison_enemy_info"));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo == null) return;
            playerInfo.SkillChance = 0;

            var enemies = Utilities.GetPlayers().Where(p => p.PawnIsAlive && p.Team != player.Team && p.IsValid && !p.IsBot && !p.IsHLTV && p.Team != CsTeam.Spectator && p.Team != CsTeam.None).ToArray();
            if (enemies.Length > 0)
            {
                HashSet<(string, string)> menuItems = enemies.Select(e => (e.PlayerName, e.Index.ToString())).ToHashSet();
                SkillUtils.CreateMenu(player, menuItems);
            }
            else
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            poisonedPlayers.Remove(player);
            SkillUtils.CloseMenu(player);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#902eff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 1, int damage = 1) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int Damage { get; set; } = damage;
            public float Cooldown { get; set; } = cooldown;
        }
    }
}