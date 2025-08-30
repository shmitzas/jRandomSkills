using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class WeaponsSwap : ISkill
    {
        private const Skills skillName = Skills.WeaponsSwap;
        private static float timerCooldown = Config.GetValue<float>(skillName, "cooldown");
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<ulong, PlayerSkillInfo>();

        private static string[] weapons = { "weapon_deagle", "weapon_revolver", "weapon_glock", "weapon_usp_silencer",
        "weapon_cz75a", "weapon_fiveseven", "weapon_p250", "weapon_tec9", "weapon_elite", "weapon_hkp2000",
        "weapon_mp9", "weapon_mac10", "weapon_bizon", "weapon_mp7", "weapon_ump45", "weapon_p90",
        "weapon_mp5sd", "weapon_famas", "weapon_galilar", "weapon_m4a4", "weapon_m4a1_silencer", "weapon_ak47",
        "weapon_aug", "weapon_sg553", "weapon_ssg08", "weapon_awp", "weapon_scar20", "weapon_g3sg1",
        "weapon_nova", "weapon_xm1014", "weapon_mag7", "weapon_sawedoff", "weapon_m249", "weapon_negev" };

        public static void LoadSkill()
        {
            SkillUtils.RegisterSkill(skillName, Config.GetValue<string>(skillName, "color"));

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == skillName)
                        {
                            EnableSkill(player);
                        }
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
                            if (skillInfo.LastClick.AddSeconds(4) >= DateTime.Now)
                                UpdateHUD(player, skillInfo, true);
                            else
                                UpdateHUD(player, skillInfo, false);
                }
            });
        }

        public static void EnableSkill(CCSPlayerController player)
        {
            SkillPlayerInfo[player.SteamID] = new PlayerSkillInfo
            {
                SteamID = player.SteamID,
                CanUse = true,
                Cooldown = DateTime.MinValue,
                LastClick = DateTime.MinValue,
                FindedEnemy = true,
                HaveWeapon = true,
            };
        }

        public static void DisableSkill(CCSPlayerController player)
        {
            if (SkillPlayerInfo.ContainsKey(player.SteamID))
                SkillPlayerInfo.Remove(player.SteamID);
        }

        private static void UpdateHUD(CCSPlayerController player, PlayerSkillInfo skillInfo, bool showInfo)
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
            string remainingLine = "";

            if (showInfo)
                remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("hud_info", $"<font color='#FF0000'>{cooldown}</font>")}</font> <br>"
                                : !skillInfo.FindedEnemy ? $"<font class='fontSize-m' color='#FF0000'>{Localization.GetTranslation("hud_info_no_enemy")}</font> <br>"
                                : !skillInfo.HaveWeapon ? $"<font class='fontSize-m' color='#FF0000'>{Localization.GetTranslation("weaponsswap_hud_info2")}</font> <br>"
                                : "";
            else
                remainingLine = cooldown != 0 ? $"<font class='fontSize-m' color='#FFFFFF'>{Localization.GetTranslation("hud_info", $"<font color='#FF0000'>{cooldown}</font>")}</font> <br>" : "";

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
                    CCSPlayerController enemy = GetRandomEnemy(player);
                    if (enemy == null)
                    {
                        skillInfo.FindedEnemy = false;
                        skillInfo.LastClick = DateTime.Now;
                        return;
                    }

                    string[]? playerWeapon = GetWeapons(player);
                    string[]? enemyWeapon = GetWeapons(enemy);

                    if (playerWeapon.FirstOrDefault(w => weapons.Contains(w)) == null)
                    {
                        skillInfo.FindedEnemy = true;
                        skillInfo.HaveWeapon = false;
                        skillInfo.LastClick = DateTime.Now;
                        return;
                    }

                    skillInfo.HaveWeapon = true;
                    skillInfo.FindedEnemy = true;
                    skillInfo.CanUse = false;
                    skillInfo.Cooldown = DateTime.Now;

                    player.RemoveWeapons();
                    enemy.RemoveWeapons();
                    GiveWeapons(player, enemyWeapon, playerWeapon.Contains("weapon_c4"));
                    GiveWeapons(enemy, playerWeapon, enemyWeapon.Contains("weapon_c4"));
                }
                else
                    skillInfo.LastClick = DateTime.Now;
            }
        }

        private static string[]? GetWeapons(CCSPlayerController player)
        {
            List<string> playerWeapons = new List<string>();
            foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
                if (weapon.Value != null && weapon.Value.IsValid)
                    playerWeapons.Add(SkillUtils.GetDesignerName(weapon.Value));
            return playerWeapons.Count == 0 ? null : playerWeapons.ToArray();
        }

        private static void GiveWeapons(CCSPlayerController player, string[] weapons, bool addC4)
        {
            foreach (var weapon in weapons)
                if (weapon != "weapon_c4")
                    player.GiveNamedItem(weapon);
            if (addC4)
                player.GiveNamedItem("weapon_c4");
        }

        private static CCSPlayerController GetRandomEnemy(CCSPlayerController player)
        {
            CCSPlayerController[] enemies = Utilities.GetPlayers().FindAll(e => e.Team != player.Team).ToArray();
            if (enemies.Length == 0) return null;
            return enemies[Instance.Random.Next(enemies.Length)];
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
            public DateTime LastClick { get; set; }
            public bool FindedEnemy { get; set; }
            public bool HaveWeapon { get; set; }
        }

        public class SkillConfig : Config.DefaultSkillInfo
        {
            public float Cooldown { get; set; }
            public SkillConfig(Skills skill = skillName, bool active = true, string color = "#c7e03a", CsTeam onlyTeam = CsTeam.None, bool needsTeammates = false, float cooldown = 30f) : base(skill, active, color, onlyTeam, needsTeammates)
            {
                Cooldown = cooldown;
            }
        }
    }
}