using CounterStrikeSharp.API;
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
        private static float maxDistance = Config.GetValue<float>(skillName, "maxDistance");

        public unsafe static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventWeaponFire>((@event, info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                var activeWeapon = player.Pawn.Value.WeaponServices.ActiveWeapon.Value;
                if (activeWeapon?.DesignerName != "weapon_knife") return HookResult.Continue;

                var pawn = player.PlayerPawn.Value;
                Vector eyePos = new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.ViewOffset.Z);
                Vector endPos = eyePos + SkillUtils.GetForwardVector(pawn.EyeAngles) * maxDistance;

                Ray ray = new Ray(Vector3.Zero);
                CTraceFilter filter = new CTraceFilter(pawn.Index, pawn.Index)
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
                    return HookResult.Continue;

                if (target.Handle == player.Handle || trace.Distance() <= 70) return HookResult.Continue;
                target.PlayerPawn.Value.EmitSound("Player.DamageBody.Onlooker");
                SkillUtils.TakeHealth(target.PlayerPawn.Value, Instance.Random.Next(21, 34));
                return HookResult.Continue;
            });
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float MaxDistance { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#c9f8ff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float maxDistance = 4096f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MaxDistance = maxDistance;
            }
        }
    }
}