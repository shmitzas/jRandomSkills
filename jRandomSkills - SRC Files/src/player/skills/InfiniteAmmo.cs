using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class InfiniteAmmo : ISkill
    {
        private static Skills skillName = Skills.InfiniteAmmo;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#0000FF");

            Instance.RegisterEventHandler<EventWeaponFire>((@event, info) =>
            {
                var player = @event.Userid;

                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    ApplyInfiniteAmmo(player);
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventGrenadeThrown>((@event, info) =>
            {
                var player = @event.Userid;

                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                    player.GiveNamedItem($"weapon_{@event.Weapon}");

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventWeaponReload>((@event, info) =>
            {
                var player = @event.Userid;

                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    ApplyInfiniteAmmo(player);
                }
                
                return HookResult.Continue;
            });
        }

        private static void ApplyInfiniteAmmo(CCSPlayerController player)
        {
            var activeWeaponHandle = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon;
            if (activeWeaponHandle?.Value != null)
            {
                activeWeaponHandle.Value.Clip1 = 100;
            }
        }
    }
}