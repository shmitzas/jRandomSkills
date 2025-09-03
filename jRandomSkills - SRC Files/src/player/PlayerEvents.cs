using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using System.Text.RegularExpressions;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public static class Event
    {
        private static jSkill_SkillInfo ctSkill = new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false);
        private static jSkill_SkillInfo tSkill = new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false);
        private static jSkill_SkillInfo allSkill = new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false);
        private static List<jSkill_SkillInfo> debugSkills = new List<jSkill_SkillInfo>(SkillData.Skills);

        public static void Load()
        {
            Instance.RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
            {
                var player = @event.Userid;

                if (player == null || !player.IsValid) return HookResult.Continue;

                Instance.skillPlayer.Add(new jSkill_PlayerInfo
                {
                    SteamID = player.SteamID,
                    PlayerName = player.PlayerName,
                    Skill = Skills.None,
                    SpecialSkill = Skills.None,
                    IsDrawing = false,
                    SkillChance = 1,
                });

                const string defaultWelcomeMsg = "Welcome {PLAYER}, to the {SERVER_NAME} server!\n"
                    + "The current version of the jRandomSkills: {VERSION} ({SKILLS_COUNT} skills).\n\n"
                    + "Original plugin created by:\n{AUTHOR1}\n"
                    + "Modified and improved by:\n{AUTHOR2}";
                string langWelcomeMsg = Localization.GetTranslation("welcome_message", "welcome");
                string welcomeMsg = Regex.IsMatch(langWelcomeMsg, @"\{AUTHOR1\}", RegexOptions.IgnoreCase) && Regex.IsMatch(langWelcomeMsg, @"\{AUTHOR2\}", RegexOptions.IgnoreCase) ? langWelcomeMsg : defaultWelcomeMsg;

                foreach (string line in welcomeMsg.Split("\n"))
                    player.PrintToChat($" {ChatColors.Green}" + line.Replace("{PLAYER}", $" {ChatColors.Red}{player.PlayerName}{ChatColors.Green}", StringComparison.OrdinalIgnoreCase)
                                            .Replace("{SERVER_NAME}", $" {ChatColors.Red}{ConVar.Find("hostname").StringValue ?? "Default Server"}{ChatColors.Green}", StringComparison.OrdinalIgnoreCase)
                                            .Replace("{VERSION}", $" {ChatColors.Red}v{Instance.ModuleVersion}{ChatColors.Green}", StringComparison.OrdinalIgnoreCase)
                                            .Replace("{SKILLS_COUNT}", $" {ChatColors.Red}{SkillData.Skills.Count - 1}{ChatColors.Green}", StringComparison.OrdinalIgnoreCase)
                                            .Replace("{AUTHOR1}", $" {ChatColors.Red}Jakub Bartosik (D3X){ChatColors.Green} ({ChatColors.Red}https://github.com/jakubbartosik/dRandomSkills{ChatColors.Green})", StringComparison.OrdinalIgnoreCase)
                                            .Replace("{AUTHOR2}", $" {ChatColors.Red}Juzlus{ChatColors.Green} ({ChatColors.Red}https://github.com/Juzlus/jRandomSkills{ChatColors.Green})", StringComparison.OrdinalIgnoreCase));

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
            {
                var player = @event.Userid;

                if (player == null || !player.IsValid) return HookResult.Continue;

                var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (skillPlayer != null)
                {
                    Instance.skillPlayer.Remove(skillPlayer);
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (skillPlayer != null)
                        skillPlayer.IsDrawing = true;
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    Instance.AddTimer(0.5f, () =>
                    {
                        var _players = Utilities.GetPlayers().Where(p => p.IsValid).OrderBy(p => p.Team);

                        string skillsText = "";
                        foreach (var _player in _players)
                        {
                            var _playerSkill = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == _player.SteamID);
                            if (_playerSkill != null)
                            {
                                var skillInfo = SkillData.Skills.FirstOrDefault(p => p.Skill == _playerSkill.Skill);
                                var specialSkillInfo = SkillData.Skills.FirstOrDefault(s => s.Skill == _playerSkill.SpecialSkill);
                                skillsText += $" {ChatColors.DarkRed}{_player.PlayerName}{ChatColors.Lime}: {(_playerSkill.SpecialSkill == Skills.None ? skillInfo.Name : $"{specialSkillInfo.Name} -> {skillInfo.Name}")}\n";
                            }
                        }

                        if (Config.config.Settings.SummaryAfterTheRound && !string.IsNullOrEmpty(skillsText))
                        {
                            player.PrintToChat(" ");
                            player.PrintToChat($" {ChatColors.Lime}{Localization.GetTranslation("summary_start")}");
                            foreach (string text in skillsText.Split("\n"))
                                if (!string.IsNullOrEmpty(text))
                                    player.PrintToChat(text);
                            player.PrintToChat($" {ChatColors.Lime}{Localization.GetTranslation("summary_end")}");
                            player.PrintToChat(" \n");
                        }
                    });
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Config.DefaultSkillInfo[] terroristSkills = Config.config.SkillsInfo.Where(s => s.OnlyTeam == (int)CsTeam.Terrorist).ToArray();
                Config.DefaultSkillInfo[] counterterroristSkills = Config.config.SkillsInfo.Where(s => s.OnlyTeam == (int)CsTeam.CounterTerrorist).ToArray();
                Config.DefaultSkillInfo[] allTeamsSkills = Config.config.SkillsInfo.Where(s => s.OnlyTeam == 0).ToArray();

                if (Config.config.Settings.GameMode == (int)Config.GameModes.TeamSkills)
                {
                    List<jSkill_SkillInfo> tSkills = new List<jSkill_SkillInfo>(SkillData.Skills);
                    tSkills.RemoveAll(s => s.Skill == tSkill.Skill || s.Skill == Skills.None || counterterroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));
                    tSkill = tSkills.Count == 0 ? new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false) : tSkills[Instance.Random.Next(tSkills.Count)];

                    List<jSkill_SkillInfo> ctSkills = new List<jSkill_SkillInfo>(SkillData.Skills);
                    ctSkills.RemoveAll(s => s.Skill == ctSkill.Skill || s.Skill == Skills.None || terroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));
                    ctSkill = ctSkills.Count == 0 ? new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false) : ctSkills[Instance.Random.Next(ctSkills.Count)];
                }
                else if (Config.config.Settings.GameMode == (int)Config.GameModes.SameSkills)
                {
                    List<jSkill_SkillInfo> allSkills = new List<jSkill_SkillInfo>(SkillData.Skills);
                    allSkills.RemoveAll(s => s.Skill == allSkill.Skill || s.Skill == Skills.None || !allTeamsSkills.Any(s2 => s2.Name == s.Skill.ToString()));
                    allSkill = allSkills.Count == 0 ? new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false) : allSkills[Instance.Random.Next(allSkills.Count)];
                }
                else if (Config.config.Settings.GameMode == (int)Config.GameModes.Debug && debugSkills.Count == 0)
                    debugSkills = new List<jSkill_SkillInfo>(SkillData.Skills);

                foreach (var player in Utilities.GetPlayers())
                {
                    var playerTeam = player.Team;
                    var teammates = Utilities.GetPlayers().Where(p => p.Team == playerTeam && p != player);
                    string teammateSkills = "";

                    var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                    if (skillPlayer != null)
                    {
                        skillPlayer.IsDrawing = false;
                        jSkill_SkillInfo randomSkill = new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false);

                        if (Instance?.GameRules?.WarmupPeriod == false)
                        {
                            if (Config.config.Settings.GameMode == (int)Config.GameModes.Normal)
                            {
                                List<jSkill_SkillInfo> skillList = new List<jSkill_SkillInfo>(SkillData.Skills);
                                skillList.RemoveAll(s => s?.Skill == skillPlayer?.Skill || s?.Skill == skillPlayer?.SpecialSkill || s?.Skill == Skills.None);

                                if (Utilities.GetPlayers().FindAll(p => p.Team == player.Team && p.IsValid && !p.IsBot).Count == 1)
                                {
                                    Config.DefaultSkillInfo[] skillsNeedsTeammates = Config.config.SkillsInfo.Where(s => s.NeedsTeammates).ToArray();
                                    skillList.RemoveAll(s => skillsNeedsTeammates.Any(s2 => s2.Name == s.Skill.ToString()));
                                }

                                if (player.Team == CsTeam.Terrorist)
                                    skillList.RemoveAll(s => counterterroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));
                                else
                                    skillList.RemoveAll(s => terroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));

                                randomSkill = skillList.Count == 0 ? new jSkill_SkillInfo(Skills.None, Config.GetValue<string>(Skills.None, "color"), false) : skillList[Instance.Random.Next(skillList.Count)];
                            }
                            else if (Config.config.Settings.GameMode == (int)Config.GameModes.TeamSkills)
                                randomSkill = player.Team == CsTeam.Terrorist ? tSkill : ctSkill;
                            else if (Config.config.Settings.GameMode == (int)Config.GameModes.SameSkills)
                                randomSkill = allSkill;
                            else if (Config.config.Settings.GameMode == (int)Config.GameModes.Debug)
                            {
                                if (debugSkills.Count == 0)
                                    debugSkills = new List<jSkill_SkillInfo>(SkillData.Skills);
                                randomSkill = debugSkills[0];
                                debugSkills.RemoveAt(0);
                                player.PrintToChat($"{SkillData.Skills.Count - debugSkills.Count}/{SkillData.Skills.Count}");
                            }
                        }

                        skillPlayer.Skill = randomSkill.Skill;
                        skillPlayer.SpecialSkill = Skills.None;
                        Debug.WriteToDebug($"Player {skillPlayer.PlayerName} has got the skill \"{randomSkill.Name}\".");

                        if (randomSkill.Display)
                            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{randomSkill.Name}{ChatColors.Lime}: {randomSkill.Description}", false);

                        if (Config.config.Settings.TeamMateSkillInfo)
                        {
                            Instance.AddTimer(0.5f, () =>
                            {
                                foreach (var teammate in teammates)
                                {
                                    var teammateSkill = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == teammate.SteamID)?.Skill;
                                    if (teammateSkill != null)
                                    {
                                        var skillInfo = SkillData.Skills.FirstOrDefault(p => p.Skill == teammateSkill);
                                        teammateSkills += $" {ChatColors.DarkRed}{teammate.PlayerName}{ChatColors.Lime}: {skillInfo.Name}\n";
                                    }
                                }

                                if (!string.IsNullOrEmpty(teammateSkills))
                                {
                                    SkillUtils.PrintToChat(player, $" {ChatColors.Lime}{Localization.GetTranslation("teammate_skills")}:", false);
                                    foreach (string text in teammateSkills.Split("\n"))
                                        if (!string.IsNullOrEmpty(text))
                                            player.PrintToChat(text);
                                }
                            });
                        }
                    }
                }

                return HookResult.Continue;
            });


            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;

                if (victim == null || attacker == null || victim == attacker) return HookResult.Continue;

                if(Config.config.Settings.KillerSkillInfo)
                {
                    var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                    if (attackerInfo != null)
                    {
                        var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == attackerInfo.Skill);
                        var specialSkillData = SkillData.Skills.FirstOrDefault(s => s.Skill == attackerInfo.SpecialSkill);
                        string skillDesc = skillData?.Description;

                        SkillUtils.PrintToChat(victim, $"{Localization.GetTranslation("enemy_skill")} {ChatColors.DarkRed}{attacker.PlayerName}{ChatColors.Lime}:", false);
                        SkillUtils.PrintToChat(victim, $"{ChatColors.DarkRed}{(attackerInfo.SpecialSkill == Skills.None ? skillData.Name : $"{specialSkillData.Name} -> {skillData.Name}")}{ChatColors.Lime} - {skillDesc}", false);
                    }
                }

                return HookResult.Continue;
            });
        }
    }
}