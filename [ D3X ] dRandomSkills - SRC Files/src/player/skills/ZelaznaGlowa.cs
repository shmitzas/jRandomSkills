using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class ZelaznaGlowa
    {

        public static void LoadZelaznaGlowa()
        {
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                int hitgroup = @event.Hitgroup;

                if (!IsValidPlayer(attacker) || !IsValidPlayer(victim) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);

                if (playerInfo?.Skill == "Żelazna Głowa" && hitgroup != 3)
                {
                    ApplyIronHeadEffect(victim, @event.DmgHealth);
                    return HookResult.Stop;
                }

                return HookResult.Continue;
            });
        }

        private static bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn != null && player.PlayerPawn.Value != null;
        }

        private static void ApplyIronHeadEffect(CCSPlayerController victim, float damage)
        {
            var playerPawn = victim.PlayerPawn.Value;
            var newHealth = playerPawn.Health + damage;

            if (newHealth > 100)
                newHealth = 100;

            playerPawn.Health = (int)newHealth;
        }
    }
}