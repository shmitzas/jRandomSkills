using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class BunnyHop : ISkill
    {
        private const Skills skillName = Skills.BunnyHop;
        private static readonly float maxSpeed = Config.GetValue<float>(skillName, "maxSpeed");
        private static readonly float bunnyHopVelocity = Config.GetValue<float>(skillName, "jumpVelocity");
        private static readonly float jumpBoost = Config.GetValue<float>(skillName, "jumpBoost");

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));
            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.SkillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    GiveBunnyHop(player);
            }
        }

        private static void GiveBunnyHop(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null || !playerPawn.IsValid) return;

            var flags = (PlayerFlags)playerPawn.Flags;
            var buttons = player.Buttons;

            if (buttons.HasFlag(PlayerButtons.Jump) && flags.HasFlag(PlayerFlags.FL_ONGROUND) && !playerPawn.MoveType.HasFlag(MoveType_t.MOVETYPE_LADDER))
            {
                playerPawn.AbsVelocity.Z = bunnyHopVelocity;

                var vX = playerPawn.AbsVelocity.X;
                var vY = playerPawn.AbsVelocity.Y;
                var speed2D = Math.Sqrt(vX * vX + vY * vY);
                var scale = 1d;

                if (speed2D < maxSpeed)
                {
                    var newSpeed = Math.Min(speed2D * jumpBoost, maxSpeed);
                    scale = newSpeed / (speed2D == 0 ? 1 : speed2D);
                }
                else if (speed2D > maxSpeed)
                    scale = maxSpeed / speed2D;

                playerPawn.AbsVelocity.X = (float)(vX * scale);
                playerPawn.AbsVelocity.Y = (float)(vY * scale);
            }
        }

        public class SkillConfig(Skills skill = skillName, bool active = true, string color = "#d1430a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float maxSpeed = 500f, float jumpVelocity = 300f, float jumpBoost = 2f) : Config.DefaultSkillInfo(skill, active, color, onlyTeam, needsTeammates)
        {
            public float MaxSpeed { get; set; } = maxSpeed;
            public float JumpVelocity { get; set; } = jumpVelocity;
            public float JumpBoost { get; set; } = jumpBoost;
        }
    }
}