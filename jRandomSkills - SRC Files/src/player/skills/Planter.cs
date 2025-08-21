using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Planter : ISkill
    {
        private static Skills skillName = Skills.Planter;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#7d7d7d");

            Instance.RegisterEventHandler<EventRoundStart>((@event, @info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!Instance.IsPlayerValid(player)) continue;
                    Schema.SetSchemaValue<bool>(player.Pawn.Value.Handle, "CCSPlayerPawn", "m_bInBombZone", false);
                }
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventBombPlanted>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!Instance.IsPlayerValid(player)) continue;

                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill != skillName) continue;

                    var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
                    if (plantedBomb != null)
                    {
                        Server.NextFrame(() =>
                        {
                            plantedBomb.C4Blow = (float)Server.EngineTime + 60;
                        });
                    }
                }

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            Schema.SetSchemaValue<bool>(player.Pawn.Value.Handle, "CCSPlayerPawn", "m_bInBombZone", false);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Instance.IsPlayerValid(player)) continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                    Schema.SetSchemaValue<bool>(player.Pawn.Value.Handle, "CCSPlayerPawn", "m_bInBombZone", true);
            }
        }
    }
}