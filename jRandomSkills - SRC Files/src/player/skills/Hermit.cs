using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Hermit : ISkill
    {
        private static Skills skillName = Skills.Hermit;
        private static readonly Dictionary<string, int> maxReserveAmmo = new Dictionary<string, int>
        {
            { "weapon_glock", 120 },
            { "weapon_usp_silencer", 24 },
            { "weapon_hkp2000", 52 },
            { "weapon_p250", 26 },
            { "weapon_cz75", 12 },
            { "weapon_deagle", 35 },
            { "weapon_fiveseven", 100 },
            { "weapon_elite", 120 },
            { "weapon_tec9", 90 },
            { "weapon_revolver", 8 },
            { "weapon_mac10", 100 },
            { "weapon_mp9", 120 },
            { "weapon_mp7", 120 },
            { "weapon_mp5", 120 },
            { "weapon_mp5sd", 120 },
            { "weapon_ump45", 100 },
            { "weapon_p90", 100 },
            { "weapon_bizon", 120 },
            { "weapon_ak47", 90 },
            { "weapon_m4a1", 90 },
            { "weapon_m4a1_silencer", 80 },
            { "weapon_galilar", 90 },
            { "weapon_famas", 90 },
            { "weapon_aug", 90 },
            { "weapon_sg556", 90 },
            { "weapon_ssg08", 90 },
            { "weapon_awp", 30 },
            { "weapon_scar20", 90 },
            { "weapon_g3sg1", 90 },
            { "weapon_nova", 32 },
            { "weapon_xm1014", 32 },
            { "weapon_sawedoff", 32 },
            { "weapon_mag7", 32 },
            { "weapon_m249", 200 },
            { "weapon_negev", 300 }
        };

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#ded678");
            
            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var attacker = @event.Attacker;
                if (!Instance.IsPlayerValid(attacker)) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                if (attackerInfo?.Skill != skillName) return HookResult.Continue;

                var pawn = attacker.PlayerPawn.Value;
                if (pawn == null || !pawn.IsValid) return HookResult.Continue;

                var weapon = pawn.WeaponServices.ActiveWeapon.Value;
                if (weapon == null || !weapon.IsValid) return HookResult.Continue;

                var maxReserveAmmoClip = maxReserveAmmo.TryGetValue(weapon.DesignerName, out var reserve) ? reserve : 100;
                weapon.Clip1 = weapon.VData.MaxClip1;
                weapon.ReserveAmmo.Fill(maxReserveAmmoClip);
                
                Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
                Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
                SkillUtils.AddHealth(pawn, 25);

                return HookResult.Continue;
            });
        }
    }
}