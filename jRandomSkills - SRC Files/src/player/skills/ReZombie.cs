using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using System.Drawing;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class ReZombie : ISkill
    {
        private const Skills skillName = Skills.ReZombie;
        private static int zombieHealth = Config.GetValue<int>(skillName, "zombieHealth");
        private static HashSet<CCSPlayerController> zombies = new HashSet<CCSPlayerController>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventItemEquip>((@event, info) =>
            {
                var player = @event.Userid;
                var weapon = @event.Item;

                if (!zombies.Contains(player) || weapon == "c4") return HookResult.Continue;
                player.ExecuteClientCommand("slot3");
                return HookResult.Stop;
            });

            Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                foreach(var player in  zombies)
                    DisableSkill(player);
                zombies.Clear();
                return HookResult.Stop;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null || !player.IsValid || !player.PlayerPawn.Value.IsValid || zombies.Contains(player)) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                var pawn = player.PlayerPawn.Value;
                Vector deadPosition = new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z);
                QAngle deadRotation = new QAngle(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z);

                player.Respawn();
                Instance.AddTimer(.2f, () => {
                    player.Respawn();
                    zombies.Add(player);
                    player.ExecuteClientCommand("slot3");
                    SetPlayerColor(pawn, false);
                    SkillUtils.AddHealth(pawn, zombieHealth - 100, zombieHealth);
                    pawn.Teleport(deadPosition, deadRotation);
                });

                return HookResult.Continue;
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            zombies.Remove(player);
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            zombies.Remove(player);
            SetPlayerColor(player.PlayerPawn.Value, true);
        }

        private static void SetPlayerColor(CCSPlayerPawn pawn, bool normal)
        {
            var color = normal ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(255, 255, 0, 0);
            pawn.Render = color;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int ZombieHealth { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#ff5C0A", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int zombieHealth = 200) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                ZombieHealth = zombieHealth;
            }
        }
    }
}