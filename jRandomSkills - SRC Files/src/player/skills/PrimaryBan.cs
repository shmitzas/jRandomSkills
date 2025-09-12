using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class PrimaryBan : ISkill
    {
        private const Skills skillName = Skills.PrimaryBan;
        private static readonly HashSet<ulong> bannedPlayers = [];
        private static readonly string[] disabledWeapons =
        [
            "ak47", "m4a1", "m4a4", "m4a1_silencer", "famas", "galilar", "aug", "sg553", "mp9", "mac10", "bizon", "mp7", "ump45",
            "p90", "mp5sd", "ssg08", "awp", "scar20", "g3sg1", "nova", "xm1014", "mag7", "sawedoff", "m249", "negev"
        ];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            bannedPlayers.Clear();
            foreach (var player in Utilities.GetPlayers())
                SkillUtils.CloseMenu(player);
        }

        public static void WeaponEquip(EventItemEquip @event)
        {
            var player = @event.Userid;
            var weapon = @event.Item;
            if (player == null || !player.IsValid) return;
            if (!bannedPlayers.Contains(player.SteamID) || !disabledWeapons.Contains(weapon)) return;
            player.ExecuteClientCommand("slot3");
        }

        public static void OnTick()
        {
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

            bannedPlayers.Add(enemy.SteamID);
            CheckWeapon(enemy);
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("primaryban_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("primaryban_enemy_info"));
        }

        private static void CheckWeapon(CCSPlayerController player)
        {
            var activeWeapon = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon?.Value;
            if (activeWeapon == null || !activeWeapon.IsValid) return;
            if (activeWeapon.DesignerName == null || string.IsNullOrEmpty(activeWeapon.DesignerName)) return;

            if (!bannedPlayers.Contains(player.SteamID) || !disabledWeapons.Contains(activeWeapon.DesignerName?.Replace("weapon_", ""))) return;
            player.ExecuteClientCommand("slot3");
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
            bannedPlayers.Remove(player.SteamID);
            SkillUtils.CloseMenu(player);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#ffc061", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}