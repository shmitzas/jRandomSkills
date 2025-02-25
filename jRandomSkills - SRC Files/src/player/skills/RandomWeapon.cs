using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using jRandomSkills.src.player;
using jRandomSkills.src.utils;
using System.Collections.Immutable;
using static CounterStrikeSharp.API.Core.Listeners;
using static jRandomSkills.jRandomSkills;

namespace jRandomSkills
{
    public class RandomWeapon : ISkill
    {
        private static Skills skillName = Skills.RandomWeapon;
        private static float timerCooldown = (float)(Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Cooldown);
        private static readonly Dictionary<ulong, PlayerSkillInfo> SkillPlayerInfo = new Dictionary<ulong, PlayerSkillInfo>();

        private static string[] pistols = { "weapon_deagle", "weapon_revolver", "weapon_glock", "weapon_usp_silencer",
        "weapon_cz75a", "weapon_fiveseven", "weapon_p250", "weapon_tec9", "weapon_elite", "weapon_hkp2000" };

        private static string[] rifles = { "weapon_mp9", "weapon_mac10", "weapon_bizon", "weapon_mp7", "weapon_ump45", "weapon_p90",
        "weapon_mp5sd", "weapon_famas", "weapon_galilar", "weapon_m4a1", "weapon_m4a1_silencer", "weapon_ak47",
        "weapon_aug", "weapon_sg553", "weapon_ssg08", "weapon_awp", "weapon_scar20", "weapon_g3sg1",
        "weapon_nova", "weapon_xm1014", "weapon_mag7", "weapon_sawedoff", "weapon_m249", "weapon_negev" };

        public static void LoadSkill()
        {
            if (Config.config.SkillsInfo.FirstOrDefault(s => s.Name == skillName.ToString())?.Active != true)
                return;

            Utils.RegisterSkill(skillName, "#e0873a");

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
                            UpdateHUD(player, skillInfo);
                }
            });

            Instance.AddCommand("css_useSkill", "Use Skill", (player, _) =>
            {
                if (player == null) return;
                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                if (playerInfo?.Skill == skillName)
                    UseSkill(player);
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

        private static void UseSkill(CCSPlayerController player)
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
                    RemoveAndGiveWeapon(player);
                }
                else
                    skillInfo.LastClick = DateTime.Now;
            }
        }

        private static void RemoveAndGiveWeapon(CCSPlayerController player)
        {
            List<string> playerWeapons = new List<string>();
            foreach (var item in player?.PlayerPawn?.Value?.WeaponServices?.MyWeapons)
                if (!string.IsNullOrEmpty(item?.Value?.DesignerName))
                    playerWeapons.Add(item?.Value?.DesignerName);

            if (playerWeapons.Count == 0)
                return;

            List<string> weaponList = new List<string>(pistols.Concat(rifles));
            weaponList.RemoveAll(w => playerWeapons.Contains(w));

            if (weaponList.Count == 0)
                return;

            string weapon = weaponList[Instance.Random.Next(weaponList.Count)];
            bool isPistol = pistols.Contains(weapon);

            string? weaponToRemove = playerWeapons.FirstOrDefault(itemName =>
                (isPistol && pistols.Contains(itemName)) || (!isPistol && rifles.Contains(itemName)));

            if (!string.IsNullOrEmpty(weaponToRemove))
            {
                foreach (var item in player.PlayerPawn.Value.WeaponServices.MyWeapons)
                {
                    if (item != null && item.Value != null && item.Value.DesignerName == weaponToRemove)
                        item.Value.Remove();
                }
            }

            Instance.AddTimer(.1f, () =>
            {
                player.GiveNamedItem(weapon);
            });
        }

        public class PlayerSkillInfo
        {
            public ulong SteamID { get; set; }
            public bool CanUse { get; set; }
            public DateTime Cooldown { get; set; }
            public DateTime LastClick { get; set; }
        }
    }
}