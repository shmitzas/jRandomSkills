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
        private static int secondLifeHealth = Config.GetValue<int>(skillName, "startHealth");
        private static HashSet<nint> secondLifePlayers = new HashSet<nint>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (player == null || !player.IsValid || player.PlayerPawn?.Value == null) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                secondLifePlayers.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                int damage = @event.DmgHealth;

                if (!Instance.IsPlayerValid(victim)) return HookResult.Continue;
                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                if (victimInfo == null || victimInfo.Skill != skillName) return HookResult.Continue;

                var victimPawn = victim.PlayerPawn.Value;
                if (victimPawn.Health > 0 || secondLifePlayers.TryGetValue(victim.Handle, out _) == true)
                    return HookResult.Continue;

                secondLifePlayers.Add(victim.Handle);
                SetHealth(victim, secondLifeHealth);
                victimPawn.Teleport(GetSpawnVector(victim), victimPawn.AbsRotation, null);
                return HookResult.Stop;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SetHealth(player, secondLifeHealth);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            secondLifePlayers.Remove(player.Handle);
        }

        private static void SetHealth(CCSPlayerController player, int health)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) return;

            pawn.Health = health;
            pawn.MaxHealth = health;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");

            pawn.ArmorValue = 0;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
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

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int StartHealth { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#d41c1c", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int startHealth = 50) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                StartHealth = startHealth;
            }
        }
    }
}