using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class HealingSmoke : ISkill
    {
        private const Skills skillName = Skills.HealingSmoke;
        private static int smokeHeal = Config.GetValue<int>(skillName, "smokeHeal");
        private static float smokeRadius = Config.GetValue<float>(skillName, "smokeRadius");
        private static List<Vector> smokes = new List<Vector>();

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
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventSmokegrenadeDetonate>((@event, info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                smokes.Add(new Vector(@event.X, @event.Y, @event.Z));
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventSmokegrenadeExpired>((@event, @info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                smokes.RemoveAll(v => v.X == @event.X && v.Y == @event.Y && v.Z == @event.Z);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (name != "smokegrenade_projectile") return;

                var grenade = @event.As<CBaseCSGrenadeProjectile>();
                var pawn = grenade.OwnerEntity.Value.As<CCSPlayerPawn>();
                var player = pawn.Controller.Value.As<CCSPlayerController>();

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return;

                Server.NextFrame(() =>
                {
                    var smoke = @event.As<CSmokeGrenadeProjectile>();
                    smoke.SmokeColor.X = 0;
                    smoke.SmokeColor.Y = 255;
                    smoke.SmokeColor.Z = 0;
                });
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (Vector smokePos in smokes)
                    foreach (var player in Utilities.GetPlayers())
                        if (Server.TickCount % 17 == 0)
                            if (SkillUtils.GetDistance(smokePos, player.PlayerPawn.Value.AbsOrigin) <= smokeRadius)
                                AddHealth(player.PlayerPawn.Value, smokeHeal);
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

            if (player.Health != player.MaxHealth)
                player.EmitSound("Healthshot.Success");

            player.Health = Math.Min(player.Health + health, player.MaxHealth);
            Utilities.SetStateChanged(player, "CBaseEntity", "m_iHealth");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int SmokeHeal { get; set; }
            public float SmokeRadius { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#1fe070", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int smokeHeal = 1, float smokeRadius = 180) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                SmokeHeal = smokeHeal;
                SmokeRadius = smokeRadius;
            }
        }
    }
}