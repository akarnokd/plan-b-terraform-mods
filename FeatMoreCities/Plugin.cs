using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FeatMoreCities
{
    [BepInPlugin("akarnokd.planbterraformmods.featmorecities", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<int> cityCount;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            cityCount = Config.Bind("General", "CityCountAdd", 0, "How many more cities to generate for a new game");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWorld_GenerationLua), nameof(SWorld_GenerationLua.GenerateCities))]
        static void SWorld_GenerationLua_GenerateCities(ref int nb)
        {
            nb += cityCount.Value;
        }
    }
}
