using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Impostor : ISkill
    {
        private static Skills skillName = Skills.Impostor;
        private static readonly string defaultCTModel = "characters/models/ctm_sas/ctm_sas.vmdl";
        private static readonly string defaultTModel = "characters/models/tm_phoenix_heavy/tm_phoenix_heavy.vmdl";

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#99140B");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                        {
                            string model = GetEnemyModel(player);
                            SetPlayerModel(player, model);
                        }
                    }
                });

                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            string model = GetEnemyModel(player);
            SetPlayerModel(player, model);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SetPlayerModel(player, player.Team == CsTeam.Terrorist ? defaultTModel : defaultCTModel);
        }

        private static string GetEnemyModel(CCSPlayerController player)
        {
            CCSPlayerController[] models = Utilities.GetPlayers().FindAll(p => p.IsValid && p.PawnIsAlive && p.Team != player.Team).ToArray();
            if (models.Length > 0)
            {
                string modelName = models[Instance.Random.Next(models.Length)].PlayerPawn.Value.CBodyComponent.SceneNode.GetSkeletonInstance().ModelState.ModelName;
                if (modelName != null) return modelName;
            }
            return player.Team == CsTeam.CounterTerrorist ? defaultTModel : defaultCTModel;
        }

        private static void SetPlayerModel(CCSPlayerController player, string model)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            Server.NextFrame(() =>
            {
                pawn.SetModel(model);

                var originalRender = pawn.Render;
                pawn.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
            });
        }
    }
}