using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Numerics;
using static dRandomSkills.dRandomSkills;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace dRandomSkills
{
    public static class Teleporter
    {
        public static void LoadTeleporter()
        {
            Utils.RegisterSkill("Teleportator", "Zamieniasz się miejscami z trafionym wrogiem", "#8A2BE2");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;

                if (!IsValidPlayer(victim) || !IsValidPlayer(attacker)) return HookResult.Continue;

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

            Vector attackerPosition = new Vector(attackerPawn.AbsOrigin.X, attackerPawn.AbsOrigin.Y, attackerPawn.AbsOrigin.Z);
            QAngle attackerAngles = new QAngle(attackerPawn.AbsRotation.X, attackerPawn.AbsRotation.Y, attackerPawn.AbsRotation.Z);
            Vector attackerVelocity = new Vector(attackerPawn.AbsVelocity.X, attackerPawn.AbsVelocity.Y, attackerPawn.AbsVelocity.Z);

            Vector victimPosition = new Vector(victimPawn.AbsOrigin.X, victimPawn.AbsOrigin.Y, victimPawn.AbsOrigin.Z);
            QAngle victimAngles = new QAngle(victimPawn.AbsRotation.X, victimPawn.AbsRotation.Y, victimPawn.AbsRotation.Z);
            Vector victimVelocity = new Vector(victimPawn.AbsVelocity.X, victimPawn.AbsVelocity.Y, victimPawn.AbsVelocity.Z);

            victimPawn.Teleport(attackerPosition, attackerAngles, attackerVelocity);
            attackerPawn.Teleport(victimPosition, victimAngles, victimVelocity);
        }
    }
}
