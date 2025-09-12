using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Struct;
using jRandomSkills.src.player;
using System.Numerics;
using static jRandomSkills.jRandomSkills;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace jRandomSkills
{
    public class LongKnife : ISkill
    {
        private const Skills skillName = Skills.LongKnife;
        private static readonly float maxDistance = Config.GetValue<float>(skillName, "maxDistance");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public unsafe static void WeaponFire(EventWeaponFire @event)
        {
            var player = @event.Userid;
            if (!Instance.IsPlayerValid(player)) return;

            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player?.SteamID);
            if (playerInfo?.Skill != skillName) return;

            var pawn = player!.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid || pawn.AbsOrigin == null || pawn.WeaponServices == null) return;

            var activeWeapon = pawn.WeaponServices.ActiveWeapon.Value;
            if (activeWeapon == null || !activeWeapon.IsValid || activeWeapon.DesignerName != "weapon_knife") return;

            Vector eyePos = new(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.ViewOffset.Z);
            Vector endPos = eyePos + SkillUtils.GetForwardVector(pawn.EyeAngles) * maxDistance;

            Ray ray = new(Vector3.Zero);
            CTraceFilter filter = new(pawn.Index, pawn.Index)
            {
                m_nObjectSetMask = 0xf,
                m_nCollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT,
                m_nInteractsWith = pawn.GetInteractsWith(),
                m_nInteractsExclude = 0,
                m_nBits = 11,
                m_bIterateEntities = true,
                m_bHitTriggers = false,
                m_nInteractsAs = 0x40000
            };

            filter.m_nHierarchyIds[0] = pawn.GetHierarchyId();
            filter.m_nHierarchyIds[1] = 0;
            CGameTrace trace = TraceRay.TraceHull(eyePos, endPos, filter, ray);

            if (!trace.HitPlayer(out CCSPlayerController? target) || target == null)
                return;

            if (target.Handle == player.Handle || target.PlayerPawn.Value == null || !target.PlayerPawn.Value.IsValid || trace.Distance() <= 70) return;
            target.PlayerPawn.Value.EmitSound("Player.DamageBody.Onlooker");
            SkillUtils.TakeHealth(target.PlayerPawn.Value, Instance.Random.Next(21, 34));
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#c9f8ff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float maxDistance = 4096f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float MaxDistance { get; set; } = maxDistance;
        }
    }
}