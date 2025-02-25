using jRandomSkills.src.player;

namespace jRandomSkills
{
    public class None : ISkill
    {
        private static Skills skillName = Skills.None;

        public static void LoadSkill()
        {
            Utils.RegisterSkill(skillName, "#FFFFFF", false);
        }
    }
}