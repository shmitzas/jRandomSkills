using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Regeneration : ISkill
    {
        private const Skills skillName = Skills.Regeneration;
        private static float cooldown = Config.GetValue<float>(skillName, "cooldown");
        private static int healthToAdd = Config.GetValue<int>(skillName, "healthToAdd");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            Instance.RegisterListener<Listeners.OnTick>(OnTick);
        }

        private static void OnTick()
        {
            if (Server.TickCount % (int)(64 * cooldown) != 0) return;
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) continue;

                var pawn = player.PlayerPawn.Value;
                if (pawn == null || !pawn.IsValid) continue;
                SkillUtils.AddHealth(pawn, healthToAdd);
            }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int HealthToAdd { get; set; }
            public float Cooldown { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#ff462e", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int healthToAdd = 1, float cooldown = .25f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                HealthToAdd = healthToAdd;
                Cooldown = cooldown;
            }
        }
    }
}