using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Soldier : ISkill
    {
        private const Skills skillName = Skills.Soldier;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

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
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            float newScale = (float)Instance.Random.NextDouble() * (Config.GetValue<float>(skillName, "ChanceTo") - Config.GetValue<float>(skillName, "ChanceFrom")) + Config.GetValue<float>(skillName, "ChanceFrom");
            playerInfo.SkillChance = newScale;
            newScale = (float)Math.Round(newScale, 2);
            SkillUtils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("soldier")}{ChatColors.Lime}: " + Localization.GetTranslation("soldier_desc2", newScale), false);
        }

        private static HookResult OnTakeDamage(DynamicHook h)
        {
            CEntityInstance param = h.GetParam<CEntityInstance>(0);
            CTakeDamageInfo param2 = h.GetParam<CTakeDamageInfo>(1);

            if (param == null || param.Entity == null || param2 == null || param2.Attacker == null || param2.Attacker.Value == null)
                return HookResult.Continue;

            CCSPlayerPawn attackerPawn = new CCSPlayerPawn(param2.Attacker.Value.Handle);
            CCSPlayerPawn victimPawn = new CCSPlayerPawn(param.Handle);

            if (attackerPawn.DesignerName != "player" || victimPawn.DesignerName != "player")
                return HookResult.Continue;

            if (attackerPawn == null || attackerPawn.Controller?.Value == null || victimPawn == null || victimPawn.Controller?.Value == null)
                return HookResult.Continue;

            CCSPlayerController attacker = attackerPawn.Controller.Value.As<CCSPlayerController>();
            CCSPlayerController victim = victimPawn.Controller.Value.As<CCSPlayerController>();

            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
            if (playerInfo == null) return HookResult.Continue;

            if (playerInfo.Skill == skillName && attacker.PawnIsAlive)
            {
                param2.Damage *= (float)playerInfo.SkillChance;
            }

            return HookResult.Continue;
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float ChanceFrom { get; set; }
            public float ChanceTo { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#09ba00", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float chanceFrom = 1.15f, float chanceTo = 1.35f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                ChanceFrom = chanceFrom;
                ChanceTo = chanceTo;
            }
        }
    }
}