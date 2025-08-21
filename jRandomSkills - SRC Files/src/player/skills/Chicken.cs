using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;
using jRandomSkills.src.utils;

namespace jRandomSkills
{
    public class Chicken : ISkill
    {
        private static Skills skillName = Skills.Chicken;
        private static string[] disabledWeapons =
        {
            "weapon_ak47",
            "weapon_m4a4",
            "weapon_m4a1",
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
        private static Dictionary<CCSPlayerController, CBaseModelEntity> chickens = new Dictionary<CCSPlayerController, CBaseModelEntity>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#FF8B42");
            
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
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!Instance.IsPlayerValid(player)) continue;

                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill != skillName) continue;
                    DisableSkill(player);
                }

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                DisableSkill(@event.Userid);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                SetWeaponAttack(player, true);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(OnTick);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn?.Value;
            if (playerPawn != null)
            {
                playerPawn.VelocityModifier = 1.1f;
                Utilities.SetStateChanged(player, "CCSPlayerPawn", "m_flVelocityModifier");

                playerPawn.Health = 50;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");

                playerPawn.CBodyComponent.SceneNode.Scale = 0.2f;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");

                playerPawn.Render = Color.FromArgb(0, 255, 255, 255);
                playerPawn.ShadowStrength = 0.0f;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");

                SetWeaponAttack(player, true);
                CreateChicken(player);
            }
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn?.Value;
            if (playerPawn != null)
            {
                playerPawn.VelocityModifier = 1f;
                Utilities.SetStateChanged(player, "CCSPlayerPawn", "m_flVelocityModifier");

                playerPawn.Health += 50;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");

                playerPawn.CBodyComponent.SceneNode.Scale = 1f;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");

                playerPawn.Render = Color.FromArgb(255, 255, 255, 255);
                playerPawn.ShadowStrength = 1.0f;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");

                SetWeaponAttack(player, false);
            }

            foreach (var chicken in chickens)
            {
                if (chicken.Value != null && chicken.Value.IsValid)
                    chicken.Value.Remove();
                chickens.Remove(chicken.Key);
            }
        }

        private static void SetWeaponAttack(CCSPlayerController player, bool disableWeapon)
        {
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

        private static void CreateChicken(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            var chickenModel = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            if (chickenModel == null)
                return;
            Vector pos = new Vector(0, 0, 0);

            chickenModel.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(chickenModel.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
            chickenModel.SetModel("models/chicken/chicken.vmdl");
            chickenModel.Render = Color.FromArgb(255, 255, 255, 255);
            chickenModel.Teleport(pos, playerPawn.AbsRotation, null);
            chickenModel.DispatchSpawn();
            chickenModel.AcceptInput("InitializeSpawnFromWorld", playerPawn, playerPawn, "");
            Utilities.SetStateChanged(chickenModel, "CBaseEntity", "m_CBodyComponent");
            Instance.AddTimer(1f, () => chickens.TryAdd(player, chickenModel));
        }

        private static async void OnTick()
        {
            foreach (var valuePair in chickens)
            {
                var player = valuePair.Key;
                var chicken = valuePair.Value;
                if (player == null || !player.IsValid) continue;
                if (chicken == null && !chicken.IsValid) continue;

                var pawn = player.Pawn.Value;
                if (pawn == null && !pawn.IsValid) continue;

                float X = (float)Math.Round(pawn.AbsOrigin.X, 2);
                float Y = (float)Math.Round(pawn.AbsOrigin.Y, 2);
                float Z = (float)Math.Round(pawn.AbsOrigin.Z, 2);
                Vector pos = new Vector(X, Y, Z);
                if (chicken.AbsOrigin.X != pos.X || chicken.AbsOrigin.Y != pos.Y || chicken.AbsOrigin.Z != pos.Z)
                    chicken.Teleport(pos, pawn.AbsRotation, null);
                UpdateHUD(player);
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
    }
}