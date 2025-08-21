using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Ninja : ISkill
    {
        private static Skills skillName = Skills.Ninja;
        private static float idlePercentInvisibility = 0.3f;
        private static float duckPercentInvisibility = 0.3f;
        private static float knifePercentInvisibility = 0.3f;
        private static Dictionary<nint, float> invisibilityChanged = new Dictionary<nint, float>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#dedede");

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                    DisableSkill(player);
                invisibilityChanged.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                UpdateNinja(player);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    UpdateNinja(player);
            }
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, 0);
            SetWeaponVisibility(player, 0);
        }

        private static void UpdateNinja(CCSPlayerController player)
        {
            var flags = (PlayerFlags)player.PlayerPawn.Value.Flags;
            var buttons = player.Buttons;
            var activeWeapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value;
            float percentInvisibility = 0;

            if (buttons.HasFlag(PlayerButtons.Duck))
                percentInvisibility += duckPercentInvisibility;
            if (activeWeapon.DesignerName == "weapon_knife")
                percentInvisibility += knifePercentInvisibility;
            if (!buttons.HasFlag(PlayerButtons.Moveleft) && !buttons.HasFlag(PlayerButtons.Moveright) && !buttons.HasFlag(PlayerButtons.Forward) && !buttons.HasFlag(PlayerButtons.Back) && flags.HasFlag(PlayerFlags.FL_ONGROUND))
                percentInvisibility += idlePercentInvisibility;

            if (invisibilityChanged.TryGetValue(player.Handle, out float oldInvisibility))
                if (percentInvisibility == oldInvisibility)
                    return;

            invisibilityChanged[player.Handle] = percentInvisibility;
            SetPlayerVisibility(player, percentInvisibility);
            SetWeaponVisibility(player, percentInvisibility);
        }

        private static void SetPlayerVisibility(CCSPlayerController player, float percentInvisibility)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null)
            {
                var color = Color.FromArgb(Math.Max(255 - (int)(255 * percentInvisibility), 0), 255, 255, 255);
                playerPawn.Render = color;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            }
        }

        private static void SetWeaponVisibility(CCSPlayerController player, float percentInvisibility)
        {
            if (!Instance.IsPlayerValid(player)) return;
            var playerPawn = player.PlayerPawn.Value;

            var color = Color.FromArgb(Math.Max(255 - (int)(255 * percentInvisibility * 2), 0), 255, 255, 255);

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