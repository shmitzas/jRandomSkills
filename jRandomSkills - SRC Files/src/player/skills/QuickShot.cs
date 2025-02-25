using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Memory;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
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

            Utils.RegisterSkill(skillName, "#8a42f5");
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

                    Schema.SetSchemaValue<Int32>(weapon.Handle, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick", Server.TickCount);
                    Schema.SetSchemaValue<Int32>(weapon.Handle, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick", Server.TickCount);
                }
            }
        }
    }
}