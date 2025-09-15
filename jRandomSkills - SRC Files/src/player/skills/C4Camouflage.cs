using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class C4Camouflage : ISkill
    {
        private const Skills skillName = Skills.C4Camouflage;
        private static readonly Dictionary<ulong, List<uint>> invisibleEntities = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void WeaponPickup(EventItemPickup @event)
        {
            var player = @event.Userid;
            if (!Instance.IsPlayerValid(player)) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
            if (playerInfo?.Skill != skillName) return;
            EnableSkill(player!);
        }

        public static void WeaponEquip(EventItemEquip @event)
        {
            var player = @event.Userid;
            var weapon = @event.Item;

            if (!Instance.IsPlayerValid(player)) return;
            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
            if (playerInfo?.Skill != skillName) return;

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
        }

        public static void CheckTransmit([CastFrom(typeof(nint))] CCheckTransmitInfoList infoList)
        {
            foreach (var (info, player) in infoList)
            {
                if (player == null) continue;
                foreach ((var playerId, var itemList) in invisibleEntities)
                    if (player.SteamID != playerId)
                        foreach (var item in itemList)
                            info.TransmitEntities.Remove(item);
            }
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.EnableTransmit();
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
            invisibleEntities.Remove(player.SteamID);
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

        private static void SetWeaponVisibility(CCSPlayerController player, bool visible)
        {
            if (!Instance.IsPlayerValid(player)) return;
            var playerPawn = player.PlayerPawn.Value!;
            if (playerPawn.WeaponServices == null) return;

            invisibleEntities.Remove(player.SteamID);
            foreach (var weapon in playerPawn.WeaponServices.MyWeapons)
            {
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                {
                    if (!visible)
                    {
                        if (invisibleEntities.TryGetValue(player.SteamID, out var items))
                        {
                            if (!items.Contains(weapon.Index))
                                items.Add(weapon.Index);
                        }
                        else
                            invisibleEntities.Add(player.SteamID, [weapon.Index]);
                    }
                }
            }

            if (visible)
                invisibleEntities.Remove(player.SteamID);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#00911f", CsTeam onlyTeam = CsTeam.Terrorist, bool needsTeammates = false) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
        }
    }
}