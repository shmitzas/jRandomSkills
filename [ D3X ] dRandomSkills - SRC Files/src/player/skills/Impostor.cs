using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static dRandomSkills.dRandomSkills;

namespace dRandomSkills
{
    public static class Impostor
    {
        private static readonly List<string> CTModels = new List<string>
        {
            // SWAT
            "characters/models/ctm_swat/ctm_swat_variante.vmdl",
            "characters/models/ctm_swat/ctm_swat_variantf.vmdl",
            "characters/models/ctm_swat/ctm_swat_varianth.vmdl",
            "characters/models/ctm_swat/ctm_swat_varianti.vmdl",
            "characters/models/ctm_swat/ctm_swat_variantj.vmdl",
            "characters/models/ctm_swat/ctm_swat_variantk.vmdl",

            // ST6
            "characters/models/ctm_st6/ctm_st6_variante.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantg.vmdl",
            "characters/models/ctm_st6/ctm_st6_varianti.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantj.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantk.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantl.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantm.vmdl",
            "characters/models/ctm_st6/ctm_st6_variantn.vmdl",

            // SAS
            "characters/models/ctm_sas/ctm_sas.vmdl",
            "characters/models/ctm_sas/ctm_sas_variantf.vmdl",
            "characters/models/ctm_sas/ctm_sas_variantg.vmdl",

            // CTM_HEAVY
            "characters/models/ctm_heavy/ctm_heavy.vmdl",

            // GENDARMERIE
            "characters/models/ctm_gendarmerie/ctm_gendarmerie_varianta.vmdl",
            "characters/models/ctm_gendarmerie/ctm_gendarmerie_variantb.vmdl",
            "characters/models/ctm_gendarmerie/ctm_gendarmerie_variantc.vmdl",
            "characters/models/ctm_gendarmerie/ctm_gendarmerie_variantd.vmdl",
            "characters/models/ctm_gendarmerie/ctm_gendarmerie_variante.vmdl",

            // FBI
            "characters/models/ctm_fbi/ctm_fbi.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_varianta.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variantb.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variantc.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variantd.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variante.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variantf.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_variantg.vmdl",
            "characters/models/ctm_fbi/ctm_fbi_varianth.vmdl",

            // DIVER
            "characters/models/ctm_diver/ctm_diver_varianta.vmdl",
            "characters/models/ctm_diver/ctm_diver_variantb.vmdl",
            "characters/models/ctm_diver/ctm_diver_variantc.vmdl",
        };


        private static readonly List<string> TModels = new List<string>
        {
            // BALKAN
            "characters/models/tm_balkan/tm_balkan_variantf.vmdl",
            "characters/models/tm_balkan/tm_balkan_variantg.vmdl",
            "characters/models/tm_balkan/tm_balkan_varianth.vmdl",
            "characters/models/tm_balkan/tm_balkan_varianti.vmdl",
            "characters/models/tm_balkan/tm_balkan_variantj.vmdl",
            "characters/models/tm_balkan/tm_balkan_variantk.vmdl",
            "characters/models/tm_balkan/tm_balkan_variantl.vmdl",

            // JUMPSUIT
            "characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl",
            "characters/models/tm_jumpsuit/tm_jumpsuit_variantb.vmdl",
            "characters/models/tm_jumpsuit/tm_jumpsuit_variantc.vmdl",

            // JUNGLERAIDER
            "characters/models/tm_jungle_raider/tm_jungle_raider_varianta.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantb.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantb2.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantc.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantd.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variante.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantf.vmdl",
            "characters/models/tm_jungle_raider/tm_jungle_raider_variantf2.vmdl",

            // LEET
            "characters/models/tm_leet/tm_leet_varianta.vmdl",
            "characters/models/tm_leet/tm_leet_variantb.vmdl",
            "characters/models/tm_leet/tm_leet_variantc.vmdl",
            "characters/models/tm_leet/tm_leet_variantd.vmdl",
            "characters/models/tm_leet/tm_leet_variante.vmdl",
            "characters/models/tm_leet/tm_leet_variantf.vmdl",
            "characters/models/tm_leet/tm_leet_variantg.vmdl",
            "characters/models/tm_leet/tm_leet_varianth.vmdl",
            "characters/models/tm_leet/tm_leet_varianti.vmdl",
            "characters/models/tm_leet/tm_leet_variantj.vmdl",
            "characters/models/tm_leet/tm_leet_variantk.vmdl",

            // PHOENIX
            "characters/models/tm_phoenix/tm_phoenix.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_varianta.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_variantb.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_variantc.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_variantd.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_variantf.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_variantg.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_varianth.vmdl",
            "characters/models/tm_phoenix/tm_phoenix_varianti.vmdl",

            // PHOENIX HEAVY
            "characters/models/tm_phoenix_heavy/tm_phoenix_heavy.vmdl",

            // PROFESSIONAL
            "characters/models/tm_professional/tm_professional_varf.vmdl",
            "characters/models/tm_professional/tm_professional_varf1.vmdl",
            "characters/models/tm_professional/tm_professional_varf2.vmdl",
            "characters/models/tm_professional/tm_professional_varf3.vmdl",
            "characters/models/tm_professional/tm_professional_varf4.vmdl",
            "characters/models/tm_professional/tm_professional_varf5.vmdl",
            "characters/models/tm_professional/tm_professional_varg.vmdl",
            "characters/models/tm_professional/tm_professional_varh.vmdl",
            "characters/models/tm_professional/tm_professional_vari.vmdl",
            "characters/models/tm_professional/tm_professional_varj.vmdl",
        };


        public static void LoadImpostor()
        {
            Utils.RegisterSkill("Impostor", "Otrzymujesz na start rundy model postaci wroga", "#99140B");

            Instance.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
            {
                Instance.AddTimer(0.1f, () => 
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        var playerInfo = Instance.skillPlayer.FirstOrDefault(p => p.SteamID == player.SteamID);
                        if (playerInfo?.Skill == "Impostor")
                        {
                            string model = player.Team == CsTeam.Terrorist
                                ? GetRandomModel(CTModels)
                                : player.Team == CsTeam.CounterTerrorist
                                ? GetRandomModel(TModels)
                                : null;

                            if (model != null)
                            {
                                SetPlayerModel(player, model);
                            }
                        }
                    }
                });

                return HookResult.Continue;
            });
        }

        private static string GetRandomModel(List<string> models)
        {
            if (models == null || models.Count == 0) return null;
            var random = new Random();
            int index = random.Next(models.Count);
            return models[index];
        }

        private static void SetPlayerModel(CCSPlayerController player, string model)
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) return;

            Server.NextFrame(() =>
            {
                pawn.SetModel(model);

                var originalRender = pawn.Render;
                pawn.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
            });
        }
    }
}