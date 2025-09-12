using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;

namespace jRandomSkills
{
    public class Distancer : ISkill
    {
        private const Skills skillName = Skills.Distancer;
        private static readonly HashSet<CCSPlayerController> distancerPlayers = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            distancerPlayers.Clear();
        }

        public static void OnTick()
        {
            foreach (var player in distancerPlayers)
            {
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn == null || !playerPawn.IsValid) return;
                if (playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;

                var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
                if (skillData == null) return;

                string closetEnemy = "Bot";
                double closetDistance = double.MaxValue;

                foreach (var enemy in Utilities.GetPlayers().Where(p => p.Team != player.Team))
                {
                    var enemyPawn = enemy.PlayerPawn.Value;
                    if (enemyPawn == null || !enemyPawn.IsValid) continue;
                    if (enemyPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE || playerPawn.AbsOrigin == null || enemyPawn.AbsOrigin == null) continue;
                    double distance = (int)SkillUtils.GetDistance(playerPawn.AbsOrigin, enemyPawn.AbsOrigin);
                    if (distance >= closetDistance) continue;
                    closetDistance = distance;
                    closetEnemy = enemy.PlayerName;
                }

                string distanceColor = closetDistance > 1500 ? "#00FF00" : closetDistance > 600 ? "#FFFF00" : "#FF0000";

                string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
                string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
                string remainingLine = $"<font class='fontSize-m' color='#FFFFFF'>{closetEnemy}: <font color='{distanceColor}'>{closetDistance}</font></font> <br>";

                var hudContent = infoLine + skillLine + remainingLine;
                player.PrintToCenterHtml(hudContent);
            }
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            distancerPlayers.Add(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            distancerPlayers.Remove(player);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#00f2ff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}