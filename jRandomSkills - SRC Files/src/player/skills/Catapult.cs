using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Catapult : ISkill
    {
        private static Skills skillName = Skills.Catapult;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#FF4500", false);

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

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == skillName && victim.PawnIsAlive)
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

        public static void EnableSkill(CCSPlayerController player)
        {
            var skillConfig = Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString());
            if (skillConfig == null) return;

            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            float newChance = (float)Instance.Random.NextDouble() * (skillConfig.ChanceTo - skillConfig.ChanceFrom) + skillConfig.ChanceFrom;
            playerInfo.SkillChance = newChance;
            newChance = (float)Math.Round(newChance, 2) * 100;
            newChance = (float)Math.Round(newChance);
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("catapult")}{ChatColors.Lime}: " + Localization.GetTranslation("catapult_desc2", newChance), false);
        }
    }
}