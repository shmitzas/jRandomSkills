using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class KillerFlash : ISkill
    {
        private const Skills skillName = Skills.KillerFlash;
        private static float flashDuration = Config.GetValue<float>(skillName, "flashDuration");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

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

            Instance.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;
                var attacker = @event.Attacker;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == skillName && playerInfo?.Skill != Skills.AntyFlash && player?.PlayerPawn.Value.FlashDuration >= flashDuration)
                   player?.PlayerPawn?.Value?.CommitSuicide(false, true);

                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, CsItem.FlashbangGrenade);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float FlashDuration { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#57bcff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float flashDuration = 1f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                FlashDuration = flashDuration;
            }
        }
    }
}