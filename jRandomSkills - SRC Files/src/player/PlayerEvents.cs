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
        public static void Load()
        {
            Instance.RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
            {
                var player = @event.Userid;

                if (player == null || !player.IsValid) return HookResult.Continue;

                Instance.skillPlayer.Add(new dSkill_PlayerInfo
                {
                    SteamID = player.SteamID,
                    PlayerName = player.PlayerName,
                    Skill = src.player.Skills.None,
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
                            var _playerSkill = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == _player.SteamID)?.Skill;
                            if (_playerSkill != null)
                            {
                                var skillInfo = SkillData.Skills.FirstOrDefault(p => p.Skill == _playerSkill);
                                skillsText += $" {ChatColors.DarkRed}{_player.PlayerName}{ChatColors.Lime}: {skillInfo.Name}\n";
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
                foreach (var player in Utilities.GetPlayers())
                {
                    var playerTeam = player.Team;
                    var teammates = Utilities.GetPlayers().Where(p => p.Team == playerTeam && p != player);
                    string teammateSkills = "";

                    var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                    if (skillPlayer != null)
                    {
                        skillPlayer.IsDrawing = false;

                        List<dSkill_SkillInfo> skillList = new List<dSkill_SkillInfo>(SkillData.Skills);
                        skillList.RemoveAll(s => s?.Skill == skillPlayer?.Skill || s?.Skill == src.player.Skills.None);

                        if (Utilities.GetPlayers().FindAll(p => p.Team == player.Team && p.IsValid && !p.IsBot).Count != 1) {
                            Config.SkillInfo[] skillsOnly1v1 = Config.config.SkillsInfo.Where(s => s.Only1v1).ToArray();
                            skillList.RemoveAll(s => skillsOnly1v1.Any(s2 => s2.Name == s.Skill.ToString()));
                        }

                        Config.SkillInfo[] terroristSkills = Config.config.SkillsInfo.Where(s => s.Team == 2).ToArray();
                        Config.SkillInfo[] counterterroristSkills = Config.config.SkillsInfo.Where(s => s.Team == 3).ToArray();
                        if (player.Team == CsTeam.Terrorist)
                            skillList.RemoveAll(s => counterterroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));
                        else
                            skillList.RemoveAll(s => terroristSkills.Any(s2 => s2.Name == s.Skill.ToString()));

                        var randomSkill = skillList.Count == 0 ? new dSkill_SkillInfo(Skills.None, "#ffffff", false) : skillList[Instance.Random.Next(skillList.Count)];
                        skillPlayer.Skill = randomSkill.Skill;

                        if (randomSkill.Display)
                            Utils.PrintToChat(player, $"{ChatColors.DarkRed}{randomSkill.Name}{ChatColors.Lime}: {randomSkill.Description}", false);

                        if(Config.config.Settings.TeamMateSkillInfo)
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
                                    Utils.PrintToChat(player, $" {ChatColors.Lime}{Localization.GetTranslation("teammate_skills")}:", false);
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
                        string skillDesc = skillData?.Description;

                        Utils.PrintToChat(victim, $"{Localization.GetTranslation("enemy_skill")} {ChatColors.DarkRed}{attacker.PlayerName}{ChatColors.Lime}:", false);
                        Utils.PrintToChat(victim, $"{ChatColors.DarkRed}{skillData.Name}{ChatColors.Lime} - {skillDesc}", false);
                    }
                }

                return HookResult.Continue;
            });
        }
    }
}