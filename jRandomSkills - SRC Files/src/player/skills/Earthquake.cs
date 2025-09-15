using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Earthquake : ISkill
    {
        private const Skills skillName = Skills.Earthquake;
        private static readonly float timerCooldown = Config.GetValue<float>(skillName, "cooldown");
        private static readonly float amplitude = Config.GetValue<float>(skillName, "amplitude");
        private static readonly float frequency = Config.GetValue<float>(skillName, "frequency");
        private static readonly float duration = Config.GetValue<float>(skillName, "duration");
        private static readonly float radius = Config.GetValue<float>(skillName, "radius");
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

        public static void DisableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo.Remove(player.SteamID);
        }

        public static void PlayerDeath(EventPlayerDeath @event)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill == skillName)
                SkillPlayerInfo.Remove(player.SteamID);
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
                if (!player.IsValid || !player.PawnIsAlive) return;
                if (skillInfo.CanUse)
                {
                    skillInfo.CanUse = false;
                    skillInfo.Cooldown = DateTime.Now;
                    MakeShake(player);
                }
            }
        }

        private static void MakeShake(CCSPlayerController player)
        {
            foreach (var enemy in Utilities.GetPlayers().Where(p => p != null && p.IsValid && p.PawnIsAlive))
                CreateShake(player);
        }

        private static void CreateShake(CCSPlayerController player)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid || pawn.AbsOrigin == null) return;

            var shake = Utilities.CreateEntityByName<CEnvShake>("env_shake");
            if (shake == null) return;

            shake.DispatchSpawn();
            if (!shake.IsValid) return;

            shake.Amplitude = amplitude;
            shake.Frequency = frequency;
            shake.Duration = duration;
            shake.Radius = radius;

            shake.Teleport(new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.ViewOffset.Z));
            shake.AcceptInput("SetParent", pawn, pawn, "!activator");
            shake.AcceptInput("StartShake");
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
        }

        public class SkillConfig(Skills skill = skillName, bool active = false, string color = "#42f59b", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 16f, float amplitude = 15f, float frequency = 500f, float duration = 8f, float radius = 50f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float Cooldown { get; set; } = cooldown;
            public float Amplitude { get; set; } = amplitude;
            public float Frequency { get; set; } = frequency;
            public float Duration { get; set; } = duration;
            public float Radius { get; set; } = radius;
        }
    }
}