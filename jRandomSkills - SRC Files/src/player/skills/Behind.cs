using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Behind : ISkill
    {
        private static Skills skillName = Skills.Behind;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#00FF00", false);

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
                var attacker = @event!.Attacker;
                var victim = @event!.Userid;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim) || attacker == victim) 
                    return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == skillName && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= playerInfo.SkillChance)
                        RotateEnemy(victim);
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
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("behind")}{ChatColors.Lime}: " + Localization.GetTranslation("behind_desc2", newChance), false);
        }

        private static void RotateEnemy(CCSPlayerController player)
        {
            if (player.PlayerPawn.Value.LifeState != (int)LifeState_t.LIFE_ALIVE)
                return;

            var currentPosition = player.PlayerPawn.Value.AbsOrigin;
            var currentAngles = player.PlayerPawn.Value.EyeAngles;

            QAngle newAngles = new QAngle(
                currentAngles.X,
                currentAngles.Y + 180,
                currentAngles.Z
            );

            player.PlayerPawn.Value.Teleport(currentPosition, newAngles, new Vector(0, 0, 0));
        }
    }
}