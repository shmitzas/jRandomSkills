using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class HolyHandGrenade : ISkill
    {
        private static Skills skillName = Skills.HolyHandGrenade;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#ffdd00");

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

            Instance.RegisterListener<Listeners.OnEntitySpawned>(@event =>
            {
                var name = @event.DesignerName;
                if (!name.EndsWith("hegrenade_projectile"))
                    return;

                Server.NextFrame(() =>
                {
                    var hegrenade = @event.As<CHEGrenadeProjectile>();
                    var playerPawn = hegrenade.Thrower.Value;

                    var player = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerPawn.Index == playerPawn.Index);
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill != skillName)
                        return;

                    hegrenade.Damage *= 2;
                    hegrenade.DmgRadius *= 2;
                });
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, CsItem.HEGrenade);
        }
    }
}