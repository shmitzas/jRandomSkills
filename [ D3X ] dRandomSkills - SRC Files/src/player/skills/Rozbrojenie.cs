using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Rozbrojenie
    {

        public static void LoadRozbrojenie()
        {
            Utils.RegisterSkill("Rozbrojenie", "Masz losow¹ szanse na wyrzucenie broni wroga po trafieniu", "#FF4500", false);

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () =>
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        if (!IsPlayerValid(player)) continue;

                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill != "Rozbrojenie") continue;

                        float newChance = (float)Instance.Random.NextDouble() * (.40f - .20f) + .20f;
                        playerInfo.SkillChance = newChance;
                        newChance = (float)Math.Round(newChance, 2) * 100;
                        newChance = (float)Math.Round(newChance);

                        Utils.PrintToChat(player, $"{ChatColors.DarkRed}\"Rozbrojenie\"{ChatColors.Lime}: Twoje szanse na wyrzucenie broni wroga to: {newChance}%", false);
                    }
                });

                return HookResult.Continue;
            });

            Instance.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;

                if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;

                if (attacker == victim) return HookResult.Continue;

                var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);

                if (playerInfo?.Skill == "Rozbrojenie" && victim.PawnIsAlive)
                {
                    if (Instance.Random.NextDouble() <= playerInfo.SkillChance)
                    {
                        var weaponServices = victim.PlayerPawn?.Value?.WeaponServices;
                        if (weaponServices?.ActiveWeapon == null) return HookResult.Continue;

                        var weaponName = weaponServices?.ActiveWeapon?.Value?.DesignerName;
                        if (weaponName != null && !weaponName.Contains("weapon_knife") && !weaponName.Contains("weapon_c4"))
                            victim.DropActiveWeapon();
                    }
                }
                return HookResult.Continue;
            });
        }
        private static bool IsPlayerValid(CCSPlayerController player)
        {
            return player != null && player.IsValid && player.PlayerPawn?.Value != null;
        }
    }
}