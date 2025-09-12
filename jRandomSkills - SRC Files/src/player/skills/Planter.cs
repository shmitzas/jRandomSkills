using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Planter : ISkill
    {
        private const Skills skillName = Skills.Planter;
        private static readonly int extraC4BlowTime = Config.GetValue<int>(skillName, "extraC4BlowTime");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void BombPlanted(EventBombPlanted @event)
        {
            var player = @event.Userid;
            if (!Instance.IsPlayerValid(player)) return;

            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
            if (playerInfo?.Skill != skillName) return;

            var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
            if (plantedBomb != null)
                Server.NextFrame(() => plantedBomb.C4Blow = (float)Server.EngineTime + extraC4BlowTime);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (!Instance.IsPlayerValid(player)) return;
            Schema.SetSchemaValue<bool>(player!.PlayerPawn.Value!.Handle, "CCSPlayerPawn", "m_bInBombZone", false);
        }

        public static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Instance.IsPlayerValid(player)) continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                    Schema.SetSchemaValue<bool>(player!.PlayerPawn.Value!.Handle, "CCSPlayerPawn", "m_bInBombZone", true);
            }
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#7d7d7d", CsTeam onlyTeam = CsTeam.Terrorist, bool needsTeammates = false, int extraC4BlowTime = 60) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int ExtraC4BlowTime { get; set; } = extraC4BlowTime;
        }
    }
}