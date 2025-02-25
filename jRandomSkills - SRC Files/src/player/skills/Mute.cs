using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Mute : ISkill
    {
        private static Skills skillName = Skills.None;
        private const float ExplosionRadius = 500.0f;
        private const int ExplosionDamage = 999;

        public static void LoadSkill()
        {
            return;

            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#F5CB42");

            Instance.RegisterListener<Listeners.OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (!name.EndsWith("_projectile"))
                    return;

                // mp_freezetime 0; mp_warmup_end
                Server.PrintToChatAll("-> " + name);

                switch (name)
                {
                    case "flashbang_projectile":
                        var flash = @event.As<CFlashbangProjectile>();
                        break;
                    case "smokegrenade_projectile":
                        var smoke = @event.As<CSmokeGrenadeProjectile>();
                        Schema.SetSchemaValue<int>(smoke.Handle, "CSmokeGrenadeProjectile", "m_nSmokeEffectTickBegin", int.MaxValue);
                        Schema.SetSchemaValue<bool>(smoke.Handle, "CSmokeGrenadeProjectile", "m_bDidSmokeEffect", true);
                        break;
                    case "hegrenade_projectile":
                        var hegrenade = @event.As<CHEGrenadeProjectile>();
                        hegrenade.DetonateTime = 0;
                        break;
                    case "molotov_projectile":
                        var molotov = @event.As<CMolotovProjectile>();
                        molotov.StillTimer.Timestamp = 0;
                        break;
                    case "decoy_projectile":
                        var decoy = @event.As<CDecoyProjectile>();
                        break;
                }
            });
        }
    }
}