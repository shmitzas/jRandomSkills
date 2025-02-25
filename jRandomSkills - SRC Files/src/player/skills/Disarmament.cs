using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Disarmament : ISkill
    {
        private static Skills skillName = Skills.Disarmament;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#FF4500", false);

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

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == skillName && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= playerInfo.SkillChance)
                    {
                        var weaponServices = victim.PlayerPawn?.Value?.WeaponServices;
                        if (weaponServices?.ActiveWeapon == null) return HookResult.Continue;

                        var weaponName = weaponServices?.ActiveWeapon?.Value?.DesignerName;
                        if (weaponName != null && !weaponName.Contains("weapon_knife") && !weaponName.Contains("weapon_c4"))
                            victim.DropActiveWeapon();
                    }
                }
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var skillConfig = Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString());
            if (skillConfig == null) return;

            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            float newChance = (float)Instance.Random.NextDouble() * (skillConfig.ChanceTo - skillConfig.ChanceFrom) + skillConfig.ChanceFrom;
            playerInfo.SkillChance = newChance;
            newChance = (float)Math.Round(newChance, 2) * 100;
            newChance = (float)Math.Round(newChance);
            Utils.PrintToChat(player, $"{ChatColors.DarkRed}{Localization.GetTranslation("disarmament")}{ChatColors.Lime}: " + Localization.GetTranslation("disarmament_desc2", newChance), false);
        }
    }
}