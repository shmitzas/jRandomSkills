using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Cien
    {
        private const float TeleportDistance = 100.0f;

        public static void LoadCien()
        {
            Utils.RegisterSkill("Cień", "Teleportujesz się za plecy losowego wroga", "#18171A");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!IsPlayerValid(attacker) || !IsPlayerValid(victim)) return HookResult.Continue;

                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == "Cień")
                {
                    TeleportAttackerBehindVictim(attacker, victim);
                }

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }

        private static void TeleportAttackerBehindVictim(CCSPlayerController attacker, CCSPlayerController victim)
        {
            var victimPawn = victim.PlayerPawn.Value;
            var attackerPawn = attacker.PlayerPawn.Value;

            if (victimPawn == null || attackerPawn == null) return;

            Vector victimPosition = victimPawn.AbsOrigin;
            QAngle victimAngles = victimPawn.AbsRotation;

            Vector behindPosition = victimPosition - GetForwardVector(victimAngles) * TeleportDistance;

            attackerPawn.Teleport(behindPosition, victimAngles, new Vector(0, 0, 0));
        }

        private static Vector GetForwardVector(QAngle angles)
        {
            float pitch = angles.X * (float)(Math.PI / 180);
            float yaw = angles.Y * (float)(Math.PI / 180);

            float x = (float)(Math.Cos(pitch) * Math.Cos(yaw));
            float y = (float)(Math.Cos(pitch) * Math.Sin(yaw));
            float z = (float)Math.Sin(pitch);

            return new Vector(x, y, z);
        }
    }
}
