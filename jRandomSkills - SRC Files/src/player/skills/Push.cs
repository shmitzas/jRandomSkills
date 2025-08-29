using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Push : ISkill
    {
        private const Skills skillName = Skills.Push;
        private static float jumpVelocity = Config.GetValue<float>(skillName, "jumpVelocity");
        private static float pushVelocity = Config.GetValue<float>(skillName, "pushVelocity");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);

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
                        PushEnemy(victim, attacker.PlayerPawn.Value.EyeAngles);
                }
                
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            float newChance = (float)Instance.Random.NextDouble() * (Config.GetValue<float>(skillName, "ChanceTo") - Config.GetValue<float>(skillName, "ChanceFrom")) + Config.GetValue<float>(skillName, "ChanceFrom");
            playerInfo.SkillChance = newChance;
            newChance = (float)Math.Round(newChance, 2) * 100;
            newChance = (float)Math.Round(newChance);
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("push")}{ChatColors.Lime}: " + Localization.GetTranslation("push_desc2", newChance), false);
        }

        private static void PushEnemy(CCSPlayerController player, QAngle attackerAngle)
        {
            if (player.PlayerPawn.Value.LifeState != (int)LifeState_t.LIFE_ALIVE)
                return;

            var currentPosition = player.PlayerPawn.Value.AbsOrigin;
            var currentAngles = player.PlayerPawn.Value.EyeAngles;

            Vector newVelocity = SkillUtils.GetForwardVector(attackerAngle) * pushVelocity;
            newVelocity.Z = player.PlayerPawn.Value.AbsVelocity.Z + jumpVelocity;

            player.PlayerPawn.Value.Teleport(currentPosition, currentAngles, newVelocity);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float ChanceFrom { get; set; }
            public float ChanceTo { get; set; }
            public float JumpVelocity { get; set; }
            public float PushVelocity { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#1e9ab0", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float chanceFrom = 1f, float chanceTo = 1f, float jumpVelocity = 300f, float pushVelocity = 400f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                ChanceFrom = chanceFrom;
                ChanceTo = chanceTo;
                JumpVelocity = jumpVelocity;
                PushVelocity = pushVelocity;
            }
        }
    }
}