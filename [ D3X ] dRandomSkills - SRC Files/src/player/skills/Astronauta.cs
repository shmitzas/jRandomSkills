using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Astronauta
    {
        public static void LoadAstronauta()
        {
            Utils.RegisterSkill("Astronauta", "Otrzymujesz losową ilość grawitacji na start rundy", "#7E10AD", false);
            
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
                            ApplyGravityModifier(player);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        private static void ApplyGravityModifier(CCSPlayerController player)
        {
            float gravityModifier = (float)Math.Round(Instance.Random.NextDouble() * (0.7f - 0.1f) + 0.1f, 1);
            Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"Astronauta\"{ChatColors.Lime}: Twoja losowa grawitacja wynosi: {gravityModifier}x", false);
            player.Pawn.Value.GravityScale = gravityModifier;
        }
    }
}