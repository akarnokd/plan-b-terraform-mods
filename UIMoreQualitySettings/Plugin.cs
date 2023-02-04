using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIMoreQualitySettings
{
    [BepInPlugin("akarnokd.planbterraformmods.uimorequalitysettings", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<ShadowQuality> shadowQuality;
        static ConfigEntry<int> qualityLevel;

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            shadowQuality = Config.Bind("General", "ShadowQuality", QualitySettings.shadows, "The global shadow quality settings");
            qualityLevel = Config.Bind("General", "QualityLevel", QualitySettings.GetQualityLevel(), "The current quality level, 0 (low) - 5 (unlimited) typically");

            logger.LogInfo("Quality level names: " + string.Join(", ", QualitySettings.names));
            // Harmony.CreateAndPatchAll(typeof(Plugin));

            ApplySettings();
        }


        static void ApplySettings()
        {
            logger.LogInfo("Shadows = " + shadowQuality.Value);
            QualitySettings.shadows = shadowQuality.Value;
            logger.LogInfo("QualityLevel = " + qualityLevel.Value);
            QualitySettings.SetQualityLevel(qualityLevel.Value, true);
        }

    }
}
