using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Teleporter
    {
        public static void LoadTeleporter()
        {
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;

                if (!IsValidPlayer(victim) || !IsValidPlayer(attacker)) return HookResult.Continue;

                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == "Teleportator")
                {
                    TeleportPlayers(attacker, victim);
                }

                return HookResult.Continue;
            });
        }

        private static bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn != null && player.PlayerPawn.Value != null;
        }

        private static void TeleportPlayers(CCSPlayerController attacker, CCSPlayerController victim)
        {
            var attackerPawn = attacker.PlayerPawn.Value;
            var victimPawn = victim.PlayerPawn.Value;

            var attackerPosition = attackerPawn.AbsOrigin;
            var attackerAngles = attackerPawn.AbsRotation;
            var attackerVelocity = attackerPawn.AbsVelocity;

            var victimPosition = victimPawn.AbsOrigin;
            var victimAngles = victimPawn.AbsRotation;
            var victimVelocity = victimPawn.AbsVelocity;

            attackerPawn.Teleport(victimPosition, victimAngles, victimVelocity);
            victimPawn.Teleport(attackerPosition, attackerAngles, attackerVelocity);
        }
    }
}
