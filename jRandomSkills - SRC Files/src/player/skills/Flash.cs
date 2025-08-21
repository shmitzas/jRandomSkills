using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Flash : ISkill
    {
        private static Skills skillName = Skills.Flash;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#A31912", false);
            Instance.RegisterListener<OnTick>(UpdateSpeed);

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerPawn = player.PlayerPawn?.Value;

                        playerPawn.VelocityModifier = 1;
                        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });
                
                return HookResult.Continue;
            });

            Instance.HookUserMessage(208, um =>
            {
                var soundevent = um.ReadUInt("soundevent_hash");
                var userIndex = um.ReadUInt("source_entity_index");

                if (userIndex == 0) return HookResult.Continue;
                if (!Instance.footstepSoundEvents.Contains(soundevent)) return HookResult.Continue;
                
                var player = Utilities.GetPlayers().FirstOrDefault(p => p.Pawn.Value.Index == userIndex);
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                if (player.Buttons.HasFlag(PlayerButtons.Speed) || player.Buttons.HasFlag(PlayerButtons.Duck))
                {
                    um.Recipients.Clear();
                    return HookResult.Handled;
                }

                return HookResult.Continue;
            }, HookMode.Pre);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerPawn == null) return;

            var skillConfig = Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString());
            if (skillConfig == null) return;

            float newSpeed = (float)Instance.Random.NextDouble() * (skillConfig.ChanceTo - skillConfig.ChanceFrom) + skillConfig.ChanceFrom;
            newSpeed = (float)Math.Round(newSpeed, 2);
            playerInfo.SkillChance = newSpeed;

            playerPawn.VelocityModifier = newSpeed;
            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("flash")}{ChatColors.Lime}: " + Localization.GetTranslation("flash_desc2", newSpeed), false);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null) return;
            playerPawn.VelocityModifier = 1;
            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
        }

        private static void UpdateSpeed()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Instance.IsPlayerValid(player)) continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) continue;

                var playerPawn = player.PlayerPawn?.Value;
                if (playerPawn != null && playerPawn.VelocityModifier != 0)
                {
                    playerPawn.VelocityModifier = Math.Max((float)playerInfo?.SkillChance, 1);
                    Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
                }
            }
        }
    }
}