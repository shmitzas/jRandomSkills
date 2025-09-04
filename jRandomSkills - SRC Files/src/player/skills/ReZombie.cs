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
        private static readonly int zombieHealth = Config.GetValue<int>(skillName, "zombieHealth");
        private static readonly HashSet<CCSPlayerController> zombies = [];

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventItemEquip>((@event, info) =>
            {
                var player = @event.Userid;
                var weapon = @event.Item;
                if (player == null || !player.IsValid) return HookResult.Continue;
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
                if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid || zombies.Contains(player)) return HookResult.Continue;

                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill != skillName) return HookResult.Continue;

                var pawn = player.PlayerPawn.Value;
                if (pawn.AbsOrigin == null) return HookResult.Continue;
                Vector deadPosition = new(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z);
                QAngle deadRotation = new(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z);

                player.Respawn();
                Instance.AddTimer(.2f, () => {
                    player.Respawn();
                    zombies.Add(player);
                    SetPlayerColor(pawn, false);
                    SkillUtils.AddHealth(pawn, zombieHealth - 100, zombieHealth);
                    pawn.Teleport(deadPosition, deadRotation);
                    player.ExecuteClientCommand("slot3");
                    Instance.AddTimer(1, () => player.ExecuteClientCommand("slot3"));
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
            if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return;
            zombies.Remove(player);
            SetPlayerColor(player.PlayerPawn.Value, true);
        }

        private static void SetPlayerColor(CCSPlayerPawn pawn, bool normal)
        {
            var color = normal ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(255, 255, 0, 0);
            pawn.Render = color;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#ff5C0A", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int zombieHealth = 200) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public int ZombieHealth { get; set; } = zombieHealth;
        }
    }
}