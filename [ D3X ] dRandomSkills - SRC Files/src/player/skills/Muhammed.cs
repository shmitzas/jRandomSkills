using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Muhammed
    {
        private const float ExplosionRadius = 500.0f;
        private const int ExplosionDamage = 999;

        public static void LoadMuhammed()
        {
            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                CCSPlayerController player = @event.Userid;

                if (!IsPlayerValid(player)) return HookResult.Continue;
                
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != "Muhammed") return HookResult.Continue;

                SpawnExplosion(player);

                return HookResult.Continue;
            });
        }

        private static void SpawnExplosion(CCSPlayerController player)
        {
            var heProjectile = Utilities.CreateEntityByName<CHEGrenadeProjectile>("hegrenade_projectile");
            if (heProjectile == null || !heProjectile.IsValid) return;

            Vector pos = player.PlayerPawn.Value.AbsOrigin;
            pos.Z += 10;

            heProjectile.TicksAtZeroVelocity = 100;
            heProjectile.TeamNum = player.TeamNum;
            heProjectile.Damage = ExplosionDamage;
            heProjectile.DmgRadius = (int)ExplosionRadius;
            heProjectile.Teleport(pos, null, new Vector(0, 0, -10));
            heProjectile.DispatchSpawn();
            heProjectile.AcceptInput("InitializeSpawnFromWorld", player.PlayerPawn.Value, player.PlayerPawn.Value, "");
            heProjectile.DetonateTime = 0;

            Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} SUPERMOCE {ChatColors.Green}] {ChatColors.DarkRed}{player.PlayerName}: {ChatColors.Lime}ALLAHU AKBAR!!!");

            var fileNames = new[] { "radiobotfallback01", "radiobotfallback02", "radiobotfallback04" };
            Instance.AddTimer(0.1f, () =>
            {
                var randomFile = fileNames[new Random().Next(fileNames.Length)];
                player.ExecuteClientCommand($"play vo/agents/balkan/{randomFile}.vsnd");
            });
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}