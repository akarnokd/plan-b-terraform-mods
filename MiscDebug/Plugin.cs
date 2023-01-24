using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiscDebug
{
    [BepInPlugin("akarnokd.planbterraformmods.miscdebug", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            // Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load(Dictionary<string, CSentence> ____dicoLoc)
        {
            logger.LogInfo("Localization dump");
            foreach (var e in ____dicoLoc)
            {
                logger.LogInfo("   " + e.Key + " - " + e.Value.translation);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "ProcessStocks")]
        static void CItem_ContentFactory_ProcessStocks(CRecipe recipe)
        {
            logger.LogInfo("Recipe " + recipe.codeName);
            foreach (var outp in recipe.outputs)
            {
                logger.LogInfo("  " + outp.item.codeName);
            }
        }
    }
}
