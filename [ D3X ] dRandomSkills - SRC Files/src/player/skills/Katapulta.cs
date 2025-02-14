using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Katapulta
    {

        public static void LoadKatapulta()
        {
            Utils.RegisterSkill("Katapulta", "Masz losow¹ szanse na podrzucenie wroga", "#FF4500");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Katapulta") continue;

                        float newChance = (float)Instance.Random.NextDouble() * (.40f - .20f) + .20f;
                        playerInfo.SkillChance = newChance;
                        newChance = (float)Math.Round(newChance, 2) * 100;
                        newChance = (float)Math.Round(newChance);

                        Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"Katapulta\"{ChatColors.Lime}: Twoje szanse na podrzucenie wroga po trafieniu to: {newChance}%", false);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == "Katapulta" && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= attackerInfo.SkillChance)
                    {
                        var victimPawn = victim.PlayerPawn?.Value;
                        if (victimPawn != null)
                        {
                            victimPawn.AbsVelocity.Z = 300f;
                        }
                    }
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