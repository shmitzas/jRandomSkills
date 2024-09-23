using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;
using CounterStrikeSharp.API;
using System.Text.RegularExpressions;

namespace dRandomSkills
{
    public static class Menu
    {
        public static void DisplaySkillsList(CCSPlayerController player)
        {
            CenterHtmlMenu menu = new($"[ ★ Lista Super Mocy ★ ]");
            
            for (int i = 0; i < SkillData.Skills.Count; i++)
            {
                var skill = SkillData.Skills[i];
                string skillName = $"{skill.Name}";
                menu.AddMenuOption($" ★ {skillName}", (player, option) =>
                {
                    string selectedSkillName = option.Text;
                    string pattern = "\\[/?color\\b[^\\]]*\\]";
                    string cleanSkillName = Regex.Replace(selectedSkillName, pattern, "");
                    string skillName = cleanSkillName.Replace($" ★ ", "");
                    int intValue = Array.IndexOf(SkillData.Skills.Select(r => r.Name).ToArray(), skillName);
                    string skillDesc = SkillData.Skills[intValue].Description;
                    Utils.PrintToChat(player, $"{ChatColors.DarkRed}{skillName}{ChatColors.Lime}: {skillDesc}", false);
                    MenuManager.CloseActiveMenu(player);
                });
            }

            MenuManager.OpenCenterHtmlMenu(Instance, player, menu);
        }
    }
}