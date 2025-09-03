using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Watchmaker : ISkill
    {
        private const Skills skillName = Skills.Watchmaker;
        private static int roundTime = Config.GetValue<int>(skillName, "changeRoundTime");
        private static bool bombPlanted = false;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                bombPlanted = false;
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventBombPlanted>((@event, info) =>
            {
                bombPlanted = true;
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (!name.EndsWith("_projectile"))
                    return;

                var grenade = @event.As<CBaseCSGrenadeProjectile>();
                if (grenade.OwnerEntity.Value == null || !grenade.OwnerEntity.Value.IsValid) return;

                var pawn = grenade.OwnerEntity.Value.As<CCSPlayerPawn>();
                var player = pawn.Controller.Value.As<CCSPlayerController>();

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return;

                Instance.GameRules.RoundTime += player.Team == CsTeam.Terrorist ? roundTime : -roundTime;
                if (player.Team == CsTeam.Terrorist)
                    Server.PrintToChatAll($" {ChatColors.Orange}{Localization.GetTranslation("watchmaker_tt", roundTime)}");
                else
                    Server.PrintToChatAll($" {ChatColors.LightBlue}{Localization.GetTranslation("watchmaker_ct", roundTime)}");
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            if (bombPlanted) return;
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    UpdateHUD(player);
            }
        }

        private static void UpdateHUD(CCSPlayerController player)
        {
            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null || Instance?.GameRules == null || Instance?.GameRules?.RoundTime == null || Instance.GameRules?.RoundStartTime == null) return;

            int seconds = 1 + (int)(Instance.GameRules.RoundTime - (Server.CurrentTime - Instance.GameRules.RoundStartTime));

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = $"<font class='fontSize-m' color='#FFFFFF'>{SkillUtils.SecondsToTimer(seconds)}</font> <br>";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int ChangeRoundTime { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#ff462e", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int changeRoundTime = 10) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                ChangeRoundTime = changeRoundTime;
            }
        }
    }
}