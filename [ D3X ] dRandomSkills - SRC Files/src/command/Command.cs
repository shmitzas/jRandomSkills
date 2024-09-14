using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Command
    {
        public static void Load()
        {
            var config = Config.config?.Settings;
            if (config == null || config == null) return;

            var commands = new Dictionary<IEnumerable<string>, (string description, CommandInfo.CommandCallback handler)>
            {
                { SplitCommands(config.Set_Skill), ("Delete record", Command_SetSkill) },
                { SplitCommands(config.SkillsList_Menu), ("Delete all records", Command_SkillsListMenu) }
            };

            foreach (var commandPair in commands)
            {
                foreach (var command in commandPair.Key)
                {
                    Instance.AddCommand($"css_{command}", commandPair.Value.description, commandPair.Value.handler);
                }
            }
        }

        private static IEnumerable<string> SplitCommands(string commands)
        {
            return commands.Split(',').Select(c => c.Trim());
        }

        public static void AddCommands(IEnumerable<string> commands, string description, CommandInfo.CommandCallback commandAction)
        {
            foreach (var command in commands)
            {
                Instance.AddCommand($"css_{command}", description, commandAction);
            }
        }

        [RequiresPermissions("@css/root")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_ONLY)]
        public static void Command_SetSkill(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            (List<CCSPlayerController> players, string targetname) = FindTarget.Find(command, 2, false, true);

            if (command.ArgCount < 1)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                Utils.PrintToChat(player, $"Poprawne użycie: {ChatColors.DarkRed}!setskill <nick> <supermoc>", true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            if (command.ArgCount < 2)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                Utils.PrintToChat(player, $"Poprawne użycie: {ChatColors.DarkRed}!setskill <nick> <supermoc>", true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            var skillName = command.GetArg(2);
            var skill = SkillData.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skill == null)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                Utils.PrintToChat(player, $"Nie znaleziono takiej {ChatColors.DarkRed}supermocy", true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            foreach (CCSPlayerController target in players)
            {
                var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == target.SteamID);
                if (skillPlayer != null)
                {
                    skillPlayer.Skill = skill.Name;
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                    Utils.PrintToChat(player, $"Ustawiono Supermoc: {ChatColors.LightRed}{skill.Name} {ChatColors.Lime}dla {ChatColors.LightRed}{target.PlayerName}", false);
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                    Utils.PrintToChat(player, $"Nie udało się ustawić {ChatColors.DarkRed}supermocy", true);
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                }
            }
            
        }

        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_ONLY)]
        public static void Command_SkillsListMenu(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            Menu.DisplaySkillsList(player);
        }
    }
}