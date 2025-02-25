using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Zeus : ISkill
    {
        private static Skills skillName = Skills.Zeus;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#fbff00");

            Instance.RegisterEventHandler<EventWeaponFire>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    var activeWeapon = player.Pawn.Value.WeaponServices.ActiveWeapon.Value;
                    if (activeWeapon?.DesignerName != "weapon_taser") return HookResult.Continue;
                    var taser = activeWeapon.As<CWeaponTaser>();
                    Instance.AddTimer(.1f, () =>
                    {
                        if (taser.IsValid)
                        {
                            taser.LastAttackTick = 0;
                            taser.FireTime = 0;
                        }
                    });
                }

                return HookResult.Continue;
            });
        }
    }
}