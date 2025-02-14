using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static CounterStrikeSharp.API.Core.Listeners;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Kurczak
    {
        private static string[] pistols =
        {
            "weapon_deagle",
            "weapon_revolver",
            "weapon_glock",
            "weapon_usp_silencer",
            "weapon_cz75a",
            "weapon_fiveseven",
            "weapon_p250",
            "weapon_tec9",
            "weapon_elite",
            "weapon_hkp2000"
        };

        public static void LoadKurczak()
        {
            Utils.RegisterSkill("Kurczak", "Otrzymujesz model kurczaka + jesteś o 10% szybszy - 50hp", "#FF8B42");
            
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

                            playerPawn.Health = 50;
                            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                        }
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == "Kurczak")
                {
                    var activeWeapon = player.Pawn.Value.WeaponServices?.ActiveWeapon.Value;
                    if (activeWeapon != null && activeWeapon.IsValid && activeWeapon.Clip1 != 0 && !pistols.Contains(activeWeapon?.DesignerName))
                    {
                        activeWeapon.Clip1 = 0;
                        activeWeapon.Clip2 = 0;
                    }
                }
            }
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