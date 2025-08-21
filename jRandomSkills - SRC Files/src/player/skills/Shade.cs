using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Shade : ISkill
    {
        private static Skills skillName = Skills.Shade;
        private const float TeleportDistance = 100.0f;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#18171A");
            
            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim)) return HookResult.Continue;

                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (attackerInfo?.Skill == skillName)
                    TeleportAttackerBehindVictim(attacker, victim);

                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                var skillInfo = Instance.skillPlayer.FirstOrDefault(p => p.Skill == skillName);
                if (skillInfo == null) return;
                var player = Utilities.GetPlayerFromSteamId(skillInfo.SteamID);
            });
        }

        private static void TeleportAttackerBehindVictim(CCSPlayerController attacker, CCSPlayerController victim)
        {
            var victimPawn = victim.PlayerPawn.Value;
            var attackerPawn = attacker.PlayerPawn.Value;

            if (victimPawn == null || attackerPawn == null) return;

            Vector victimPosition = victimPawn.AbsOrigin;
            QAngle victimAngles = victimPawn.AbsRotation;

            Vector behindPosition = victimPosition - SkillUtils.GetForwardVector(victimAngles) * TeleportDistance;
            attackerPawn.Teleport(behindPosition, victimAngles, new Vector(0, 0, 0));
        }
    }
}
