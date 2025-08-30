using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class BladeMaster : ISkill
    {
        private const Skills skillName = Skills.BladeMaster;
        private static string[] noReflectionWeapon = { "inferno", "flashbang", "smokegrenade", "decoy", "hegrenade", "knife", "taser" };
        private static float torseReflectionChance = Config.GetValue<float>(skillName, "torseReflectionChance") * 100;
        private static float legReflectionChance = Config.GetValue<float>(skillName, "legReflectionChance") * 100;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var victim = @event.Userid;
                int damage = @event.DmgHealth;
                HitGroup_t hitGroup = (HitGroup_t)@event.Hitgroup;
                string weapon = @event.Weapon;

                if (noReflectionWeapon.Contains(weapon) || !Instance.IsPlayerValid(victim)) return HookResult.Continue;
                var victimInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == victim.SteamID);
                if (victimInfo == null || victimInfo.Skill != skillName) return HookResult.Continue;

                int chance = Instance.Random.Next(0, 101);
                if (hitGroup == HitGroup_t.HITGROUP_LEFTLEG || hitGroup == HitGroup_t.HITGROUP_RIGHTLEG)
                {
                    if (chance > legReflectionChance)
                        return HookResult.Continue;
                }
                else
                    if (chance > torseReflectionChance)
                    return HookResult.Continue;

                var pawn = victim.PlayerPawn.Value;
                if (pawn == null || !pawn.IsValid) return HookResult.Continue;

                var activeWeapon = pawn.WeaponServices.ActiveWeapon.Value;
                if (activeWeapon == null || !activeWeapon.IsValid || activeWeapon.DesignerName != "weapon_knife") return HookResult.Continue;

                RestoreHealth(victim, damage);
                return HookResult.Stop;
            });
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
            public float TorseReflectionChance { get; set; }
            public float LegReflectionChance { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#cc7504", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float torseReflectionChance = .95f, float legReflectionChance = .80f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                TorseReflectionChance = torseReflectionChance;
                LegReflectionChance = legReflectionChance;
            }
        }
    }
}