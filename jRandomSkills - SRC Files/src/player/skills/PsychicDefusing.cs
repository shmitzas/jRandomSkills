using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class PsychicDefusing : ISkill
    {
        private const Skills skillName = Skills.PsychicDefusing;
        private static readonly Dictionary<CCSPlayerPawn, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<CCSPlayerPawn, PlayerSkillInfo>();
        private static Vector bombLocation = null;
        private static float maxDefusingRange = Config.GetValue<float>(skillName, "maxDefusingRange");
        private static float defusingTime = Config.GetValue<float>(skillName, "defusingTime");
        private static float tickRate = 64f;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

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
                SkillPlayerInfo.Clear();
                bombLocation = null;
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    if (SkillPlayerInfo.ContainsKey(player.PlayerPawn.Value))
                        SkillPlayerInfo.Remove(player.PlayerPawn.Value);

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventBombPlanted>((@event, info) =>
            {
                var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
                if (plantedBomb != null)
                    bombLocation = plantedBomb.AbsOrigin;
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                if (bombLocation == null) return;
                foreach (var skillInfo in SkillPlayerInfo)
                {
                    var player = skillInfo.Key;
                    var info = skillInfo.Value;

                    if (SkillUtils.GetDistance(player.AbsOrigin, bombLocation) > maxDefusingRange)
                    {
                        info.Defusing = false;
                        info.DefusingTime = defusingTime;
                        continue;
                    }

                    if (!info.Defusing)
                        player.EmitSound("c4.disarmstart");
                    info.Defusing = true;
                    info.DefusingTime -= (1f / tickRate);

                    if (info.DefusingTime <= 0)
                    {
                        var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
                        if (plantedBomb != null)
                        {
                            plantedBomb.Remove();
                            SkillUtils.TerminateRound(CsTeam.CounterTerrorist);
                        }

                        SkillPlayerInfo.Clear();
                    }

                    UpdateHUD(player.Controller.Value.As<CCSPlayerController>(), info);
                }
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo[player.PlayerPawn.Value] = new PlayerSkillInfo
            {
                SteamID = player.SteamID,
                Defusing = false,
                DefusingTime = defusingTime,
            };
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (SkillPlayerInfo.ContainsKey(player.PlayerPawn.Value))
                SkillPlayerInfo.Remove(player.PlayerPawn.Value);
        }

        private static void UpdateHUD(CCSPlayerController player, PlayerSkillInfo skillInfo)
        {
            if (!skillInfo.Defusing) return;
            int cooldown = (int)skillInfo.DefusingTime + 1;

            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("psychicdefusing_hud_info", $"<font color='#00d5ff'>{cooldown}</font>")}</font> <br>" : "";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }
        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool Defusing { get; set; }
            public float DefusingTime { get; set; }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float MaxDefusingRange { get; set; }
            public float DefusingTime { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#507529", CsTeam onlyTeam = CsTeam.CounterTerrorist, bool needsTeammates = false, float maxDefusingRange = 80f, float defusingTime = 10f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MaxDefusingRange = maxDefusingRange;
                DefusingTime = defusingTime;
            }
        }
    }
}