using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class SoundMaker : ISkill
    {
        private const Skills skillName = Skills.SoundMaker;
        private static float timerCooldown = Config.GetValue<float>(skillName, "Cooldown");
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<ulong, PlayerSkillInfo>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) return;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
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

        public static void UseSkill(CCSPlayerController player)
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
                    MakeSound(player);
                }
                else
                    skillInfo.LastClick = DateTime.Now;
            }
        }

        private static void MakeSound(CCSPlayerController player)
        {
            foreach (var enemy in Utilities.GetPlayers().Where(p => p.Team != player.Team))
                if (enemy != null && enemy.IsValid && enemy.PawnIsAlive)
                    enemy.PlayerPawn.Value.EmitSound($"C4.PlantSoundB", volume: 1f);
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
            public DateTime LastClick { get; set; }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float Cooldown { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#e3ed8c", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 5f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                Cooldown = cooldown;
            }
        }
    }
}