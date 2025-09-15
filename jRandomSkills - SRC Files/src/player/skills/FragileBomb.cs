using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class FragileBomb : ISkill
    {
        private const Skills skillName = Skills.FragileBomb;
        private static int bombHealth = Config.GetValue<int>(skillName, "maxBombHealth");
        private static readonly int maxBombHealth = Config.GetValue<int>(skillName, "maxBombHealth");

        private static CPlantedC4? plantedC4;
        private static CTriggerMultiple? triggerC4;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            bombHealth = maxBombHealth;
            plantedC4 = null;
            triggerC4 = null;
        }

        public static void BombPlanted(EventBombPlanted _)
        {
            var plantedBomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
            if (plantedBomb == null) return;
            plantedC4 = plantedBomb;
            CreateTrigger();
        }

        private static void CreateTrigger()
        {
            var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");
            if (trigger == null || plantedC4 == null || trigger.AbsOrigin == null || plantedC4.AbsOrigin == null) return;

            trigger.Collision.SolidType = SolidType_t.SOLID_CAPSULE;
            trigger.Collision.SolidFlags = 0;
            trigger.Spawnflags = 1;
            trigger.Globalname = $"planted_bomb_prop_{trigger.Index}";
            trigger.Collision.SolidFlags = 1;

            trigger.AbsOrigin.X = plantedC4.AbsOrigin.X;
            trigger.AbsOrigin.Y = plantedC4.AbsOrigin.Y;
            trigger.AbsOrigin.Z = plantedC4.AbsOrigin.Z;

            trigger.Collision.CapsuleRadius = 10;
            trigger.Collision.BoundingRadius = 10;

            trigger.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_TRIGGER;
            trigger.Collision.EnablePhysics = 1;
            trigger.Collision.TriggerBloat = 0;

            trigger.Collision.SurroundType = SurroundingBoundsType_t.USE_OBB_COLLISION_BOUNDS;
            trigger.Collision.CollisionAttribute.CollisionFunctionMask = 39;
            trigger.Collision.CollisionAttribute.CollisionGroup = 2;

            trigger.DispatchSpawn();
            triggerC4 = trigger;
        }

        private static void RemoveBomb()
        {
            if (plantedC4 != null && plantedC4.IsValid)
                plantedC4.AcceptInput("Kill");
            if (triggerC4 != null && triggerC4.IsValid)
                triggerC4.AcceptInput("Kill");
            SkillUtils.TerminateRound(CsTeam.CounterTerrorist);
        }

        public static void OnTakeDamage(DynamicHook h)
        {
            CEntityInstance param = h.GetParam<CEntityInstance>(0);
            CTakeDamageInfo param2 = h.GetParam<CTakeDamageInfo>(1);

            if (param == null || param.Entity == null || param2 == null || param2.Attacker == null || param2.Attacker.Value == null)
                return;

            CCSPlayerPawn attackerPawn = new(param2.Attacker.Value.Handle);

            if (attackerPawn.DesignerName != "player" || param.DesignerName != "trigger_multiple")
                return;

            CTriggerMultiple trigger = new(param.Handle);

            if (attackerPawn == null || attackerPawn.Controller?.Value == null || trigger == null || !trigger.Globalname.StartsWith("planted_bomb_prop_"))
                return;

            CCSPlayerController attacker = attackerPawn.Controller.Value.As<CCSPlayerController>();

            var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
            if (playerInfo == null || playerInfo.Skill != skillName) return;

            bombHealth -= (int)param2.TotalledDamage;
            if (bombHealth <= 0)
            {
                RemoveBomb();
                return;
            }

            Localization.PrintTranslationToChatAll($" {ChatColors.Gold}{{0}}: {ChatColors.Red}{bombHealth}{ChatColors.Gold}/{ChatColors.Green}{maxBombHealth}", ["fragilebomb_bomb_health"]);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#5d00ff", CsTeam onlyTeam = CsTeam.CounterTerrorist, bool needsTeammates = false, int maxBombHealth = 1000) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int MaxBombHealth { get; set; } = maxBombHealth;
        }
    }
}