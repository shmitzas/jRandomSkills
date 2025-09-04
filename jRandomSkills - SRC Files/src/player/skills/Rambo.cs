using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Rambo : ISkill
    {
        private const Skills skillName = Skills.Rambo;
        private static int minExtraHealth = Config.GetValue<int>(skillName, "minExtraHealth");
        private static int maxExtraHealth = Config.GetValue<int>(skillName, "maxExtraHealth");

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
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
                    }
                });
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            int healthBonus = Instance.Random.Next(minExtraHealth, maxExtraHealth);
            AddHealth(player, healthBonus);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            ResetHealth(player);
        }

        public static void AddHealth(CCSPlayerController player, int health)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            pawn.MaxHealth = Math.Min(pawn.Health + health, 1000);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");

            pawn.Health = pawn.MaxHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        public static void ResetHealth(CCSPlayerController player)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            pawn.MaxHealth = 100;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");

            pawn.Health = Math.Min(pawn.Health, 100);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int MinExtraHealth { get; set; }
            public int MaxExtraHealth { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#009905", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int minExtraHealth = 50, int maxExtraHealth = 501) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MinExtraHealth = minExtraHealth;
                MaxExtraHealth = maxExtraHealth;
            }
        }
    }
}