using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class ZamianaMiejsc
    {
        private static float timerCooldown = 30f;
        private static readonly Dictionary<ulong, ZamianaMiejsc_PlayerInfo> SkillPlayerInfo = new Dictionary<ulong, ZamianaMiejsc_PlayerInfo>();
        public static void LoadZamianaMiejsc()
        {
            Utils.RegisterSkill("ZamianaMiejsc", "Zamiana miejscami z losowym wrogiem. Kliknij [USE - E], cooldown 30s!", "#1466F5");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.RegisterListener<OnTick>(CheckHandle);
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == "ZamianaMiejsc")
                        {
                            SkillPlayerInfo[player.SteamID] = new ZamianaMiejsc_PlayerInfo
                            {
                                SteamID = player.SteamID,
                                CanUse = true,
                                Cooldown = DateTime.MinValue
                            };
                        }
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                Instance.RemoveListener<OnTick>(CheckHandle);
                SkillPlayerInfo.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == "ZamianaMiejsc")
                    if (SkillPlayerInfo.ContainsKey(player.SteamID))
                        SkillPlayerInfo.Remove(player.SteamID);

                return HookResult.Continue;
            });
        }

        private static void CheckHandle()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == "ZamianaMiejsc")
                {
                    Handle(player);
                }
            }
        }

        private static void Handle(CCSPlayerController player)
        {
            var buttons = player.Buttons;
            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                if (buttons.HasFlag(PlayerButtons.Use))
                {
                    if (skillInfo.CanUse)
                        UseSkill(player, skillInfo);
                    else
                        UpdateHUD(player, skillInfo);
                }
            }
        }

        private static void UpdateHUD(CCSPlayerController player, ZamianaMiejsc_PlayerInfo skillInfo = null)
        {
            float cooldown = 0;
            if (skillInfo != null)
            {
                float time = (float)((skillInfo.Cooldown.Second + timerCooldown)  - DateTime.Now.Second);
                cooldown = time > 0 ? time : 0;
            }

            string infoLine = $"<font color='#FFFFFF'>ZamianaMiejsc:</font> <br>";
            string remainingLine = skillInfo == null ? "<font color='#FF0000'>Nie znaleziono przeciwnika</font> <br>" : $"<font color='#FFFFFF'>Musisz poczekać jeszcze <font color='#FF0000'>{cooldown}</font> sekund</font> <br>";

            var hudContent = infoLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        private static void ActiveUse(CCSPlayerController player)
        {
            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                skillInfo.CanUse = true;
            }
        }

        private static void UseSkill(CCSPlayerController player, ZamianaMiejsc_PlayerInfo skillInfo)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;

            List<CCSPlayerController> enemy = Utilities.GetPlayers().FindAll(p => IsPlayerValid(p) && p.Team != player.Team && p.PawnIsAlive);
            if (enemy.Count == 0)
            {
                UpdateHUD(player);
                return;
            }

            CCSPlayerController randomEnemy = enemy[Instance.Random.Next(0, enemy.Count)];
            if (player.IsValid && player.PawnIsAlive && randomEnemy.IsValid && randomEnemy.PawnIsAlive)
            {
                skillInfo.CanUse = false;
                skillInfo.Cooldown = DateTime.Now;
                Instance.AddTimer(timerCooldown, () => ActiveUse(player));
                TeleportPlayers(player, randomEnemy);
            }
            else
                UpdateHUD(player);
        }

        private static void TeleportPlayers(CCSPlayerController attacker, CCSPlayerController victim)
        {
            var attackerPawn = attacker.PlayerPawn.Value;
            var victimPawn = victim.PlayerPawn.Value;

            Vector attackerPosition = new Vector(attackerPawn.AbsOrigin.X, attackerPawn.AbsOrigin.Y, attackerPawn.AbsOrigin.Z);
            QAngle attackerAngles = new QAngle(attackerPawn.AbsRotation.X, attackerPawn.AbsRotation.Y, attackerPawn.AbsRotation.Z);
            Vector attackerVelocity = new Vector(attackerPawn.AbsVelocity.X, attackerPawn.AbsVelocity.Y, attackerPawn.AbsVelocity.Z);

            Vector victimPosition = new Vector(victimPawn.AbsOrigin.X, victimPawn.AbsOrigin.Y, victimPawn.AbsOrigin.Z);
            QAngle victimAngles = new QAngle(victimPawn.AbsRotation.X, victimPawn.AbsRotation.Y, victimPawn.AbsRotation.Z);
            Vector victimVelocity = new Vector(victimPawn.AbsVelocity.X, victimPawn.AbsVelocity.Y, victimPawn.AbsVelocity.Z);

            victimPawn.Teleport(attackerPosition, attackerAngles, attackerVelocity);
            attackerPawn.Teleport(victimPosition, victimAngles, victimVelocity);
        }

        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }

        public class ZamianaMiejsc_PlayerInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
        }
    }
}