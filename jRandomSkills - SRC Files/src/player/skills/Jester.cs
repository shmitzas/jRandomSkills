using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using System.Drawing;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Jester : ISkill
    {
        private const Skills skillName = Skills.Jester;
        private static bool jesterMode = false;
        private static bool jesterStarted = false;
        private static readonly float minTime = Config.GetValue<float>(skillName, "minTime");
        private static readonly float maxTime = Config.GetValue<float>(skillName, "maxTime");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            jesterStarted = false;
        }

        public static void PlayerHurt(EventPlayerHurt @event)
        {
            if (!jesterMode) return;
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            int hitgroup = @event.Hitgroup;

            if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim)) return;
            var attackerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == attacker?.SteamID);
            var victimInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == victim?.SteamID);

            if (attackerInfo?.Skill == skillName || victimInfo?.Skill == skillName)
                RestoreHealth(victim!, @event.DmgHealth);
        }

        private static void RestoreHealth(CCSPlayerController victim, float damage)
        {
            var playerPawn = victim.PlayerPawn.Value;
            if (playerPawn == null || !playerPawn.IsValid) return;
            var newHealth = playerPawn.Health + damage;

            if (newHealth > 100)
                newHealth = 100;

            playerPawn.Health = (int)newHealth;
        }

        public static void EnableSkill(CCSPlayerController _)
        {
            if (!jesterStarted)
            {
                float wait = (float)Instance.Random.NextDouble() * (maxTime - minTime) + minTime;
                Instance.AddTimer(wait, ChangeMode);
                jesterStarted = true;
            }
        }

        private static void ChangeMode()
        {
            if (Instance.SkillPlayer.Any(p => p.Skill == skillName))
            {
                jesterMode = !jesterMode;
                foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot && p.PawnIsAlive))
                {
                    if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) continue;
                    var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
                    if (playerInfo?.Skill == skillName)
                    {
                        SetPlayerColor(player.PlayerPawn.Value);
                        player.ExecuteClientCommand("play sounds/weapons/taser/taser_charge_ready");
                    }
                }
                float wait = (float)Instance.Random.NextDouble() * (maxTime - minTime) + minTime;
                Instance.AddTimer(wait, ChangeMode);
            }
        }

        private static void SetPlayerColor(CCSPlayerPawn pawn)
        {
            var color = jesterMode ? Color.FromArgb(255, 128, 0, 128) : Color.FromArgb(255, 255, 255, 255);
            pawn.Render = color;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
        }

        public static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot && p.PawnIsAlive))
            {
                if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
                if (playerInfo?.Skill == skillName)
                    UpdateHUD(player);
            }
        }

        private static void UpdateHUD(CCSPlayerController player)
        {
            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("jester_mode")}: <font color='{(jesterMode ? "#00ff00" : "#ff0000")}'>{Localization.GetTranslation(jesterMode ? "jester_on" : "jester_off")}</font></font> <br>";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#8f108f", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float minTime = 10f, float maxTime = 25f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float MinTime { get; set; } = minTime;
            public float MaxTime { get; set; } = maxTime;
        }
    }
}