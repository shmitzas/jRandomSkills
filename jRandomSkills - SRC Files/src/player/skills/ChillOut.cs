using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class ChillOut : ISkill
    {
        private const Skills skillName = Skills.ChillOut;
        private static readonly float bombArmedTime = Config.GetValue<float>(skillName, "bombArmedTime");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            
            Instance.RegisterEventHandler<EventBombBeginplant>((@event, info) =>
            {
                var player = @event.Userid;

                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var anyChillOut = Instance.SkillPlayer.FirstOrDefault(p => p.Skill == skillName);
                if (anyChillOut != null)
                {
                    var bombEntities = Utilities.FindAllEntitiesByDesignerName<CC4>("weapon_c4").ToList();

                    if (bombEntities.Count != 0)
                    {
                        var bomb = bombEntities.FirstOrDefault();
                        if (bomb != null)
                            bomb.ArmedTime = Server.CurrentTime + bombArmedTime;
                    }
                }

                return HookResult.Continue;
            });
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#343deb", CsTeam onlyTeam = CsTeam.CounterTerrorist, bool needsTeammates = false, float bombArmedTime = 10f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float BombArmedTime { get; set; } = bombArmedTime;
        }
    }
}