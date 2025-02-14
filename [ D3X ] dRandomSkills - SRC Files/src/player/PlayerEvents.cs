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
                    SkillChance = 1,
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

                        var randomSkill = SkillData.Skills[Instance.Random.Next(SkillData.Skills.Count)];
                        skillPlayer.Skill = randomSkill.Name;

                        if (randomSkill.Display)
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
                                    Utils.PrintToChat(player, $" {ChatColors.Lime}Supermoce twoich sojuszników:", false);
                                    Utils.PrintToChat(player, teammateSkills.TrimEnd('\n'), false);
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
                        var skillData = SkillData.Skills.FirstOrDefault(s => s.Name == attackerInfo.Skill);
                        string skillDesc = skillData?.Description ?? "Brak opisu";

                        Utils.PrintToChat(victim, $"Supermoc przeciwnika {ChatColors.DarkRed}{attacker.PlayerName}{ChatColors.Lime}:", false);
                        Utils.PrintToChat(victim, $"{ChatColors.DarkRed}{attackerInfo.Skill}{ChatColors.Lime} - {skillDesc}", false);
                    }
                }

                return HookResult.Continue;
            });
        }
    }
}