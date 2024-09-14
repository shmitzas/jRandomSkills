using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Astronauta
    {
        public static void LoadAstronauta()
        {
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        var playerPawn = player.PlayerPawn.Value;
                        
                        if (playerInfo?.Skill == "Astronauta" && playerPawn != null)
                        {
                            ApplyGravityModifier(playerPawn);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        private static void ApplyGravityModifier(CCSPlayerPawn playerPawn)
        {
            float gravityModifier = (float)Math.Round(Instance.Random.NextDouble() * (0.7f - 0.2f) + 0.2f, 1);
            playerPawn.GravityScale = gravityModifier;
        }
    }
}