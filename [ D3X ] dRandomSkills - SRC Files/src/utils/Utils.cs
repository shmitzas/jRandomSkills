using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace dRandomSkills
{
    public static class Utils
    {
        public static void PrintToChat(CCSPlayerController player, string msg, bool isError)
        {
            string checkIcon = isError ? $"{ChatColors.DarkRed}✖{ChatColors.LightRed}" : $"{ChatColors.Green}✔{ChatColors.Lime}";
            player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} LOSOWE MOCE {ChatColors.Green}] {checkIcon} {msg}");
        }

        public static void RegisterSkill(string name, string description, string color)
        {
            if (!SkillData.Skills.Any(skill => skill.Name == name))
            {
                SkillData.Skills.Add(new dSkill_SkillInfo(name, description, color));
            }
        }
    }
}