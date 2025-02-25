using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Ghost : ISkill
    {
        private static Skills skillName = Skills.Ghost;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#FFFFFF");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                        {
                            SetPlayerVisibility(player, false);
                        }
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!Instance.IsPlayerValid(player)) continue;
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == skillName)
                        SetPlayerVisibility(player, true);
                }
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;
                if (!player.IsValid || player.PlayerPawn.Value == null) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    SetPlayerVisibility(player, true);

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemEquip>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                SetWeaponVisibility(player, false);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, false);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, true);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    var activeWeapon = player.Pawn.Value.WeaponServices?.ActiveWeapon.Value;
                    if (activeWeapon != null && activeWeapon.IsValid && activeWeapon.Clip1 != 0)
                    {
                        activeWeapon.Clip1 = 0;
                        activeWeapon.Clip2 = 0;
                    }
                }
            }
        }

        private static void SetPlayerVisibility(CCSPlayerController player, bool visible)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null)
            {
                var color = visible ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(0, 255, 255, 255);
                var shadowStrength = visible ? 1.0f : 0.0f;

                playerPawn.Render = color;
                playerPawn.ShadowStrength = shadowStrength;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");

                SetWeaponVisibility(player, visible);
            }
        }

        private static void SetWeaponVisibility(CCSPlayerController player, bool visible)
        {
            if (!Instance.IsPlayerValid(player)) return;
            var playerPawn = player.PlayerPawn.Value;

            var color = visible ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(0, 255, 255, 255);
            var shadowStrength = visible ? 1.0f : 0.0f;

            var activeWeapon = playerPawn.WeaponServices?.ActiveWeapon.Value;
            if (activeWeapon != null && activeWeapon.IsValid)
            {
                activeWeapon.Render = color;
                activeWeapon.ShadowStrength = shadowStrength;
                Utilities.SetStateChanged(activeWeapon, "CBaseModelEntity", "m_clrRender");
            }

            var myWeapons = playerPawn.WeaponServices?.MyWeapons;
            if (myWeapons != null)
            {
                foreach (var gun in myWeapons)
                {
                    var weapon = gun.Value;
                    if (weapon != null)
                    {
                        weapon.Render = color;
                        weapon.ShadowStrength = shadowStrength;
                        Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                    }
                }
            }
        }
    }
}