using System.Diagnostics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static CounterStrikeSharp.API.Core.Listeners;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class PlayerOnTick
    {
        private static readonly Stopwatch DrawStopwatch = new Stopwatch();
        private static readonly float DrawUpdateInterval = 0.5f;

        public static void Load()
        {
            DrawStopwatch.Start();

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player != null && player.IsValid)
                    {
                        UpdatePlayerHud(player);
                    }
                }
            });
        }

        private static void UpdatePlayerHud(CCSPlayerController player)
        {
            if (player == null) return;

            var skillPlayer = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (skillPlayer == null) return;

            string infoLine = "";
            string skillLine = "";

            if (skillPlayer.IsDrawing && DrawStopwatch.Elapsed.TotalSeconds >= DrawUpdateInterval)
            {
                var randomSkill = SkillData.Skills[Instance.Random.Next(SkillData.Skills.Length)];
                infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>Losowanie mocy:</font> <br>";
                skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{randomSkill.Color}'>{randomSkill.Name}</font> <br>";
                DrawStopwatch.Restart();
            }
            else if (!skillPlayer.IsDrawing)
            {
                var skillInfo = SkillData.Skills.FirstOrDefault(s => s.Name == skillPlayer.Skill);
                if (skillInfo != null)
                {
                    infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>Twoja aktualna moc:</font> <br>";
                    skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillInfo.Color}'>{skillPlayer.Skill}</font> <br>";
                }
            }

            var hudContent = infoLine + skillLine;
            player.PrintToCenterHtml(hudContent);
        }
    }
}