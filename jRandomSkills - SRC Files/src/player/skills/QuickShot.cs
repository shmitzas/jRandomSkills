using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class QuickShot : ISkill
    {
        private const Skills skillName = Skills.QuickShot;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
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

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#8a42f5", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : base(skill, active, color, onlyTeam, needsTeammates)
            {
            }
        }
    }
}