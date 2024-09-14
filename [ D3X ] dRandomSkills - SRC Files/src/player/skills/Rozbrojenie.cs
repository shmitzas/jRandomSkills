using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Rozbrojenie
    {

        public static void LoadRozbrojenie()
        {
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == "Rozbrojenie" && attacker.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= 0.25)
                    {
                        var weaponServices = victim.PlayerPawn?.Value?.WeaponServices;
                        if (weaponServices?.MyWeapons == null) return HookResult.Continue;

                        foreach (var weapon in weaponServices.MyWeapons)
                        {
                            var weaponName = weapon.Value.DesignerName;
                            if (weaponName != null && !weaponName.Contains("weapon_knife") && !weaponName.Contains("weapon_c4"))
                            {
                                victim.DropActiveWeapon();
                            }
                        }
                    }
                }
                return HookResult.Continue;
            });
        }
    }
}