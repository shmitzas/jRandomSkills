using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
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

            Instance.AddCommand($"css_map", "", Command_ChangeMap);
            Instance.AddCommand($"css_console", "", Command_CustomCommand);
            Instance.AddCommand($"css_start", "", Command_StartGame);
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

            if (command.ArgCount < 2)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                Utils.PrintToChat(player, Localization.GetTranslation("correct_form_setskill"), true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            var skillName = command.ArgCount > 3 ? $"{command.GetArg(2)} {command.GetArg(3)}" : command.GetArg(2);
            var skill = SkillData.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) || s.Skill.ToString().Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skill == null)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                Utils.PrintToChat(player, Localization.GetTranslation("skill_not_found_setskill"), true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            foreach (CCSPlayerController target in players)
            {
                var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == target.SteamID);
                if (skillPlayer != null)
                {

                    Instance.SkillAction(skillPlayer.Skill.ToString(), "DisableSkill", new object[] { target });
                    skillPlayer.Skill = skill.Skill;
                    Instance.SkillAction(skill.Skill.ToString(), "EnableSkill", new object[] { target });

                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                    Utils.PrintToChat(player, $"{Localization.GetTranslation("done_setskill")}: {ChatColors.LightRed}{skill.Name} {ChatColors.Lime}{Localization.GetTranslation("for_setskill")} {ChatColors.LightRed}{target.PlayerName}", false);
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                    Utils.PrintToChat(player, Localization.GetTranslation("error_setskill"), true);
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

        [RequiresPermissions("@css/root")]
        [CommandHelper(minArgs: 1, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public static void Command_ChangeMap(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            string map = command.GetArg(1);

            if (string.IsNullOrEmpty(map))
            {
                command.ReplyToCommand($" {ChatColors.Red}{Localization.GetTranslation("invalid_map")}");
                return;
            }

            Server.PrintToChatAll($" {ChatColors.Yellow}{Localization.GetTranslation("loading_map")} ({ChatColors.Green}{map}{ChatColors.Yellow})...");

            if (uint.TryParse(map, out _))
                Server.ExecuteCommand($"host_workshop_map {map}");
            else if (!Server.IsMapValid(map))
                command.ReplyToCommand($" {ChatColors.Red}{Localization.GetTranslation("invalid_map")}");
            else
                Server.ExecuteCommand($"changelevel {map}");
        }

        [RequiresPermissions("@css/root")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public static void Command_StartGame(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            int cheats = command.GetArg(1) == "sv" ? 1 : 0;

            Server.ExecuteCommand($"mp_freezetime {(cheats == 1 ? 0 : 5)}");
            Server.ExecuteCommand("mp_warmup_end");
            Server.ExecuteCommand("mp_restartgame 1");

            Instance.AddTimer(2.0f, () => {
                Server.PrintToChatAll($" {ChatColors.Green}{Localization.GetTranslation("game_start")}");
                Server.ExecuteCommand("mp_forcecamera 0");
                Server.ExecuteCommand($"mp_freezetime {(cheats == 1 ? 0 : 5)}");
                Server.ExecuteCommand("mp_overtime_enable 1");
                Server.ExecuteCommand($"sv_cheats {cheats}");
            });
        }

        [RequiresPermissions("@css/root")]
        [CommandHelper(minArgs: 1, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public static void Command_CustomCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            string param = command.GetArg(1);
            Server.ExecuteCommand(param);
        }
    }
}