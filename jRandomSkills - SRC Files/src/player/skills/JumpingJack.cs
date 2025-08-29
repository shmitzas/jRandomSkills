using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class JumpingJack : ISkill
    {
        private const Skills skillName = Skills.JumpingJack;
        private static int addHealth = Config.GetValue<int>(skillName, "healthToAdd");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventPlayerJump>((@event, info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                SkillUtils.AddHealth(player.PlayerPawn.Value, addHealth);
                return HookResult.Continue;
            });
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int HealthToAdd { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#a86eff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int healthToAdd = 3) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                HealthToAdd = healthToAdd;
            }
        }
    }
}