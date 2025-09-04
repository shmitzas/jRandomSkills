using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class ToxicSmoke : ISkill
    {
        private const Skills skillName = Skills.ToxicSmoke;
        private static readonly int smokeDamage = Config.GetValue<int>(skillName, "smokeDamage");
        private static readonly float smokeRadius = Config.GetValue<float>(skillName, "smokeRadius");
        private static readonly List<Vector> smokes = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                smokes.Clear();
                return HookResult.Continue;
            });

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

            Instance.RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null || !player.IsValid) return HookResult.Continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                smokes.Add(new Vector(@event.X, @event.Y, @event.Z));
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventSmokegrenadeExpired>((@event, @info) =>
            {
                var player = @event.Userid;
                if (player == null || !player.IsValid) return HookResult.Continue;
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                smokes.RemoveAll(v => v.X == @event.X && v.Y == @event.Y && v.Z == @event.Z);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (name != "smokegrenade_projectile") return;

                var grenade = @event.As<CBaseCSGrenadeProjectile>();
                if (grenade == null || !grenade.IsValid || grenade.OwnerEntity == null || !grenade.OwnerEntity.IsValid || grenade.OwnerEntity.Value == null || !grenade.OwnerEntity.Value.IsValid) return;

                var pawn = grenade.OwnerEntity.Value.As<CCSPlayerPawn>();
                if (pawn == null || !pawn.IsValid || pawn.Controller == null || !pawn.Controller.IsValid || pawn.Controller.Value == null || !pawn.Controller.Value.IsValid) return;

                var player = pawn.Controller.Value.As<CCSPlayerController>();
                if (player == null || !player.IsValid) return;

                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return;

                Server.NextFrame(() =>
                {
                    var smoke = @event.As<CSmokeGrenadeProjectile>();
                    smoke.SmokeColor.X = 255;
                    smoke.SmokeColor.Y = 0;
                    smoke.SmokeColor.Z = 255;
                });
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (Vector smokePos in smokes)
                    foreach (var player in Utilities.GetPlayers())
                        if (Server.TickCount % 17 == 0)
                            if (player != null && player.IsValid && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
                                if (player.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE && player.PlayerPawn.Value.AbsOrigin != null)
                                    if (SkillUtils.GetDistance(smokePos, player.PlayerPawn.Value.AbsOrigin) <= smokeRadius)
                                        AddHealth(player.PlayerPawn.Value, -smokeDamage);
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, CsItem.SmokeGrenade);
        }

        private static void AddHealth(CCSPlayerPawn player, int health)
        {
            if (player.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                return;

            player.Health += health;
            Utilities.SetStateChanged(player, "CBaseEntity", "m_iHealth");

            player.EmitSound("Player.DamageBody.Onlooker");
            if (player.Health <= 0)
                player.CommitSuicide(false, true);
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#507529", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int smokeDamage = 2, float smokeRadius = 180) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int SmokeDamage { get; set; } = smokeDamage;
            public float SmokeRadius { get; set; } = smokeRadius;
        }
    }
}