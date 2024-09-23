using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Eliminator
    {
        public static void LoadEliminator()
        {
            Utils.RegisterSkill("Eliminator", "Możesz szybciej podłożyć bombę oraz ją zdefować", "#8A2BE2");
            
            Instance.RegisterEventHandler<EventBombBeginplant>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!IsPlayerValid(player)) return HookResult.Continue;

                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == "Eliminator")
                    {
                        var bombEntities = Utilities.FindAllEntitiesByDesignerName<CC4>("weapon_c4").ToList();

                        if (bombEntities.Any())
                        {
                            var bomb = bombEntities.FirstOrDefault();
                            if (bomb != null)
                            {
                                bomb.BombPlacedAnimation = false;
                                bomb.ArmedTime = 0.0f;
                            }
                        }
                    }
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventBombBegindefuse>((@event, info) =>
            {
                var player = @event.Userid;
                
                if (IsPlayerValid(player))
                {
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == "Eliminator")
                    {
                        var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
                        if (plantedBomb != null)
                        {
                            Server.NextFrame(() =>
                            {
                                plantedBomb.DefuseCountDown = 0;
                            });
                        }
                    }
                }

                return HookResult.Continue;
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}