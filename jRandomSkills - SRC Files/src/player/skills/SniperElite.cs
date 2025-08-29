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
        private static readonly Dictionary<ulong, string> sniperElites = new Dictionary<ulong, string>();

        private static string[] rifles = { "weapon_mp9", "weapon_mac10", "weapon_bizon", "weapon_mp7", "weapon_ump45", "weapon_p90",
        "weapon_mp5sd", "weapon_famas", "weapon_galilar", "weapon_m4a1", "weapon_m4a1_silencer", "weapon_ak47",
        "weapon_aug", "weapon_sg553", "weapon_ssg08", "weapon_awp", "weapon_scar20", "weapon_g3sg1",
        "weapon_nova", "weapon_xm1014", "weapon_mag7", "weapon_sawedoff", "weapon_m249", "weapon_negev" };

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                sniperElites.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    if (sniperElites.ContainsKey(player.SteamID))
                        sniperElites.Remove(player.SteamID);

                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            sniperElites.Add(player.SteamID, "weapon_awp");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (sniperElites.ContainsKey(player.SteamID))
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
            if (sniperElites.TryGetValue(player.SteamID, out string weapon))
                holdedWeapon = weapon;

            try
            {
                string activeRifle = GetActiveRifle(player);
                if (activeRifle != null)
                    player.RemoveItemByDesignerName(activeRifle, true);
                sniperElites[player.SteamID] = activeRifle;
                Server.NextFrame(() =>
                {
                    player.GiveNamedItem(holdedWeapon ?? "weapon_awp");
                });
            }
            catch { }
        }

        private static string GetActiveRifle(CCSPlayerController player)
        {
            string rifle = null;
            var weapons = player.PlayerPawn.Value.WeaponServices.MyWeapons;
            if (weapons == null) return rifle;

            foreach (var weapon in weapons)
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                    if (rifles.Contains(weapon.Value.DesignerName))
                        rifle = weapon.Value.DesignerName;
            return rifle;
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public SkillConfig(Skills skill = skillName, bool active = false, string color = "#e0873a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : base(skill, active, color, onlyTeam, needsTeammates)
            {
            }
        }
    }
}