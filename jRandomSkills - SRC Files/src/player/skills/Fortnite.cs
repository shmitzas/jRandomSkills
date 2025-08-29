using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class Fortnite : ISkill
    {
        private const Skills skillName = Skills.Fortnite;
        private static float timerCooldown = Config.GetValue<float>(skillName, "Cooldown");
        private const string propModel = "models/props/de_aztec/hr_aztec/aztec_scaffolding/aztec_scaffold_wall_support_128.vmdl";
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<ulong, PlayerSkillInfo>();

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!Instance.IsPlayerValid(player)) return;
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                            EnableSkill(player);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventRoundEnd>((@event, info) =>
            {
                SkillPlayerInfo.Clear();
                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
            {
                var player = @event.Userid;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    if (SkillPlayerInfo.ContainsKey(player.SteamID))
                        SkillPlayerInfo.Remove(player.SteamID);
                return HookResult.Continue;
            });

            Instance.RegisterListener<OnTick>(() =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                    if (playerInfo?.Skill == skillName)
                        if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
                            UpdateHUD(player, skillInfo);
                }
            });

            Instance.RegisterListener<OnServerPrecacheResources>((ResourceManifest manifest) =>
            {
                manifest.AddResource(propModel);
            });

            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo[player.SteamID] = new PlayerSkillInfo
            {
                SteamID = player.SteamID,
                CanUse = true,
                Cooldown = DateTime.MinValue,
                LastClick = DateTime.MinValue,
            };
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (SkillPlayerInfo.ContainsKey(player.SteamID))
                SkillPlayerInfo.Remove(player.SteamID);
        }

        private static void UpdateHUD(CCSPlayerController player, PlayerSkillInfo skillInfo)
        {
            float cooldown = 0;
            if (skillInfo != null)
            {
                float time = (int)(skillInfo.Cooldown.AddSeconds(timerCooldown) - DateTime.Now).TotalSeconds;
                cooldown = Math.Max(time, 0);

                if (cooldown == 0 && skillInfo?.CanUse == false)
                    skillInfo.CanUse = true;
            }

            var skillData = SkillData.Skills.FirstOrDefault(s => s.Skill == skillName);
            if (skillData == null) return;

            string infoLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='#FFFFFF'>{Localization.GetTranslation("your_skill")}:</font> <br>";
            string skillLine = $"<font class='fontSize-l' class='fontWeight-Bold' color='{skillData.Color}'>{skillData.Name}</font> <br>";
            string remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("hud_info", $"<font color='#FF0000'>{cooldown}</font>")}</font> <br>" : "";

            var hudContent = infoLine + skillLine + remainingLine;
            player.PrintToCenterHtml(hudContent);
        }

        public static void UseSkill(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.CBodyComponent == null) return;

            if (SkillPlayerInfo.TryGetValue(player.SteamID, out var skillInfo))
            {
                if (!player.IsValid || !player.PawnIsAlive) return;
                if (skillInfo.CanUse)
                {
                    skillInfo.CanUse = false;
                    skillInfo.Cooldown = DateTime.Now;
                    CreateBox(player);
                }
                else
                    skillInfo.LastClick = DateTime.Now;
            }
        }

        private static void CreateBox(CCSPlayerController player)
        {
            var playerPawn = player.PlayerPawn.Value;
            var box = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override");
            if (box == null) return;

            float distance = 50;
            Vector pos = playerPawn.AbsOrigin + SkillUtils.GetForwardVector(playerPawn.AbsRotation) * distance;
            QAngle angle = new QAngle(playerPawn.AbsRotation.X, playerPawn.AbsRotation.Y + 90, playerPawn.AbsRotation.Z);

            box.Entity.Name = box.Globalname = $"FortniteWall_{Server.TickCount}";
            box.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            box.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(box.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
            box.DispatchSpawn();
            Server.NextFrame(() =>
            {
                box.SetModel(propModel);
                box.Teleport(pos, angle, null);
            });
        }

        private static HookResult OnTakeDamage(DynamicHook h)
        {
            CEntityInstance param = h.GetParam<CEntityInstance>(0);
            CTakeDamageInfo param2 = h.GetParam<CTakeDamageInfo>(1);

            if (param == null || param.Entity == null || param2 == null || param2.Attacker == null || param2.Attacker.Value == null)
                return HookResult.Continue;

            if (string.IsNullOrEmpty(param.Entity.Name)) return HookResult.Continue;
            if (!param.Entity.Name.StartsWith("FortniteWall")) return HookResult.Continue;

            var box = param.As<CDynamicProp>();
            if (box == null || !box.IsValid) return HookResult.Continue;
            box.EmitSound("Wood_Plank.BulletImpact", volume: 1f);
            box.Remove();
            return HookResult.Continue;
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
            public DateTime LastClick { get; set; }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float Cooldown { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#1b04cc", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 2f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                Cooldown = cooldown;
            }
        }
    }
}