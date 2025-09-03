using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Magnifier : ISkill
    {
        private const Skills skillName = Skills.Magnifier;
        private static uint customFOV = Config.GetValue<uint>(skillName, "customFOV");
        private static Dictionary<CCSPlayerController, uint> playersFOV = new Dictionary<CCSPlayerController, uint>();

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
                foreach (var player in playersFOV.Keys)
                    DisableSkill(player);
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

            if (!playersFOV.ContainsKey(enemy))
                playersFOV.Add(enemy, enemy.DesiredFOV);
            Server.PrintToChatAll($"FOV: {enemy.DesiredFOV}");
            enemy.DesiredFOV = customFOV;
            Utilities.SetStateChanged(enemy, "CBasePlayerController", "m_iDesiredFOV");

            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("magnifier_player_info", enemy.PlayerName));
            enemy.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("magnifier_enemy_info"));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("magnifier") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("magnifier_select_info")}");
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
            if (playersFOV.TryGetValue(player, out uint fov))
            {
                player.DesiredFOV = fov;
                Utilities.SetStateChanged(player, "CBasePlayerController", "m_iDesiredFOV");
            }
            playersFOV.Remove(player);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public uint CustomFOV { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#9ba882", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, uint customFOV = 50) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                CustomFOV = customFOV;
            }
        }
    }
}