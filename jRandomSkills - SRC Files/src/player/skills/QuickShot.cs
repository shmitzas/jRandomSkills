using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Memory;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class QuickShot : ISkill
    {
        private static Skills skillName = Skills.QuickShot;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#8a42f5");
            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Instance.IsPlayerValid(player)) continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    var weapon = player.Pawn.Value.WeaponServices?.ActiveWeapon.Value;
                    if (weapon == null) continue;

                    var pawn = player.PlayerPawn.Value;
                    pawn.AimPunchTickBase = 0;
                    pawn.AimPunchTickFraction = 0f;
                    pawn.CameraServices.CsViewPunchAngleTick = 0;
                    pawn.CameraServices.CsViewPunchAngleTickRatio = 0f;

                    Schema.SetSchemaValue<Int32>(weapon.Handle, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick", Server.TickCount);
                    Schema.SetSchemaValue<Int32>(weapon.Handle, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick", Server.TickCount);
                }
            }
        }
    }
}