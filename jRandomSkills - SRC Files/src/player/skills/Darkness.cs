using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Darkness : ISkill
    {
        private const Skills skillName = Skills.Darkness;
        private static float brightness = Config.GetValue<float>(skillName, "brightness");
        private static Dictionary<CCSPlayerController, CPostProcessingVolume> deafultPostProcessing = new Dictionary<CCSPlayerController, CPostProcessingVolume>();
        private static List<CPostProcessingVolume> newPostProcessing = new List<CPostProcessingVolume>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in deafultPostProcessing.Keys)
                    DisableSkill(player);
                foreach (var postProcessing in newPostProcessing)
                    postProcessing.Remove();
                newPostProcessing.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                SetUpPostProcessing(@event.Userid, true);
                return HookResult.Continue;
            });
        }

        public static void TypeSkill(CCSPlayerController player, string[] commands)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) return;
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            if (playerInfo.SkillChance == 1)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_used_info")}");
                return;
            }

            string enemyId = commands[0];
            var enemy = Utilities.GetPlayers().FirstOrDefault(p => p.Team != player.Team && p.Index.ToString() == enemyId);

            if (enemy == null)
            {
                player.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("selectplayerskill_incorrect_enemy_index"));
                return;
            }

            SetUpPostProcessing(enemy);
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("darkness_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("darkness_enemy_info"));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("darkness") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("darkness_select_info")}");
            var enemies = Utilities.GetPlayers().Where(p => p.Team != player.Team && p.IsValid && !p.IsBot).ToArray();
            if (enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                    player.PrintToChat($" {ChatColors.Green}⠀⠀⠀[{ChatColors.Red}{enemy.Index}{ChatColors.Green}] {enemy.PlayerName}");
            }
            else
                player.PrintToChat($" {ChatColors.Red}⠀⠀⠀{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("selectplayerskill_command")} {ChatColors.Red}index");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SetUpPostProcessing(player, true);
        }

        private static void SetUpPostProcessing(CCSPlayerController player, bool dontCreateNew = false)
        {
            if (deafultPostProcessing.TryGetValue(player, out var oldPostProcessing))
            {
                player.PlayerPawn.Value.CameraServices.PostProcessingVolumes.FirstOrDefault().Raw = oldPostProcessing.EntityHandle.Raw;
                deafultPostProcessing.Remove(player);
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
                return;
            }

            if (dontCreateNew)
                return;

            var postProcessing = Utilities.CreateEntityByName<CPostProcessingVolume>("post_processing_volume");
            postProcessing.ExposureControl = true;
            postProcessing.MaxExposure = brightness;
            postProcessing.MinExposure = brightness;
            deafultPostProcessing.TryAdd(player, player.PlayerPawn.Value.CameraServices.PostProcessingVolumes.FirstOrDefault().Value);
            player.PlayerPawn.Value.CameraServices.PostProcessingVolumes.FirstOrDefault().Raw = postProcessing.EntityHandle.Raw;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float Brightness { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#383838", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float brightness = .01f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                Brightness = brightness;
            }
        }
    }
}