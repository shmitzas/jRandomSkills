using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class MiniMajk
    {
        public static void LoadMiniMajk()
        {
            string defaultCT = "characters/models/ctm_sas/ctm_sas.vmdl";
            string defaultTT = "characters/models/tm_phoenix/tm_phoenix.vmdl";
            Utils.RegisterSkill("MiniMajk", "Losowa wielkość postaci na początku rundy", "#ffff00", false);

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "MiniMajk") continue;

                        var playerPawn = player.PlayerPawn?.Value;

                        if (playerPawn != null)
                        {
                            float newSize = (float)Instance.Random.NextDouble() * (.95f - .6f) + .6f;
                            newSize = (float)Math.Round(newSize, 2);

                            playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().Scale = newSize;
                            playerPawn.SetModel(player.Team == CsTeam.CounterTerrorist ? defaultTT : defaultCT);
                            playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().Scale = newSize;
                            playerPawn.SetModel(player.Team == CsTeam.CounterTerrorist ? defaultCT : defaultTT);

                            Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"MiniMajk\"{ChatColors.Lime}: Twój mnożnik wielkości to {newSize}x", false);

                            Instance.AddTimer(.2f, () => {
                                if (IsPlayerValid(player))
                                    playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().Scale = 1;
                            });
                        }
                    }
                });

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}