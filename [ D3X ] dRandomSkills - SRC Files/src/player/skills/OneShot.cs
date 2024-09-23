using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class OneShot
    {
        public static void LoadOneShot()
        {
            Utils.RegisterSkill("One Shot", "Po trafieniu od razu zabija przeciwnika", "#ff5CD9");
            
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        }

        private static HookResult OnTakeDamage(DynamicHook h)
        {
            CEntityInstance param = h.GetParam<CEntityInstance>(0);
            CTakeDamageInfo param2 = h.GetParam<CTakeDamageInfo>(1);

            if (param == null || param2 == null || param2.Attacker == null)
                return HookResult.Continue;

            CCSPlayerPawn attackerPawn = new CCSPlayerPawn(param2.Attacker.Value.Handle);
            CCSPlayerPawn victimPawn = new CCSPlayerPawn(param.Handle);

            if (attackerPawn == null || attackerPawn.Controller?.Value == null || victimPawn == null || victimPawn.Controller?.Value == null)
                return HookResult.Continue;

            CCSPlayerController attacker = attackerPawn.Controller.Value.As<CCSPlayerController>();
            CCSPlayerController victim = victimPawn.Controller.Value.As<CCSPlayerController>();
            
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
            if (playerInfo == null) return HookResult.Continue;
            
            if (playerInfo.Skill == "One Shot" && attacker.PawnIsAlive)
            {
                param2.Damage = 1000f;
            }

            return HookResult.Continue;
        }
    }
}