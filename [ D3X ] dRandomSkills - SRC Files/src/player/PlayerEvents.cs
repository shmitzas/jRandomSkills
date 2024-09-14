using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
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
                    Skill = "",
                    IsDrawing = false,
                });

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
                    {
                        skillPlayer.IsDrawing = true;
                    }
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

                        var randomSkill = SkillData.Skills[Instance.Random.Next(SkillData.Skills.Length)];
                        skillPlayer.Skill = randomSkill.Name;

                        Utils.PrintToChat(player, $"{ChatColors.DarkRed}{randomSkill.Name}{ChatColors.Lime}: {randomSkill.Description}", false);

                        if(Config.config.Settings.TeamMateSkillInfo)
                        {
                            Instance.AddTimer(0.5f, () => 
                            {
                                foreach (var teammate in teammates)
                                {
                                    var teammateSkill = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == teammate.SteamID)?.Skill;
                                    if (!string.IsNullOrEmpty(teammateSkill))
                                    {
                                        teammateSkills += $" {ChatColors.DarkRed}{teammate.PlayerName}{ChatColors.Lime}: {teammateSkill}\n";
                                    }
                                }

                                if (!string.IsNullOrEmpty(teammateSkills))
                                {
                                    player.PrintToChat($" {ChatColors.Lime}Supermoce twoich sojuszników:");
                                    player.PrintToChat(teammateSkills.TrimEnd('\n'));
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

                if(Config.config.Settings.KillerSkillInfo)
                {
                    if (victim != null && victim?.IsValid == true && attacker != null && attacker?.IsValid == true && victim != attacker)
                    {
                        var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                        if (attackerInfo != null)
                        {
                            var skillData = SkillData.Skills.FirstOrDefault(s => s.Name == attackerInfo.Skill);
                            string skillDesc = skillData?.Description ?? "Brak opisu";

                            victim.PrintToChat($"{ChatColors.Lime}Supermoc przeciwnika który cię zabił:");
                            Utils.PrintToChat(victim, $"{ChatColors.DarkRed}{attacker.PlayerName}{ChatColors.Lime} posiada mocy: {ChatColors.DarkRed}{attackerInfo.Skill}{ChatColors.Lime} - {skillDesc}", false);
                        }
                    }
                }

                return HookResult.Continue;
            });
        }
    }
}