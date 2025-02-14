using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Phoenix
    {
        public static void LoadPhoenix()
        {
            Utils.RegisterSkill("Phoenix", "Masz losową szans na odrodzenie się po śmierci", "#ff5C0A", false);

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Phoenix") continue;

                        float newChance = (float)Instance.Random.NextDouble() * (.40f - .20f) + .20f;
                        playerInfo.SkillChance = newChance;
                        newChance = (float)Math.Round(newChance, 2) * 100;
                        newChance = (float)Math.Round(newChance);

                        Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"Phoenix\"{ChatColors.Lime}: Twoje szanse na odrodzenie się po śmierci: {newChance}%", false);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                if (!IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == "Phoenix")
                {
                    if (Instance.Random.NextDouble() <= playerInfo.SkillChance)
                    {
                        player.Respawn();
                        Instance.AddTimer(.2f, () => player.Respawn());
                        Utils.PrintToChat(player, $"Zostałeś odrodzony z popiołów dzięki mocy: {ChatColors.DarkRed}Phoenix", false);
                    }
                }

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid;
        }
    }
}