using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Rambo : ISkill
    {
        private static Skills skillName = Skills.Rambo;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#009905");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
                    }
                });
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            int healthBonus = Instance.Random.Next(50, 501);
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
    }
}