using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Rambo
    {
        public static void LoadRambo()
        {
            Utils.RegisterSkill("Rambo", "Otrzymujesz losową ilość zdrowia na start rundy", "#009905");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsValidPlayer(player)) continue;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == "Rambo")
                        {
                            int healthBonus = Instance.Random.Next(50, 501);
                            AddHealth(player, healthBonus);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        private static bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid;
        }

        public static void AddHealth(CCSPlayerController player, int health)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            pawn.MaxHealth = Math.Min(pawn.Health + health, 1000);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");

            pawn.Health = pawn.MaxHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }
    }
}