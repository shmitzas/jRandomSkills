using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using System.Drawing;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Wallhack : ISkill
    {
        private static Skills skillName = Skills.Wallhack;
        private static List<CDynamicProp> glows = new List<CDynamicProp>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#5d00ff");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, @info) =>
            {
                glows.Clear();
                Instance.AddTimer(.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                        {
                            EnableSkill(player);
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetGlowEffectForEnemies(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            foreach (var glow in glows)
            {
                glows.Remove(glow);
                glow.Remove();
            }
        }

        private static void SetGlowEffectForEnemies(CCSPlayerController player)
        {
            foreach (var enemy in Utilities.GetPlayers().FindAll(p => p.Team != player.Team && p.PawnIsAlive))
            {
                var enemyPawn = enemy.PlayerPawn.Value;
                var modelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
                var modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

                if (modelGlow == null || modelRelay == null)
                    return;

                modelRelay.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(modelRelay.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
                modelRelay.SetModel(enemyPawn!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
                modelRelay.Spawnflags = 256u;
                modelRelay.RenderMode = RenderMode_t.kRenderNone;
                modelRelay.DispatchSpawn();

                modelGlow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(modelGlow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
                modelGlow.SetModel(enemyPawn!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
                modelGlow.Spawnflags = 256u;
                modelGlow.Render = Color.FromArgb(1, 255, 255, 255);
                modelGlow.DispatchSpawn();

                modelGlow.Glow.GlowColorOverride = enemy.Team == CsTeam.Terrorist ? Color.FromArgb(255, 255, 165, 0) : Color.FromArgb(255, 173, 216, 230);
                modelGlow.Glow.GlowRange = 5000;
                modelGlow.Glow.GlowTeam = (int)player.Team;
                modelGlow.Glow.GlowType = 3;
                modelGlow.Glow.GlowRangeMin = 100;

                modelRelay.AcceptInput("FollowEntity", enemyPawn, modelRelay, "!activator");
                modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
                glows.Add(modelRelay);
            }
        }
    }
}