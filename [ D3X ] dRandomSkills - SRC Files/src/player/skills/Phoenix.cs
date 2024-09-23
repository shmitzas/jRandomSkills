using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Phoenix
    {
        private const double ReviveChance = 0.35;

        public static void LoadPhoenix()
        {
            Utils.RegisterSkill("Phoenix", "Masz 35% szans na odrodzenie się po śmierci", "#ff5C0A");

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                if (!IsValidPlayer(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == "Phoenix")
                {
                    if (Instance.Random.NextDouble() <= ReviveChance)
                    {
                        player.Respawn();
                        Utils.PrintToChat(player, $"Zostałeś odrodzony z popiołów dzięki mocy: {ChatColors.DarkRed}Phoenix", false);
                    }
                }

                return HookResult.Continue;
            });
        }

        private static bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid;
        }
    }
}