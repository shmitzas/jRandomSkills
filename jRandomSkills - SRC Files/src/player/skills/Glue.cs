using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Glue : ISkill
    {
        private const Skills skillName = Skills.Glue;

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterListener<Listeners.OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (!name.EndsWith("_projectile"))
                    return;

                var grenade = @event.As<CBaseCSGrenadeProjectile>();
                if (grenade.OwnerEntity.Value == null || !grenade.OwnerEntity.Value.IsValid) return;

                var pawn = grenade.OwnerEntity.Value.As<CCSPlayerPawn>();
                var player = pawn.Controller.Value.As<CCSPlayerController>();

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return;
                grenade.Bounces = 555;
            });
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#fff52e", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false) : base(skill, active, color, onlyTeam, needsTeammates)
            {
            }
        }
    }
}