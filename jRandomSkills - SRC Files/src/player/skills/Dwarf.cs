using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Dwarf : ISkill
    {
        private static Skills skillName = Skills.Dwarf;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#ffff00", false);

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
                        player.Respawn();
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        DisableSkill(player);
                    }
                });

                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn?.Value;
            if (playerPawn != null)
            {
                var skillConfig = Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString());
                if (skillConfig == null) return;

                float newSize = (float)Instance.Random.NextDouble() * (skillConfig.ChanceTo - skillConfig.ChanceFrom) + skillConfig.ChanceFrom;
                newSize = (float)Math.Round(newSize, 2);

                playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().Scale = newSize;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");

                Utils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("dwarf")}{ChatColors.Lime}: " + Localization.GetTranslation("dwarf_desc2", newSize), false);
            }
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn?.Value;
            if (playerPawn != null && playerPawn?.CBodyComponent != null)
            {
                playerPawn.CBodyComponent.SceneNode.GetSkeletonInstance().Scale = 1;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");
            }
        }
    }
}