using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Kurczak
    {
        public static void LoadKurczak()
        {
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Kurczak") continue;

                        var playerPawn = player.PlayerPawn?.Value;
                        if (playerPawn != null)
                        {
                            SetPlayerModel(player, "models/chicken/chicken.vmdl");
                            playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().MaterialGroup.Value = (uint)Instance.Random.Next(1, 4);
                            playerPawn.VelocityModifier = 1.1f;
                            Utilities.SetStateChanged(player, "CCSPlayerPawn", "m_flVelocityModifier");
                        }
                    }
                });

                return HookResult.Continue;
            });
        }

        public static void SetPlayerModel(CCSPlayerController player, string model)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            Server.NextFrame(() =>
            {
                pawn.SetModel(model);

                Color originalRender = pawn.Render;
                pawn.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}