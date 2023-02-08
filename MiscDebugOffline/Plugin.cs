using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiscDebugOffline
{
    [BepInPlugin("akarnokd.planbterraformmods.miscdebugoffline", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSteam), "Init")]
        static void SSteam_Init(ref bool ____initiliazed, ref bool ____ownsFullGame, ref bool ____ownsDemo, ref bool ____ownsApp)
        {
            if (!____initiliazed)
            {
                ____initiliazed = true;
                ____ownsFullGame = true;
                ____ownsDemo = true;
                ____ownsApp = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSteam), nameof(SSteam.GetLangage))]
        static bool SSteam_GetLangage(bool ____initiliazed, ref GLoc.LanguageName __result)
        {
            if (!____initiliazed)
            {
                __result = GLoc.langages.Find((GLoc.LanguageName x) => x.steamAPI == "english");
                return false;
            }
            return true;
        } 
    }
}
