using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class JumpingJack : ISkill
    {
        private const Skills skillName = Skills.JumpingJack;
        private static readonly int addHealth = Config.GetValue<int>(skillName, "healthToAdd");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void PlayerJump(EventPlayerJump @event)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;
            SkillUtils.AddHealth(player.PlayerPawn.Value, addHealth);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#a86eff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int healthToAdd = 3) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int HealthToAdd { get; set; } = healthToAdd;
        }
    }
}