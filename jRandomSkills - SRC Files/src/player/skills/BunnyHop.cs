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
        private static Skills skillName = Skills.BunnyHop;
        private const float MaxSpeed = 500f;
        private const float BunnyHopVelocity = 300f;

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            SkillUtils.RegisterSkill(skillName, "#EB4034");
            Instance.RegisterListener<OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);

                if (playerInfo?.Skill == skillName)
                {
                    GiveBunnyHop(player);
                }
            }
        }

        private static void GiveBunnyHop(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null)
            {
                if (Math.Round(playerPawn.AbsVelocity.Length2D()) > MaxSpeed && MaxSpeed != 0)
                    ChangeVelocity(playerPawn, MaxSpeed);

                var flags = (PlayerFlags)playerPawn.Flags;
                var buttons = player.Buttons;

                if (buttons.HasFlag(PlayerButtons.Jump) && flags.HasFlag(PlayerFlags.FL_ONGROUND) && !playerPawn.MoveType.HasFlag(MoveType_t.MOVETYPE_LADDER))
                    playerPawn.AbsVelocity.Z = BunnyHopVelocity;
            }
        }

        private static void ChangeVelocity(CCSPlayerPawn? pawn, float vel)
        {
            if (pawn == null) return;

            var currentVelocity = new Vector(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);
            var currentSpeed3D = Math.Sqrt(currentVelocity.X * currentVelocity.X + currentVelocity.Y * currentVelocity.Y + currentVelocity.Z * currentVelocity.Z);

            pawn.AbsVelocity.X = (float)(currentVelocity.X / currentSpeed3D) * vel;
            pawn.AbsVelocity.Y = (float)(currentVelocity.Y / currentSpeed3D) * vel;
            pawn.AbsVelocity.Z = (float)(currentVelocity.Z / currentSpeed3D) * vel;
        }
    }
}