using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Katapulta
    {

        public static void LoadKatapulta()
        {
            Utils.RegisterSkill("Katapulta", "Masz 25% szans na podrzucenie wroga", "#FF4500");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == "Katapulta" && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= 0.25)
                    {
                        var victimPawn = victim.PlayerPawn?.Value;
                        if (victimPawn != null)
                        {
                            victimPawn.AbsVelocity.Z = 300f;
                        }
                    }
                }
                
                return HookResult.Continue;
            });
        }
    }
}