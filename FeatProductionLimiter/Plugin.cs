using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FeatProductionLimiter
{
    [BepInPlugin("akarnokd.planbterraformmods.featproductionlimiter", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;

        static readonly List<string> globalProducts =
            new List<string> {
                "roadway",
                "roadstop",
                "truck",
                "railway",
                "railwaystop",
                "train",
                "extractor",
                "iceExtractor",
                "pumpingStation",
                "depot",
                "depotMK2",
                "depotMK3",
                "factory",
                "factoryAssemblyPlant",
                "factoryAtmExtractor",
                "factoryGreenhouse",
                "factoryRecycle",
                "factoryFood",
                "landmark",
                "cityDam",
                "forest_pine",
                "forest_leavesHigh",
                "forest_leavesMultiple",
                "forest_cactus",
                "forest_savannah",
                "forest_coconut"
            };
        static Dictionary<string, ConfigEntry<int>> limits = new();

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");

            foreach (var ids in globalProducts)
            {
                limits.Add(ids, Config.Bind("General", ids, 500, "Limit the production of " + ids));
            }

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "CheckStocks2")]
        static void CItem_ContentFactory_CheckStocks2(CRecipe recipe, ref bool __result)
        {
            if (modEnabled.Value)
            {
                foreach (var outp in recipe.outputs)
                {
                    if (limits.TryGetValue(outp.item.codeName, out var entry))
                    {
                        if (outp.item.nbOwned >= entry.Value)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}
