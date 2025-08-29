using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using System.Drawing;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Jackal : ISkill
    {
        private const Skills skillName = Skills.Jackal;
        private static int maxStepBeam = Config.GetValue<int>(skillName, "maxStepBeam");
        private static bool exists = false;

        private static Dictionary<uint, uint> authorBeams = new Dictionary<uint, uint>();
        private static Dictionary<CCSPlayerController, List<CBeam>> stepBeams = new Dictionary<CCSPlayerController, List<CBeam>>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"), false);

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

                    if (exists)
                        Instance.RegisterListener<Listeners.CheckTransmit>(CheckTransmit);
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                foreach (var beams in stepBeams.Values)
                    foreach (var beam in beams)
                        if (beam != null && beam.IsValid)
                            beam.Remove();
                foreach (var player in stepBeams.Keys)
                    DisableSkill(player);
                authorBeams.Clear();
                stepBeams.Clear();

                if (exists)
                    Instance.RemoveListener<Listeners.CheckTransmit>(CheckTransmit);
                exists = false;
                return HookResult.Continue;
            });

            Instance.RegisterListener<Listeners.OnTick>(OnTick);
        }

        private static void OnTick()
        {
            foreach (var step in stepBeams)
            {
                if (Server.TickCount % 8 != 0) continue;

                var pawn = step.Key.PlayerPawn.Value;
                var beams = step.Value;
                if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                Vector lastBeamVector = beams.LastOrDefault()?.EndPos ?? pawn.AbsOrigin;

                var newBeam = CreateBeamStep(step.Key.Team, lastBeamVector, pawn.AbsOrigin);
                if (newBeam != null)
                    beams.Add(newBeam);

                if (beams.Count >= maxStepBeam)
                {
                    beams[0].Remove();
                    beams.RemoveAt(0);
                }
            }
        }

        public static void CheckTransmit([CastFrom(typeof(nint))] CCheckTransmitInfoList infoList)
        {
            foreach(var (info, player) in infoList)
            {
                if (player == null) continue;
                foreach (var step in stepBeams)
                {
                    var enemy = step.Key;
                    var beams = step.Value;
                    foreach (var beam in beams)
                        if (!authorBeams.TryGetValue(player.Index, out uint enemyIndex) || enemyIndex != enemy.Index)
                            info.TransmitEntities.Remove(beam);
                }
            }
        }

        public static CBeam CreateBeamStep(CsTeam team, Vector start, Vector stop)
        {
            CBeam beam = Utilities.CreateEntityByName<CBeam>("beam")!;
            if (beam == null) return null;

            beam.Render = team == CsTeam.Terrorist ? Color.FromArgb(100, 255, 165, 0) : Color.FromArgb(100, 173, 216, 230);
            beam.Width = 2.0f;
            beam.EndWidth = 2.0f;
            beam.Teleport(start);

            beam.EndPos.X = stop.X;
            beam.EndPos.Y = stop.Y;
            beam.EndPos.Z = stop.Z;

            beam.DispatchSpawn();
            beam.AcceptInput("FollowEntity", beam, null!, "");
            return beam;
        }

        public static void TypeSkill(CCSPlayerController player, string[] commands)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) return;
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            if (playerInfo?.Skill != skillName) return;

            if (playerInfo.SkillChance == 1)
            {
                player.PrintToChat($" {ChatColors.Red}{Localization.GetTranslation("areareaper_used_info")}");
                return;
            }

            string enemyId = commands[0];
            var enemy = Utilities.GetPlayers().FirstOrDefault(p => p.Team != player.Team && p.Index.ToString() == enemyId);

            if (enemy == null)
            {
                player.PrintToChat($" {ChatColors.Red}" + Localization.GetTranslation("selectplayerskill_incorrect_enemy_index"));
                return;
            }

            authorBeams.Add(player.Index, enemy.Index);
            stepBeams.Add(enemy, new List<CBeam>());
            playerInfo.SkillChance = 1;
            player.PrintToChat($" {ChatColors.Green}" + Localization.GetTranslation("jackal_player_info", enemy.PlayerName));
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            exists = true;
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
            playerInfo.SkillChance = 0;

            SkillUtils.PrintToChat(player, Localization.GetTranslation("jackal") + ":", false);

            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("jackal_select_info")}");
            var enemies = Utilities.GetPlayers().Where(p => p.Team != player.Team && p.IsValid && !p.IsBot).ToArray();
            if (enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                    player.PrintToChat($" {ChatColors.Green}⠀⠀⠀[{ChatColors.Red}{enemy.Index}{ChatColors.Green}] {enemy.PlayerName}");
            }
            else
                player.PrintToChat($" {ChatColors.Red}⠀⠀⠀{Localization.GetTranslation("selectplayerskill_incorrect_enemy_index")}");
            player.PrintToChat($" {ChatColors.Green}{Localization.GetTranslation("selectplayerskill_command")} {ChatColors.Red}index");
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            authorBeams.Remove(player.Index);
            stepBeams.Remove(player);
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public int MaxStepBeam { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#f542ef", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, int maxStepBeam = 50) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                MaxStepBeam = maxStepBeam;
            }
        }
    }
}