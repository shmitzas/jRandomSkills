using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class AntyFlash
    {
        public static void LoadAntyFlash()
        {
            Instance.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;
                var attacker = @event.Attacker;
                
                var playerPawn = player.PlayerPawn.Value;

                if (!IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                var sameTeam = attacker.Team == player.Team;

                if (playerInfo?.Skill == "Anty Flash")
                {
                    playerPawn.FlashDuration = 0.0f;
                }

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null && player.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE;
        }
    }
}