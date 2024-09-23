using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class PawelJumper
    {
        private static readonly PlayerFlags[] LF = new PlayerFlags[64];
        private static readonly int?[] J = new int?[64];
        private static readonly PlayerButtons[] LB = new PlayerButtons[64];

        public static void LoadPawelJumper()
        {
            Utils.RegisterSkill("Paweł Jumper", "Otrzymujesz dodatkowy Skok", "#FFA500");
            
            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!IsPlayerValid(player)) return;
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == "Paweł Jumper")
                    {
                        GiveAdditionalJump(player);
                    }
                }
            });
        }

        private static void GiveAdditionalJump(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            var flags = (PlayerFlags)playerPawn.Flags;
            var buttons = player.Buttons;

            if ((LF[player.Slot] & PlayerFlags.FL_ONGROUND) != 0 && (flags & PlayerFlags.FL_ONGROUND) == 0 && (LB[player.Slot] & PlayerButtons.Jump) == 0 && (buttons & PlayerButtons.Jump) != 0)
            {
                //J[player.Slot] ++;
            }
            else if ((flags & PlayerFlags.FL_ONGROUND) != 0)
            {
                J[player.Slot] = 0;
            }
            else if ((LB[player.Slot] & PlayerButtons.Jump) == 0 && (buttons & PlayerButtons.Jump) != 0 && J[player.Slot] < 1)
            {
                J[player.Slot] ++;
                playerPawn.AbsVelocity.Z = 300;
            }

            LF[player.Slot] = flags;
            LB[player.Slot] = buttons;
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}