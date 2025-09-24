using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static src.jRandomSkills;
using System.Collections.Concurrent;
using src.utils;

namespace src.player.skills
{
    public class SecondLife : ISkill
    {
        private const Skills skillName = Skills.SecondLife;
        private static readonly ConcurrentDictionary<nint, byte> secondLifePlayers = [];
        private static readonly object setLock = new();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, SkillsInfo.GetValue<string>(skillName, "color"));
        }

        public static void NewRound()
        {
            secondLifePlayers.Clear();
        }

        public static void PlayerHurt(EventPlayerHurt @event)
        {
            var victim = @event.Userid;
            int damage = @event.DmgHealth;

            if (!Instance.IsPlayerValid(victim)) return;
            var victimInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == victim?.SteamID);
            if (victimInfo == null || victimInfo.Skill != skillName) return;

            var victimPawn = victim!.PlayerPawn.Value;
            if (victimPawn!.Health > 0 || secondLifePlayers.ContainsKey(victim.Handle))
                return;

            lock (setLock)
            {
                secondLifePlayers.TryAdd(victim.Handle, 0);
                SetHealth(victim, SkillsInfo.GetValue<int>(skillName, "startHealth"));
                var spawn = GetSpawnVector(victim);
                if (spawn != null)
                    victimPawn.Teleport(spawn, victimPawn.AbsRotation, null);
            }
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetHealth(player, SkillsInfo.GetValue<int>(skillName, "startHealth"));
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            secondLifePlayers.TryRemove(player.Handle, out _);
            if (player.PlayerPawn.Value == null) return;
            SetHealth(player, Math.Min(player.PlayerPawn.Value.Health + SkillsInfo.GetValue<int>(skillName, "startHealth"), 100));
        }

        private static void SetHealth(CCSPlayerController player, int health)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) return;

            pawn.Health = health;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

            pawn.ArmorValue = 0;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
        }

        private static Vector? GetSpawnVector(CCSPlayerController player)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) return null;
            
            var abs = pawn.AbsOrigin;
            var spawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(player.Team == CsTeam.Terrorist ? "info_player_terrorist" : "info_player_counterterrorist").ToList();
            if (spawns.Count != 0)
            {
                var randomSpawn = spawns[Instance.Random.Next(spawns.Count)];
                return randomSpawn.AbsOrigin;
            }
            return abs == null ? null : new Vector(abs.X, abs.Y, abs.Z);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#d41c1c", CsTeam onlyTeam = CsTeam.None, bool disableOnFreezeTime = false, bool needsTeammates = false, int startHealth = 50) : SkillsInfo.DefaultSkillInfo(skill, active, color, onlyTeam, disableOnFreezeTime, needsTeammates)
        {
            public int StartHealth { get; set; } = startHealth;
        }
    }
}