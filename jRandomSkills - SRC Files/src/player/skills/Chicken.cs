using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Chicken : ISkill
    {
        private static Skills skillName = Skills.Chicken;
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
        private static readonly string defaultCTModel = "characters/models/ctm_sas/ctm_sas.vmdl";
        private static readonly string defaultTModel = "characters/models/tm_phoenix_heavy/tm_phoenix_heavy.vmdl";

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#FF8B42");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
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

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn?.Value;
            if (playerPawn != null)
            {
                SetPlayerModel(player, player.Team == CsTeam.CounterTerrorist ? defaultCTModel : defaultTModel);
                playerPawn.VelocityModifier = 1f;
                Utilities.SetStateChanged(player, "CCSPlayerPawn", "m_flVelocityModifier");

                playerPawn.Health += 50;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            }
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
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
    }
}