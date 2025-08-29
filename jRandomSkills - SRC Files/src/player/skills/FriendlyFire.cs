using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class FriendlyFire : ISkill
    {
        private const Skills skillName = Skills.FriendlyFire;
        private static float healthMultiplier = Config.GetValue<float>(skillName, "healthMultiplier");
        private static string[] nades = { "inferno", "flashbang", "smokegrenade", "decoy", "hegrenade" };

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var damage = @event.DmgHealth;
                var victim = @event.Userid;
                var attacker = @event.Attacker;
                var weapon = @event.Weapon;
                HitGroup_t hitgroup = (HitGroup_t)@event.Hitgroup;

                if (nades.Contains(weapon)) return HookResult.Continue;
                if (!Instance.IsPlayerValid(attacker) || !Instance.IsPlayerValid(victim) || attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
                if (playerInfo?.Skill != skillName || attacker.Team != victim.Team) return HookResult.Continue;

                Server.ExecuteCommand("mp_autokick 0");

                var pawn = victim.PlayerPawn.Value;
                SkillUtils.AddHealth(pawn, damage + (int)(damage * healthMultiplier), pawn.MaxHealth);
                
                return HookResult.Continue;
            });
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float HealthMultiplier { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#ff0000", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = true, float healthMultiplier = 1.5f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                HealthMultiplier = healthMultiplier;
        }
        }
    }
}