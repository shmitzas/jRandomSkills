using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class NoRecoil : ISkill
    {
        private static Skills skillName = Skills.NoRecoil;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#8a42f5");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        player.RemoveItemByDesignerName("weapon_healthshot");
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                DisableSkill(null);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            Server.ExecuteCommand("weapon_accuracy_nospread 1");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            Server.ExecuteCommand("weapon_accuracy_nospread 0");
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
                }
            }
        }
    }
}