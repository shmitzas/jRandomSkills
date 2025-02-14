using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Drakula
    {
        public static void LoadDrakula()
        {
            Utils.RegisterSkill("Drakula", "Po trafieniu ofiary otrzymujesz zwrot zdrowia w postaci danego procentu zadanych obrażeń", "#FA050D");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!IsPlayerValid(attacker) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == "Drakula" && victim.PawnIsAlive)
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

            int newHealth = (int)(attackerPawn.Health + (damage * 0.3));

            attackerPawn.MaxHealth = Math.Max(newHealth, 100);
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iMaxHealth");

            attackerPawn.Health = newHealth;
            Utilities.SetStateChanged(attackerPawn, "CBaseEntity", "m_iHealth");
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}