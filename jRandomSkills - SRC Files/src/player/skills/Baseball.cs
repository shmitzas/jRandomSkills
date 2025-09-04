using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Baseball : ISkill
    {
        private const Skills skillName = Skills.Baseball;
        private static readonly float speedMultipier = Config.GetValue<float>(skillName, "speedMultipier");
        private static readonly float maxSpeed = Config.GetValue<float>(skillName, "maxSpeed");
        private static readonly int damageDeal = Config.GetValue<int>(skillName, "damageDeal");
        private static readonly HashSet<CDecoyProjectile> decoys = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                var attacker = @event.Attacker;
                var weapon = @event.Weapon;

                if (weapon != "decoy") return HookResult.Continue;
                if (!Instance.IsPlayerValid(victim) || !Instance.IsPlayerValid(attacker)) return HookResult.Continue;

                var attackerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == attacker?.SteamID);
                if (attackerInfo?.Skill != skillName) return HookResult.Continue;

                SkillUtils.TakeHealth(victim!.PlayerPawn.Value, damageDeal);
                return HookResult.Continue;
            });

            Instance.RegisterListener<Listeners.OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (name != "decoy_projectile")
                    return;

                var decoy = @event.As<CDecoyProjectile>();
                if (decoy == null || !decoy.IsValid || decoy.OwnerEntity == null || decoy.OwnerEntity.Value == null || !decoy.OwnerEntity.Value.IsValid) return;
                
                var pawn = decoy.OwnerEntity.Value.As<CCSPlayerPawn>();
                if (pawn == null || !pawn.IsValid || pawn.Controller == null || pawn.Controller.Value == null || !pawn.Controller.Value.IsValid) return;
                
                var player = pawn.Controller.Value.As<CCSPlayerController>();
                if (player == null || !player.IsValid) return;

                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return;
                decoys.Add(decoy);
            });

            Instance.RegisterEventHandler<EventDecoyStarted>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null || !player.IsValid) return HookResult.Continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                var decoy = decoys.FirstOrDefault(d => d.Index == @event.Entityid);
                if (decoy != null && decoy.IsValid)
                    decoy.Remove();

                return HookResult.Continue;
            });

            Instance.RegisterListener<Listeners.OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var decoy in decoys)
            {
                if (!decoy.IsValid)
                {
                    decoys.Remove(decoy);
                    continue;
                }
                decoy.Bounces = 0;
                if (Server.TickCount % 8 != 0) continue;
                var vel = decoy.AbsVelocity;
                float speed = vel.Length();
                float targetSpeed = Math.Min(speed * speedMultipier, maxSpeed);

                if (speed > .01f)
                {
                    var dir = vel / speed;
                    var newVelocity = dir * targetSpeed;

                    decoy.AbsVelocity.X = newVelocity.X;
                    decoy.AbsVelocity.Y = newVelocity.Y;
                    decoy.AbsVelocity.Z = newVelocity.Z;
                }
            }
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, CsItem.DecoyGrenade);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#2effc7", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float speedMultipier = 2f, float maxSpeed = 900f, int damageDeal = 9999) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float SpeedMultipier { get; set; } = speedMultipier;
            public float MaxSpeed { get; set; } = maxSpeed;
            public float DamageDeal { get; set; } = damageDeal;
        }
    }
}