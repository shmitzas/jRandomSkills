using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Flash
    {
        public static void LoadFlash()
        {
            Utils.RegisterSkill("Flash", "Losowa prędkośc postaci na początku rundy", "#A31912", false);
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Flash") continue;

                        var playerPawn = player.PlayerPawn?.Value;

                        if (playerPawn != null)
                        {
                            float newSpeed = (float)Instance.Random.NextDouble() * (3.0f - 1.2f) + 1.2f;
                            newSpeed = (float)Math.Round(newSpeed, 2);
                            playerPawn.VelocityModifier = newSpeed;
                            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
                            Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"Flash\"{ChatColors.Lime}: Twój mnożnik prędkości to {newSpeed}x", false);
                        }
                    }
                });
                
                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}