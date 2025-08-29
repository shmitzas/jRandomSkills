using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Pyro : ISkill
    {
        private const Skills skillName = Skills.Pyro;
        private static float regenerationMultiplier = Config.GetValue<float>(skillName, "regenerationMultiplier");

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
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                int damage = @event.DmgHealth;
                string weapon = @event.Weapon;

                if (weapon != "inferno" || !Instance.IsPlayerValid(victim)) return HookResult.Continue;
                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                if (victimInfo == null || victimInfo.Skill != skillName) return HookResult.Continue;

                RestoreHealth(victim, damage * regenerationMultiplier);
                return HookResult.Stop;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, player.Team == CsTeam.CounterTerrorist ? CsItem.IncendiaryGrenade : CsItem.Molotov);
        }

        private static void RestoreHealth(CCSPlayerController victim, float damage)
        {
            var playerPawn = victim.PlayerPawn.Value;
            var newHealth = playerPawn.Health + damage;

            if (newHealth > 100)
                newHealth = 100;

            playerPawn.Health = (int)newHealth;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float RegenerationMultiplier { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#3c47de", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float regenerationMultiplier = 1.5f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                RegenerationMultiplier = regenerationMultiplier;
            }
        }
    }
}