using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Medyk
    {
        public static void LoadMedyk()
        {
            Utils.RegisterSkill("Medyk", "Otrzymujesz losową ilość apteczek na start rundy", "#42FF5F");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) return;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Medyk") return;

                        int healthshot = Instance.Random.Next(1, 10);
                        for (int i = 0; i < healthshot; i++)
                        {
                            player.GiveNamedItem("weapon_healthshot");
                        }
                    }
                });

                return HookResult.Continue;
            });
            
            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!IsPlayerValid(player)) return HookResult.Continue;

                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill != "Medyk") return HookResult.Continue;

                    player.RemoveItemByDesignerName("weapon_healthshot");
                }
                
                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}