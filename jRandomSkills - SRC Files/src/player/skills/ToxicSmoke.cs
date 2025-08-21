using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class ToxicSmoke : ISkill
    {
        private static Skills skillName = Skills.ToxicSmoke;
        private static List<CCSPlayerPawn> players = new List<CCSPlayerPawn>();
        private static Dictionary<int, CTriggerMultiple> triggers = new Dictionary<int, CTriggerMultiple>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#507529");

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                players.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventSmokegrenadeDetonate>((@event, @info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");
                if (trigger == null) return HookResult.Continue;

                trigger.Collision.SolidType = SolidType_t.SOLID_CAPSULE;
                trigger.Collision.SolidFlags = 0;
                trigger.Spawnflags = 1;
                trigger.Globalname = $"toxic_smoke_{trigger.Index}";
                trigger.Collision.SolidFlags = 1;

                trigger.AbsOrigin.X = @event.X;
                trigger.AbsOrigin.Y = @event.Y;
                trigger.AbsOrigin.Z = @event.Z;

                trigger.Collision.CapsuleRadius = 160;
                trigger.Collision.BoundingRadius = 160;

                trigger.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_TRIGGER;
                trigger.Collision.EnablePhysics = 1;
                trigger.Collision.TriggerBloat = 0;

                trigger.Collision.SurroundType = SurroundingBoundsType_t.USE_OBB_COLLISION_BOUNDS;
                trigger.Collision.CollisionAttribute.CollisionFunctionMask = 39;
                trigger.Collision.CollisionAttribute.CollisionGroup = 2;

                trigger.DispatchSpawn();
                triggers.Add(@event.Entityid, trigger);
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventSmokegrenadeExpired>((@event, @info) =>
            {
                var player = @event.Userid;
                if (!Instance.IsPlayerValid(player)) return HookResult.Continue;

                if (triggers.TryGetValue(@event.Entityid, out var existingTrigger))
                {
                    existingTrigger.AcceptInput("Kill");
                    triggers.Remove(@event.Entityid);
                }

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (CCSPlayerPawn player in players)
                {
                    if (Server.TickCount % 10 == 0)
                        AddHealth(player, -2);
                }
            });

            VirtualFunctions.CBaseTrigger_StartTouchFunc.Hook(StartTouchFun, HookMode.Post);
            VirtualFunctions.CBaseTrigger_EndTouchFunc.Hook(EndTouchFunc, HookMode.Post);
        }

        private static void AddHealth(CCSPlayerPawn player, int health)
        {
            if (player.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                return;

            player.Health += health;
            Utilities.SetStateChanged(player, "CBaseEntity", "m_iHealth");

            player.EmitSound("");
            if (player.Health <= 0)
                player.CommitSuicide(false, true);
        }

        private static HookResult StartTouchFun(DynamicHook h)
        {
            CBaseTrigger trigger = h.GetParam<CBaseTrigger>(0);
            CBaseEntity entity = h.GetParam<CBaseEntity>(1);

            if (trigger == null || entity == null)
                return HookResult.Continue;

            CCSPlayerPawn player = new CCSPlayerPawn(entity.Handle);
            if (player == null) return HookResult.Continue;

            if (string.IsNullOrEmpty(trigger?.Globalname) || trigger?.Globalname?.StartsWith("toxic_smoke_") == false)
                return HookResult.Continue;

            if (!players.Contains(player))
                players.Add(player);
            return HookResult.Continue;
        }

        private static HookResult EndTouchFunc(DynamicHook h)
        {
            var trigger = h.GetParam<CBaseTrigger>(0);
            var entity = h.GetParam<CBaseEntity>(1);

            if (trigger == null || entity == null) return HookResult.Continue;

            CCSPlayerPawn player = new CCSPlayerPawn(entity.Handle);
            if (player == null) return HookResult.Continue;

            if (string.IsNullOrEmpty(trigger?.Globalname) || trigger?.Globalname?.StartsWith("toxic_smoke_") == false)
                return HookResult.Continue;

            if (players.Contains(player))
                players.Remove(player);
            return HookResult.Continue;
        }
    }
}