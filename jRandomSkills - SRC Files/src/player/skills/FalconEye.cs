using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using System.Drawing;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class FalconEye : ISkill
    {
        private const Skills skillName = Skills.FalconEye;
        private static bool blocked = false;
        private static float distance = Config.GetValue<float>(skillName, "distance");
        private static Dictionary<ulong, (uint, CDynamicProp)> cameras = new Dictionary<ulong, (uint, CDynamicProp)>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                blocked = true;
                foreach (var player in Utilities.GetPlayers())
                    if (cameras.TryGetValue(player.SteamID, out _))
                        DisableSkill(player);

                foreach (var camera in cameras)
                    camera.Value.Item2.Remove();
                cameras.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                blocked = false;
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventItemPickup>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                if (cameras.TryGetValue(player.SteamID, out var cameraInfo) && cameraInfo.Item1 == player.PlayerPawn.Value.CameraServices!.ViewEntity!.Raw)
                    BlockWeapon(player, true);
                return HookResult.Continue;
            });

            Instance.RegisterListener<Listeners.OnTick>(OnTick);
        }

        public static void UseSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null || blocked) return;
            ChangeCamera(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            ChangeCamera(player, true);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
                if (cameras.TryGetValue(player.SteamID, out var cameraInfo) && cameraInfo.Item2.IsValid)
                {
                    var pawn = player.PlayerPawn.Value;
                    if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                    {
                        ChangeCamera(player, true);
                        continue;
                    }
                    Vector pos = new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 1000);
                    QAngle angle = new QAngle(90, 0, -pawn.V_angle.Y);
                    cameraInfo.Item2.Teleport(pos, angle);
                }
        }

        private static void ChangeCamera(CCSPlayerController player, bool forceToDefault = false)
        {
            uint orginalCameraRaw;
            uint newCameraRaw;
            var pawn = player.PlayerPawn.Value;
            if (cameras.TryGetValue(player.SteamID, out var cameraInfo) && cameraInfo.Item2.IsValid)
            {
                orginalCameraRaw = cameraInfo.Item1;
                newCameraRaw = cameraInfo.Item2.EntityHandle.Raw;
            }
            else
            {
                orginalCameraRaw = pawn!.CameraServices!.ViewEntity.Raw;
                newCameraRaw = CreateCamera(player);
            }

            if (newCameraRaw == 0)
                return;

            bool defaultCam = forceToDefault ? true : (pawn.CameraServices!.ViewEntity!.Raw == orginalCameraRaw ? false : true);
            pawn!.CameraServices!.ViewEntity.Raw = defaultCam ? orginalCameraRaw : newCameraRaw;
            Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pCameraServices");
            BlockWeapon(player, !defaultCam);
        }

        private static uint CreateCamera(CCSPlayerController player)
        {
            var camera = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            if (camera == null || !camera.IsValid) return 0;

            var pawn = player.PlayerPawn.Value;
            Vector pos = new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + distance);

            camera.Render = Color.FromArgb(0, 255, 255, 255);
            camera.Teleport(pos, new QAngle(90, 0, 0));
            camera.DispatchSpawn();
            cameras[player.SteamID] = (pawn.CameraServices!.ViewEntity.Raw, camera);
            return camera.EntityHandle.Raw;
        }

        private static void BlockWeapon(CCSPlayerController player, bool block)
        {
            foreach (var weapon in player.Pawn.Value.WeaponServices?.MyWeapons)
                if (weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
                {
                    weapon.Value.NextPrimaryAttackTick = block ? int.MaxValue : Server.TickCount;
                    weapon.Value.NextSecondaryAttackTick = block ? int.MaxValue : Server.TickCount;

                    Utilities.SetStateChanged(weapon.Value, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
                    Utilities.SetStateChanged(weapon.Value, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
                }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float Distance { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#d1f542", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float distance = 1000f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                Distance = distance;
            }
        }
    }
}