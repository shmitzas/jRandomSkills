using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class AntyFlash
    {
        public static void LoadAntyFlash()
        {
            Utils.RegisterSkill("Anty Flash", "Posiadasz odporność na flashe i 7 sekund trwają twoje flash'e", "#D6E6FF");
            
            Instance.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;
                var attacker = @event.Attacker;
                
                var playerPawn = player.PlayerPawn.Value;

                if (!IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == "Anty Flash")
                    playerPawn.FlashDuration = 0.0f;
                else if (Instance.skillPlayer.Any(s => s.Skill == "Anty Flash"))
                    playerPawn.FlashDuration = 7.0f;

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null && player.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE;
        }
    }
}