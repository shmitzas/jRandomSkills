using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class SecondLife : ISkill
    {
        private const Skills skillName = Skills.SecondLife;
        private static readonly int secondLifeHealth = Config.GetValue<int>(skillName, "startHealth");
        private static readonly HashSet<nint> secondLifePlayers = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
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
            if (victimPawn!.Health > 0 || secondLifePlayers.TryGetValue(victim.Handle, out _) == true)
                return;

            secondLifePlayers.Add(victim.Handle);
            SetHealth(victim, secondLifeHealth);
            var spawn = GetSpawnVector(victim);
            if (spawn != null)
                victimPawn.Teleport(spawn, victimPawn.AbsRotation, null);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetHealth(player, secondLifeHealth);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            secondLifePlayers.Remove(player.Handle);
            if (player.PlayerPawn.Value == null) return;
            SetHealth(player, Math.Min(player.PlayerPawn.Value.Health + secondLifeHealth, 100));
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

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#d41c1c", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int startHealth = 50) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int StartHealth { get; set; } = startHealth;
        }
    }
}