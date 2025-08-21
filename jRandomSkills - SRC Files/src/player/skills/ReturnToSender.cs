using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class ReturnToSender : ISkill
    {
        private static Skills skillName = Skills.ReturnToSender;
        private static HashSet<nint> playersToSender = new HashSet<nint>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#a68132");

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                playersToSender.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                int damage = @event.DmgHealth;

                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim)) return HookResult.Continue;
                var attackerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                if (attackerInfo == null || attackerInfo.Skill != skillName) return HookResult.Continue;

                if (playersToSender.TryGetValue(victim.Handle, out _))
                    return HookResult.Continue;

                victim.PlayerPawn.Value.Teleport(GetSpawnVector(victim));
                playersToSender.Add(victim.Handle);
                return HookResult.Stop;
            });
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            playersToSender.Remove(player.Handle);
        }

        private static Vector GetSpawnVector(CCSPlayerController player)
        {
            var abs = player.PlayerPawn.Value.AbsOrigin;
            var spawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(player.Team == CsTeam.Terrorist ? "info_player_terrorist" : "info_player_counterterrorist").ToList();
            if (spawns.Any())
            {
                var randomSpawn = spawns[Instance.Random.Next(spawns.Count)];
                return randomSpawn.AbsOrigin;
            }
            return new Vector(abs.X, abs.Y, abs.Z);
        }
    }
}