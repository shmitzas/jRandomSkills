using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class FrozenDecoy : ISkill
    {
        private const Skills skillName = Skills.FrozenDecoy;
        private static float decoyRadius = Config.GetValue<float>(skillName, "triggerRadius");
        private static int slownessMultiplier = Config.GetValue<int>(skillName, "slownessMultiplier");
        private static List<Vector> decoys = new List<Vector>();

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                decoys.Clear();
                return HookResult.Continue;
            });

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

            Instance.RegisterEventHandler<EventDecoyStarted>((@event, info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                decoys.Add(new Vector(@event.X, @event.Y, @event.Z));
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventDecoyDetonate>((@event, @info) =>
            {
                var player = @event.Userid;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;
                decoys.RemoveAll(v => v.X == @event.X && v.Y == @event.Y && v.Z == @event.Z);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (Vector decoyPos in decoys)
                    foreach (var player in Utilities.GetPlayers())
                    {
                        double distance = SkillUtils.GetDistance(decoyPos, player.PlayerPawn.Value.AbsOrigin);
                        if (distance <= decoyRadius)
                        {
                            double modifier = Math.Clamp(distance / decoyRadius, 0f, 1f);
                            player.PlayerPawn.Value.VelocityModifier = (float)Math.Pow(modifier, slownessMultiplier);
                        }
                    }
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillUtils.TryGiveWeapon(player, CsItem.DecoyGrenade);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float TriggerRadius { get; set; }
            public int SlownessMultiplier { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#00eaff", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float triggerRadius = 180, int slownessMultiplier = 5) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                TriggerRadius = triggerRadius;
                SlownessMultiplier = slownessMultiplier;
            }
        }
    }
}