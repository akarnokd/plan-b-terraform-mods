using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIVSyncLimitFramerate
{
    [BepInPlugin("akarnokd.planbterraformmods.uivsynclimitframerate", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<int> frameRateDivider;

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            frameRateDivider = Config.Bind("General", "FramerateDivider", 1, "Divide the framerate by this amount when VSync is enabled");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(COptionBool_VSync), nameof(COptionBool_VSync.Apply))]
        static bool COptionBool_VSync_Apply(COptionBool_VSync __instance)
        {
            int v = (__instance.value ? frameRateDivider.Value : 0);
            logger.LogInfo("Applying VSync value of " + v);
            QualitySettings.vSyncCount = v;

            return false;
        }
    }
}
