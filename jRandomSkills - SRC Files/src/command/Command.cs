using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public static class Command
    {
        private static bool gamePaused = false;

        public static void Load()
        {
            var config = Config.config?.Settings;
            if (config == null || config == null) return;

            var commands = new Dictionary<IEnumerable<string>, (string description, CommandInfo.CommandCallback handler)>
            {
                { SplitCommands(config.SetSkillCommands), ("Delete record", Command_SetSkill) },
                { SplitCommands(config.SkillsListCommands), ("Delete all records", Command_SkillsListMenu) },
                { SplitCommands(config.UseSkillCommands), ("Use/Type skill", Command_UseTypeSkill) },
                { SplitCommands(config.ChangeMapCommands), ("Change map", Command_ChangeMap) },
                { SplitCommands(config.ConsoleCommands), ("Console command", Command_CustomCommand) },
                { SplitCommands(config.StartGameCommands), ("Start game", Command_StartGame) },
                { SplitCommands(config.SwapCommands), ("Swap team", Command_Swap) },
                { SplitCommands(config.ShuffleCommands), ("Shuffle team", Command_Shuffle) },
                { SplitCommands(config.PauseCommands), ("Pause game", Command_Pause) },
                { SplitCommands(config.HealCommands), ("Heal", Command_Heal) },
                { SplitCommands(config.SetScoreCommands), ("Set teams score", Command_SetScore) },
            };

            foreach (var commandPair in commands)
                foreach (var command in commandPair.Key)
                    Instance.AddCommand($"css_{command}", commandPair.Value.description, commandPair.Value.handler);
        }

        private static IEnumerable<string> SplitCommands(string commands)
        {
            return commands.Split(',').Select(c => c.Trim());
        }

        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_ONLY)]
        private static void Command_UseTypeSkill(CCSPlayerController? player, CommandInfo _)
        {
            if (player == null) return;
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;
            if (!player.IsValid || !player.PawnIsAlive) return;

            string[] commands = _.ArgString.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteToDebug($"Player {player.PlayerName} used the skill: {playerInfo.Skill}");
            if (commands == null || commands.Length == 0)
                Instance.SkillAction(playerInfo!.Skill.ToString(), "UseSkill", new object[] { player });
            else
                Instance.SkillAction(playerInfo!.Skill.ToString(), "TypeSkill", new object[] { player, commands });
        }

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_ONLY)]
        private static void Command_SetSkill(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            (List<CCSPlayerController> players, string targetname) = FindTarget.Find(command, 2, false, true);

            if (command.ArgCount < 2)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                SkillUtils.PrintToChat(player, Localization.GetTranslation("correct_form_setskill"), true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            if (players.Count == 0)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                SkillUtils.PrintToChat(player, Localization.GetTranslation("player_not_found_setskill"), true);
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                return;
            }

            var skillName = command.ArgCount > 3 ? $"{command.GetArg(2)} {command.GetArg(3)}" : command.GetArg(2);
            var skill = SkillData.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) || s.Skill.ToString().Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skill == null)
            {
                player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                SkillUtils.PrintToChat(player, Localization.GetTranslation("skill_not_found_setskill"), true);
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
                    SkillUtils.PrintToChat(player, $"{Localization.GetTranslation("done_setskill")}: {ChatColors.LightRed}{skill.Name} {ChatColors.Lime}{Localization.GetTranslation("for_setskill")} {ChatColors.LightRed}{target.PlayerName}", false);
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                    SkillUtils.PrintToChat(player, Localization.GetTranslation("error_setskill"), true);
                    player.PrintToChat($" {ChatColors.Green}―――――――――――{ChatColors.DarkRed}◥◣◆◢◤{ChatColors.Green}―――――――――――");
                }
            }

        }

        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_ONLY)]
        private static void Command_SkillsListMenu(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            Menu.DisplaySkillsList(player);
        }

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 1, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_ChangeMap(CCSPlayerController? player, CommandInfo command)
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

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_StartGame(CCSPlayerController? player, CommandInfo command)
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

        [RequiresPermissions("@jRandmosSkills/root")]
        [CommandHelper(minArgs: 1, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_CustomCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            string param = command.ArgString;
            Server.ExecuteCommand(param);
        }

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_Swap(CCSPlayerController? _player, CommandInfo command)
        {
            foreach (var player in Utilities.GetPlayers())
                if (Instance.IsPlayerValid(player) && new CsTeam[] { CsTeam.CounterTerrorist, CsTeam.Terrorist }.Contains(player.Team))
                    player.SwitchTeam(player.Team == CsTeam.Terrorist ? CsTeam.CounterTerrorist : CsTeam.Terrorist);
            Server.ExecuteCommand($"mp_restartgame 1");
        }

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_Shuffle(CCSPlayerController? _player, CommandInfo command)
        {
            var players = Utilities.GetPlayers().FindAll(p => (Instance.IsPlayerValid(p) && new CsTeam[] { CsTeam.CounterTerrorist, CsTeam.Terrorist }.Contains(p.Team)));
            double CTlimit = Instance.Random.Next(0, 2) == 0 ? Math.Floor(players.Count / 2.0) : Math.Ceiling(players.Count / 2.0);

            foreach (var player in players.OrderBy(_ => Instance.Random.Next()).ToList())
            {
                player?.SwitchTeam(CTlimit > 0 ? CsTeam.CounterTerrorist : CsTeam.Terrorist);
                CTlimit--;
            }
            Server.ExecuteCommand($"mp_restartgame 1");
        }

        [RequiresPermissions("@jRandmosSkills/admin")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_Pause(CCSPlayerController? player, CommandInfo command)
        {
            Server.PrintToChatAll($" {(gamePaused ? ChatColors.Green : ChatColors.Red)}{Localization.GetTranslation(gamePaused ? "unpause" : "pause")}");
            Server.ExecuteCommand( gamePaused ? "mp_unpause_match" : "mp_pause_match");
            gamePaused = !gamePaused;
        }

        [RequiresPermissions("@jRandmosSkills/root")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_Heal(CCSPlayerController? player, CommandInfo command)
        {
            SkillUtils.AddHealth(player.PlayerPawn.Value, 100);
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("healed")}");
        }

        [RequiresPermissions("@jRandmosSkills/root")]
        [CommandHelper(minArgs: 2, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void Command_SetScore(CCSPlayerController? player, CommandInfo command)
        {
            if (!int.TryParse(command.GetArg(1), out int ctScore) || !int.TryParse(command.GetArg(2), out int tScore))
            {
                SkillUtils.PrintToChat(player, Localization.GetTranslation("correct_form_setscore"), true);
                return;
            }

            SkillUtils.SetTeamScores((short)ctScore, (short)tScore, RoundEndReason.RoundDraw);
        }
    }
}