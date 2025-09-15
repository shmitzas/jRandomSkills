using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class GodMode : ISkill
    {
        private const Skills skillName = Skills.GodMode;
        private static readonly float timerCooldown = Config.GetValue<float>(skillName, "cooldown");
        private static readonly float duration = Config.GetValue<float>(skillName, "duration");
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            SkillPlayerInfo.Clear();
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo[player.SteamID] = new PlayerSkillInfo
            {
                SteamID = player.SteamID,
                CanUse = true,
                Cooldown = DateTime.MinValue,
            };
        }

        public static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
                        UpdateHUD(player, skillInfo);
            }
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo.Remove(player.SteamID);
        }

        private static void UpdateHUD(CCSPlayerController player, PlayerSkillInfo skillInfo)
        {
            float cooldown = 0;
            if (skillInfo != null)
            {
                float time = (int)(skillInfo.Cooldown.AddSeconds(timerCooldown) - DateTime.Now).TotalSeconds;
                cooldown = Math.Max(time, 0);

                if (cooldown == 0 && skillInfo?.CanUse == false)
                    skillInfo.CanUse = true;
            }

            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{player.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{player.GetSkillName(skillData.Skill)}</font> <br>";
            string remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{player.GetTranslation("hud_info", $"<font color='#FF0000'>{cooldown}</font>")}</font> <br>" : "";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        public static void UseSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;

            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                if (!player.IsValid || !player.PawnIsAlive || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return;
                if (skillInfo.CanUse)
                {
                    skillInfo.CanUse = false;
                    skillInfo.Cooldown = DateTime.Now;

                    player.PrintToChat($" {ChatColors.Green} {player.GetTranslation("godmode_on")}");
                    player.PlayerPawn.Value.TakesDamage = false;

                    Instance.AddTimer(duration, () => {
                        if (player.IsValid && player.PawnIsAlive)
                        {
                            player.PlayerPawn.Value.TakesDamage = true;
                            player.PrintToChat($" {ChatColors.Red} {player.GetTranslation("godmode_off")}");
                        }
                    });
                }
            }
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#e0d83a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 30f, float duration = 2f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float Cooldown { get; set; } = cooldown;
            public float Duration { get; set; } = duration;
        }
    }
}