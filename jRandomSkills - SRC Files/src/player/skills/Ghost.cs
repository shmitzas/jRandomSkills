using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Ghost : ISkill
    {
        private const Skills skillName = Skills.Ghost;
        private static bool roundEnd = false;
        private static string[] disabledWeapons =
        {
            "weapon_deagle",
            "weapon_revolver",
            "weapon_glock",
            "weapon_usp_silencer",
            "weapon_cz75a",
            "weapon_fiveseven",
            "weapon_p250",
            "weapon_tec9",
            "weapon_elite",
            "weapon_hkp2000",
            "weapon_ak47",
            "weapon_m4a1",
            "weapon_m4a4",
            "weapon_m4a1_silencer",
            "weapon_famas",
            "weapon_galilar",
            "weapon_aug",
            "weapon_sg553",
            "weapon_mp9",
            "weapon_mac10",
            "weapon_bizon",
            "weapon_mp7",
            "weapon_ump45",
            "weapon_p90",
            "weapon_mp5sd",
            "weapon_ssg08",
            "weapon_awp",
            "weapon_scar20",
            "weapon_g3sg1",
            "weapon_nova",
            "weapon_xm1014",
            "weapon_mag7",
            "weapon_sawedoff",
            "weapon_m249",
            "weapon_negev"
        };

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                roundEnd = false;
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
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
                        DisableSkill(player);
                }
                roundEnd = true;
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;
                if (!player.IsValid || player.PlayerPawn.Value == null) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    DisableSkill(player);

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                SetWeaponVisibility(player, false);
                SetWeaponAttack(player, true);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                    SetWeaponAttack(player, false);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, false);
            SetWeaponVisibility(player, false);
            SetWeaponAttack(player, true);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            SetPlayerVisibility(player, true);
            SetWeaponVisibility(player, true);
            SetWeaponAttack(player, false);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    UpdateHUD(player);
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
            }
        }

        private static void SetWeaponVisibility(CCSPlayerController player, bool visible)
        {
            if (!Instance.IsPlayerValid(player)) return;
            var playerPawn = player.PlayerPawn.Value;

            var color = visible ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(0, 255, 255, 255);
            var shadowStrength = visible ? 1.0f : 0.0f;

            foreach (var weapon in playerPawn.WeaponServices?.MyWeapons)
            {
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                {
                    weapon.Value.Render = color;
                    weapon.Value.ShadowStrength = shadowStrength;
                    Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_clrRender");
                }
            }
        }

        private static void SetWeaponAttack(CCSPlayerController player, bool disableWeapon)
        {
            if (roundEnd) return;
            foreach (var weapon in player.Pawn.Value.WeaponServices?.MyWeapons)
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                    if (disabledWeapons.Contains(weapon?.Value?.DesignerName))
                    {
                        weapon.Value.NextPrimaryAttackTick = disableWeapon ? int.MaxValue : Server.TickCount;
                        weapon.Value.NextSecondaryAttackTick = disableWeapon ? int.MaxValue : Server.TickCount;

                        Utilities.SetStateChanged(weapon.Value, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
                        Utilities.SetStateChanged(weapon.Value, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
                    }
        }

        private static void UpdateHUD(CCSPlayerController player)
        {
            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            var weapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value;
            if (weapon == null || !disabledWeapons.Contains(weapon.DesignerName)) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = $"<font class='fontSize-m' color='#FF0000'>{Localization.GetTranslation("disabled_weapon")}</font> <br>";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#FFFFFF", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : base(skill, active, color, onlyTeam, needsTeammates)
            {
            }
        }
    }
}