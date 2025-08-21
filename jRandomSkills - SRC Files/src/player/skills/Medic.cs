using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using jRandomSkills.src.player;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Medic : ISkill
    {
        private static Skills skillName = Skills.Medic;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#42FF5F");
            
            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) continue;
                        player.RemoveItemByDesignerName("weapon_healthshot");
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != skillName) continue;
                        EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });
            
            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!Instance.IsPlayerValid(player)) return HookResult.Continue;
                    DisableSkill(player);
                }
                
                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            int healthshot = Instance.Random.Next(1, 10);
            SkillUtils.TryGiveWeapon(player, CsItem.Healthshot, healthshot);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            player.RemoveItemByDesignerName("weapon_healthshot");
        }
    }
}