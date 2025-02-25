using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Retreat : ISkill
    {
        private static Skills skillName = Skills.Retreat;
        private static float timerCooldown = (float)(Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Cooldown);
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<ulong, PlayerSkillInfo>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#a86eff");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                        {
                            EnableSkill(player);
                        }
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                SkillPlayerInfo.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    if (SkillPlayerInfo.ContainsKey(player.SteamID))
                        SkillPlayerInfo.Remove(player.SteamID);

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == skillName)
                        if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
                            UpdateHUD(player, skillInfo);
                }
            });

            Instance.AddCommand("css_useSkill", "Use Skill", (player, _) =>
            {
                if (player == null) return;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    UseSkill(player);
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo[player.SteamID] = new PlayerSkillInfo
            {
                SteamID = player.SteamID,
                CanUse = true,
                Cooldown = DateTime.MinValue,
                LastClick = DateTime.MinValue,
            };
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (SkillPlayerInfo.ContainsKey(player.SteamID))
                SkillPlayerInfo.Remove(player.SteamID);
        }

        private static void UpdateHUD(CCSPlayerController player, PlayerSkillInfo skillInfo)
        {
            float cooldown = 0;
            if (skillInfo != null)
            {
                float time = (int)(skillInfo.Cooldown.AddSeconds(timerCooldown) - DateTime.Now).TotalSeconds;
                cooldown = Math.Max(time, 0);

                if (cooldown == 0 && skillInfo?.CanUse == false)
                    skillInfo.CanUse = true;
            }

            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("hud_info", $"<font color='#FF0000'>{cooldown}</font>")}</font> <br>" : "";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        private static void ActiveUse(CCSPlayerController player)
        {
            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                skillInfo.CanUse = true;
            }
        }

        private static void UseSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;

            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                if (!player.IsValid || !player.PawnIsAlive) return;
                if (skillInfo.CanUse)
                {
                    skillInfo.CanUse = false;
                    skillInfo.Cooldown = DateTime.Now;
                    playerPawn.Teleport(GetSpawnVector(player));
                } 
                else
                    skillInfo.LastClick = DateTime.Now;
            }
        }

        private static Vector GetSpawnVector(CCSPlayerController player)
        {
            var abs = player.PlayerPawn.Value.AbsOrigin;
            var spawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(player.Team == CsTeam.Terrorist ? "info_player_terrorist" : "info_player_counterterrorist").ToList();
            if (spawns.Any())
            {
                var randomSpawn = spawns[Instance.Random.Next(spawns.Count)];
                return randomSpawn.AbsOrigin;
            }
            return new Vector(abs.X, abs.Y, abs.Z);
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
            public DateTime LastClick {  get; set; }
        }
    }
}