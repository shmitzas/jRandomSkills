using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Hermit : ISkill
    {
        private const Skills skillName = Skills.Hermit;
        private static readonly Dictionary<string, int> dictionary = new()
        {
            { "weapon_glock", 120 }, { "weapon_usp_silencer", 24 }, { "weapon_hkp2000", 52 }, { "weapon_p250", 26 },
            { "weapon_cz75", 12 }, { "weapon_deagle", 35 }, { "weapon_fiveseven", 100 }, { "weapon_elite", 120 },
            { "weapon_tec9", 90 }, { "weapon_revolver", 8 }, { "weapon_mac10", 100 }, { "weapon_mp9", 120 },
            { "weapon_mp7", 120 }, { "weapon_mp5", 120 }, { "weapon_mp5sd", 120 }, { "weapon_ump45", 100 },
            { "weapon_p90", 100 }, { "weapon_bizon", 120 }, { "weapon_ak47", 90 }, { "weapon_m4a1", 90 },
            { "weapon_m4a1_silencer", 80 }, { "weapon_galilar", 90 }, { "weapon_famas", 90 }, { "weapon_aug", 90 },
            { "weapon_sg556", 90 }, { "weapon_ssg08", 90 }, { "weapon_awp", 30 }, { "weapon_scar20", 90 },
            { "weapon_g3sg1", 90 }, { "weapon_nova", 32 }, { "weapon_xm1014", 32 }, { "weapon_sawedoff", 32 },
            { "weapon_mag7", 32 }, { "weapon_m249", 200 }, { "weapon_negev", 300 }
        };
        private static readonly Dictionary<string, int> maxReserveAmmo = dictionary;

        private static readonly int healthToAdd = Config.GetValue<int>(skillName, "healthToAdd");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void PlayerDeath(EventPlayerDeath @event)
        {
            var attacker = @event.Attacker;
            if (!Instance.IsPlayerValid(attacker)) return;

            var attackerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == attacker?.SteamID);
            if (attackerInfo?.Skill != skillName) return;

            var pawn = attacker!.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid || pawn.WeaponServices == null) return;

            var weapon = pawn.WeaponServices.ActiveWeapon.Value;
            if (weapon == null || !weapon.IsValid || weapon.VData == null) return;

            var maxReserveAmmoClip = maxReserveAmmo.TryGetValue(weapon.DesignerName, out var reserve) ? reserve : 100;
            weapon.Clip1 = weapon.VData.MaxClip1;
            weapon.ReserveAmmo.Fill(maxReserveAmmoClip);

            Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
            Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
            SkillUtils.AddHealth(pawn, healthToAdd);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#ded678", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int healthToAdd = 25) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int HealthToAdd { get; set; } = healthToAdd;
        }
    }
}