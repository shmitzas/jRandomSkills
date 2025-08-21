using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Mute : ISkill
    {
        private static Skills skillName = Skills.AntyHead;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#2fc468");

            Instance.RegisterListener<Listeners.OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (!name.EndsWith("_projectile") && name != "instanced_scripted_scene")
                    return;

                if (@event.DesignerName == "instanced_scripted_scene")
                {
                    @event.AcceptInput("Kill");
                    @event.Remove();
                    return;
                }

                Server.NextFrame(() => {
                    switch (name)
                    {
                        case "smokegrenade_projectile":
                            var smoke = @event.As<CSmokeGrenadeProjectile>();
                            smoke.SmokeEffectTickBegin = int.MaxValue;
                            break;
                        case "molotov_projectile":
                            var molotov = @event.As<CMolotovProjectile>();
                            molotov.Detonated = true;
                            break;
                        case "decoy_projectile":
                            var decoy = @event.As<CDecoyProjectile>();
                            decoy.ExpireTime = int.MaxValue;
                            decoy.DecoyShotTick = int.MaxValue;
                            decoy.ShotsRemaining = int.MaxValue;
                            break;
                        default:
                            var grenade = @event.As<CBaseCSGrenadeProjectile>();
                            grenade.DetonateTime = float.MaxValue;
                            break;
                    }

                    Instance.AddTimer(5f, () =>
                    {
                        if (@event != null && @event.IsValid)
                            @event.Remove();
                    });
                });
            });
        }
    }
}