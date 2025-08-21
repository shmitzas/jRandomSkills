using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class C4Camouflage : ISkill
    {
        private static Skills skillName = Skills.C4Camouflage;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#00911f");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerPawn = player.PlayerPawn?.Value;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                    DisableSkill(player);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                EnableSkill(player);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemEquip>((@event, info) =>
            {
                var player = @event.Userid;
                var weapon = @event.Item;
                
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                if (weapon == "c4")
                {
                    SetPlayerVisibility(player, false);
                    SetWeaponVisibility(player, false);
                }
                else
                {
                    SetPlayerVisibility(player, true);
                    SetWeaponVisibility(player, true);
                }
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var activeWeapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value;
            if (activeWeapon == null || !activeWeapon.IsValid || activeWeapon.DesignerName != "weapon_c4") return;
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
            if (playerPawn != null)
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

            var color = Color.FromArgb(enabled ? 255 : 0, 255, 255, 255);

            foreach (var weapon in playerPawn.WeaponServices?.MyWeapons)
            {
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                {
                    weapon.Value.Render = color;
                    Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_clrRender");
                }
            }
        }
    }
}