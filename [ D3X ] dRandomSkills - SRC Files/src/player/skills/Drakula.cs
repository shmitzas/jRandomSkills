using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Drakula
    {
        const float VampirePercentage = 0.3f;

        public static void LoadDrakula()
        {
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!IsPlayerValid(attacker) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == "Drakula" && attacker.PawnIsAlive)
                {
                    HealAttacker(attacker, @event.DmgHealth);
                }
                return HookResult.Continue;
            });
        }

        private static void HealAttacker(CCSPlayerController attacker, float damage)
        {
            var attackerPawn = attacker.PlayerPawn.Value;
            if (attackerPawn == null) return;

            int healing = (int)(damage * VampirePercentage);
            int newHealth = Math.Min(attackerPawn.Health + healing, attackerPawn.MaxHealth);

            attackerPawn.Health = newHealth;

            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iHealth");
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}