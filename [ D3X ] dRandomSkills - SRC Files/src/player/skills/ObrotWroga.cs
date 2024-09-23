using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class ObrotWroga
    {
        public static void LoadObrotWroga()
        {
            Utils.RegisterSkill("Obrót Wroga", "Masz 25% szans na obrócenie wroga o 180 stopni po trafieniu", "#00FF00");

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!IsPlayerValid(attacker) || !IsPlayerValid(victim) || attacker == victim) 
                    return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == "Obrót Wroga" && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= 0.25)
                    {
                        RotateEnemy(victim);
                    }
                }
                
                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }

        private static void RotateEnemy(CCSPlayerController player)
        {
            if (player.PlayerPawn.Value.LifeState != (int)LifeState_t.LIFE_ALIVE)
                return;

            var currentPosition = player.PlayerPawn.Value.AbsOrigin;
            var currentAngles = player.PlayerPawn.Value.EyeAngles;

            QAngle newAngles = new QAngle(
                currentAngles.X,
                currentAngles.Y + 180,
                currentAngles.Z
            );

            player.PlayerPawn.Value.Teleport(currentPosition, newAngles, new Vector(0, 0, 0));
        }
    }
}