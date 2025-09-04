using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class C4Camouflage : ISkill
    {
        private const Skills skillName = Skills.C4Camouflage;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        DisableSkill(player);
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerPawn = player.PlayerPawn?.Value;

                        var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null || !player.IsValid || player.PlayerPawn.Value == null) return HookResult.Continue;

                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    DisableSkill(player);

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                EnableSkill(player!);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemEquip>((@event, info) =>
            {
                var player = @event.Userid;
                var weapon = @event.Item;
                
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                if (weapon == "c4")
                {
                    SetPlayerVisibility(player!, false);
                    SetWeaponVisibility(player!, false);
                }
                else
                {
                    SetPlayerVisibility(player!, true);
                    SetWeaponVisibility(player!, true);
                }
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;
            var playerPawn = player.PlayerPawn.Value;

            if (playerPawn == null || !playerPawn.IsValid) return;
            if (playerPawn.WeaponServices == null || playerPawn.WeaponServices.ActiveWeapon == null || !playerPawn.WeaponServices.ActiveWeapon.IsValid) return;
            if (playerPawn.WeaponServices.ActiveWeapon.Value == null || !playerPawn.WeaponServices.ActiveWeapon.Value.IsValid) return;

            var activeWeapon = playerPawn.WeaponServices.ActiveWeapon.Value;
            if (activeWeapon.DesignerName != "weapon_c4") return;

            SetPlayerVisibility(player, false);
            SetWeaponVisibility(player, false);
        }


        public static void DisableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, true);
            SetWeaponVisibility(player, true);
        }

        private static void SetPlayerVisibility(CCSPlayerController player, bool enabled)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null && playerPawn.IsValid)
            {
                var color = Color.FromArgb(enabled ? 255 : 0, 255, 255, 255);
                playerPawn.Render = color;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            }
        }

        private static void SetWeaponVisibility(CCSPlayerController player, bool enabled)
        {
            if (!Instance.IsPlayerValid(player)) return;
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null || !playerPawn.IsValid) return;
            var weaponServices = playerPawn.WeaponServices;
            if (weaponServices == null) return;

            var color = Color.FromArgb(enabled ? 255 : 0, 255, 255, 255);
            foreach (var weapon in weaponServices.MyWeapons)
            {
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                {
                    weapon.Value.Render = color;
                    Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_clrRender");
                }
            }
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#00911f", CsTeam onlyTeam = CsTeam.Terrorist, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}