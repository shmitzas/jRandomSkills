using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Muhammed : ISkill
    {
        private static Skills skillName = Skills.Muhammed;
        private const float ExplosionRadius = 500.0f;
        private const int ExplosionDamage = 999;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#F5CB42");
            
            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                CCSPlayerController player = @event.Userid;
                if (!IsDeadPlayerValid(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
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
            Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} qRandomSkills {ChatColors.Green}] {ChatColors.DarkRed}{player.PlayerName}: {ChatColors.Lime}ALLAHU AKBAR!!!");

            var fileNames = new[] { "radiobotfallback01", "radiobotfallback02", "radiobotfallback04" };
            Instance.AddTimer(0.1f, () =>
            {
                var randomFile = fileNames[new Random().Next(fileNames.Length)];
                player.ExecuteClientCommand($"play vo/agents/balkan/{randomFile}.vsnd");
            });
        }

        private static bool IsDeadPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}