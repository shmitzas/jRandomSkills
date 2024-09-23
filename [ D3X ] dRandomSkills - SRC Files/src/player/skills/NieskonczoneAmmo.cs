using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class NieskonczoneAmmo
    {
        public static void LoadNieskonczoneAmmo()
        {
            Utils.RegisterSkill("Nieskończone Ammo", "Otrzymujesz nieskończoną ilość ammo do wszystkich swoich broni", "#0000FF");

            Instance.RegisterEventHandler<EventWeaponFire>((@event, info) =>
            {
                var player = @event.Userid;

                if (!IsValidPlayer(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == "Nieskończone Ammo")
                {
                    ApplyInfiniteAmmo(player);
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventWeaponReload>((@event, info) =>
            {
                var player = @event.Userid;

                if (!IsValidPlayer(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == "Nieskończone Ammo")
                {
                    ApplyInfiniteAmmo(player);
                }
                
                return HookResult.Continue;
            });
        }

        private static bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid;
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