using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UICityPopulationLabel
{
    [BepInPlugin("akarnokd.planbterraformmods.uicitypopulationlabel", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;

        static ManualLogSource logger;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");

            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SScene3D_Overlay), nameof(SScene3D_Overlay.LateUpdateScene))]
        static void SScene3D_Overlay_LateUpdateScene(List<Text> ___uiLabels)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            for (int j = 0; j < GGame.cities.Count; j++)
            {
                if (j < ___uiLabels.Count) {
                    var text = ___uiLabels[j];

                    var city = GGame.cities[j];
                    text.text = text.text + "\n" + string.Format("( {0:#,##0} )", city.population);
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                }
            }
        }
    }
}