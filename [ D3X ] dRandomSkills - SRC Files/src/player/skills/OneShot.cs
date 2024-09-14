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
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        }

        private static HookResult OnTakeDamage(DynamicHook h)
        {
            CTakeDamageInfo param = h.GetParam<CTakeDamageInfo>(1);

            if (param == null || param?.Attacker == null || !param.Attacker.Value.IsValid) return HookResult.Continue;

            CCSPlayerPawn attackerPawn = new CCSPlayerPawn(param.Attacker.Value.Handle);
            if (attackerPawn == null || attackerPawn.Controller == null || !attackerPawn.Controller.Value.IsValid) return HookResult.Continue;

            CCSPlayerController attacker = attackerPawn.Controller.Value.As<CCSPlayerController>();
            if (attacker == null) return HookResult.Continue;
            
            var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == attacker.SteamID);
            if (playerInfo == null) return HookResult.Continue;
            
            if (playerInfo.Skill == "One Shot" && attacker.PawnIsAlive)
            {
                param.Damage = 1000f;
            }

            return HookResult.Continue;
        }
    }
}