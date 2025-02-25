using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;

namespace jRandomSkills
{
    public static class Utils
    {
        public static void PrintToChat(CCSPlayerController player, string msg, bool isError)
        {
            string checkIcon = isError ? $"{ChatColors.DarkRed}✖{ChatColors.LightRed}" : $"{ChatColors.Green}✔{ChatColors.Lime}";
            player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} jRadnomSkills {ChatColors.Green}] {checkIcon} {msg}");
        }

        public static void RegisterSkill(Skills skill, string color, bool display = true)
        {
            if (!SkillData.Skills.Any(s => s.Skill == skill))
            {
                SkillData.Skills.Add(new dSkill_SkillInfo(skill, color, display));
            }
        }
    }
}