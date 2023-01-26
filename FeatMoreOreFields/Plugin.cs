using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FeatMoreOreFields
{
    [BepInPlugin("akarnokd.planbterraformmods.featmoreorefields", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;

        static ConfigEntry<int> allGenerationPeriodAdd;
        static ConfigEntry<int> allMinHexesAdd;
        static ConfigEntry<int> allMaxHexesAdd;
        static ConfigEntry<int> allMineralMaxAdd;

        static ManualLogSource logger;

        static string[] minerals = new[]
        {
            "sulfur",
            "iron",
            "aluminumOre",
            "fluorite"
        };

        static Dictionary<string, MineralConfig> mineralConfigs = new();

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");

            allGenerationPeriodAdd = Config.Bind("General", "GenerationPeriodAdd", 0, "Positive value decreases field frequency, negative value increases field frequency.");
            allMinHexesAdd = Config.Bind("General", "MinHexesAdd", 0, "Add to the minimum size of generated fields.");
            allMaxHexesAdd = Config.Bind("General", "MaxHexesAdd", 0, "Add to the maximum size of generated fields.");
            allMineralMaxAdd = Config.Bind("General", "MineralMaxAdd", 0, "Add to the maximum number of minerals in a cell.");

            foreach (var mineral in minerals)
            {
                var mc = new MineralConfig();
                var nm = "Ore-" + mineral;
                mc.generationPeriodAdd = Config.Bind(nm, "GenerationPeriodAdd", 0, "Positive value decreases field frequency, negative value increases field frequency.");
                mc.minHexesAdd = Config.Bind(nm, "MinHexesAdd", 0, "Add to the minimum size of generated fields.");
                mc.maxHexesAdd = Config.Bind(nm, "MaxHexesAdd", 0, "Add to the maximum size of generated fields.");
                mc.mineralMaxAdd = Config.Bind(nm, "MineralMaxAdd", 0, "Add to the maximum number of minerals in a cell.");

                mineralConfigs.Add(mineral, mc);
            }

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWorld_GenerationLua), nameof(SWorld_GenerationLua.GenerateOreFields))]
        static void SWorld_GenerationLua_GenerateOreFields(
            CItem_GroundMineral mineral, 
            ref int generationPeriod, 
            ref int nbHexesMin, 
            ref int nbHexesMax,
            out int __state)
        {
            __state = mineral.quantityMax;

            if (!modEnabled.Value)
            {
                return;
            }
            logger.LogInfo(mineral.codeName + " = " + mineral.id);


            generationPeriod += allGenerationPeriodAdd.Value;
            nbHexesMin += allMinHexesAdd.Value;
            nbHexesMax += allMaxHexesAdd.Value;
            mineral.quantityMax += allMineralMaxAdd.Value;

            if (mineralConfigs.TryGetValue(mineral.codeName, out var mc)) {
                generationPeriod += mc.generationPeriodAdd.Value;
                nbHexesMin += mc.minHexesAdd.Value;
                nbHexesMax += mc.maxHexesAdd.Value;
                mineral.quantityMax += mc.mineralMaxAdd.Value;
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWorld_GenerationLua), nameof(SWorld_GenerationLua.GenerateOreFields))]
        static void SWorld_GenerationLua_GenerateOreFields(
            CItem_GroundMineral mineral,
            int __state)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            

            mineral.quantityMax = __state;
        }

        internal class MineralConfig
        {
            internal ConfigEntry<int> generationPeriodAdd;
            internal ConfigEntry<int> minHexesAdd;
            internal ConfigEntry<int> maxHexesAdd;
            internal ConfigEntry<int> mineralMaxAdd;
        }
    }
}
