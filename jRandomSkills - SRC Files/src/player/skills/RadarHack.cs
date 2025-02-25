using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RadarHack : ISkill
    {
        private static Skills skillName = Skills.RadarHack;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#2effcb");
            Instance.RegisterListener<OnTick>(CheckRadarowiec);
        }

        private static void CheckRadarowiec()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Instance.IsPlayerValid(player)) continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                {
                    SetEnemiesVisibleOnRadar(player);
                }
            }
        }
        
        private static void SetEnemiesVisibleOnRadar(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) return;
            int playerIndex = (int)player.Index - 1;

            foreach (var enemy in Utilities.GetPlayers().FindAll(p => p.Team != player.Team && p.PawnIsAlive))
            {
                var enemyPawn = enemy.PlayerPawn.Value;
                if (enemyPawn == null) continue;
                enemyPawn.EntitySpottedState.SpottedByMask[0] |= (1u << (int)(playerIndex % 32));

            }

            var bombEntities = Utilities.FindAllEntitiesByDesignerName<CC4>("weapon_c4").ToList();
            if (bombEntities.Any())
            {
                var bomb = bombEntities.FirstOrDefault();
                if (bomb != null)
                    bomb.EntitySpottedState.SpottedByMask[0] |= (1u << (int)(playerIndex % 32));
            }
        }
    }
}