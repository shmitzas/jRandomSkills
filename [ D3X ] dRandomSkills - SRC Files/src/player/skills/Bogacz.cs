using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Bogacz
    {
        public static void LoadBogacz()
        {
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        
                        if (playerInfo?.Skill == "Bogacz")
                        {
                            ApplyMoneyBonus(player);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        private static void ApplyMoneyBonus(CCSPlayerController player)
        {
            int moneyBonus = Instance.Random.Next(5000, 15000);
            AddMoney(player, moneyBonus);
        }

        private static void AddMoney(CCSPlayerController player, int money)
        {
            var moneyServices = player?.InGameMoneyServices;

            if (moneyServices == null) return;

            moneyServices.Account += money;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }
    }
}