using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class SniperElite : ISkill
    {
        private const Skills skillName = Skills.SniperElite;
        private static readonly Dictionary<ulong, string> sniperElites = [];

        private static readonly string[] rifles = [ "weapon_mp9", "weapon_mac10", "weapon_bizon", "weapon_mp7", "weapon_ump45", "weapon_p90",
        "weapon_mp5sd", "weapon_famas", "weapon_galilar", "weapon_m4a1", "weapon_m4a1_silencer", "weapon_ak47",
        "weapon_aug", "weapon_sg553", "weapon_ssg08", "weapon_awp", "weapon_scar20", "weapon_g3sg1",
        "weapon_nova", "weapon_xm1014", "weapon_mag7", "weapon_sawedoff", "weapon_m249", "weapon_negev" ];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            sniperElites.Clear();
        }

        public static void PlayerDeath(EventPlayerDeath @event)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill == skillName)
                sniperElites.Remove(player.SteamID);

        }

        public static void EnableSkill(CCSPlayerController player)
        {
            sniperElites.Add(player.SteamID, "weapon_awp");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            sniperElites.Remove(player.SteamID);
        }

        public static void UseSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;

            if (sniperElites.ContainsKey(player.SteamID))
                RemoveAndGiveWeapon(player);
        }

        private static void RemoveAndGiveWeapon(CCSPlayerController player)
        {
            string holdedWeapon = "weapon_awp";
            if (sniperElites.TryGetValue(player.SteamID, out string? weapon))
                if (!string.IsNullOrEmpty(weapon))
                    holdedWeapon = weapon;

            try
            {
                string? activeRifle = GetActiveRifle(player);
                if (!string.IsNullOrEmpty(activeRifle))
                    player.RemoveItemByDesignerName(activeRifle, true);
                sniperElites[player.SteamID] = activeRifle ?? "weapon_awp";
                Server.NextFrame(() => player.GiveNamedItem(holdedWeapon ?? "weapon_awp") );
            }
            catch { }
        }

        private static string? GetActiveRifle(CCSPlayerController player)
        {
            string rifle = string.Empty;
            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) return rifle;
            var weaponServices = pawn.WeaponServices;
            if (weaponServices == null) return rifle;

            foreach (var weapon in weaponServices.MyWeapons)
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                    if (rifles.Contains(weapon.Value.DesignerName))
                        rifle = SkillUtils.GetDesignerName(weapon.Value);
            return rifle;
        }

        public class SkillConfig(Skills skill = skillName, bool active = false, string color = "#e0873a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}