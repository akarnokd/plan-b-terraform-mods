using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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

        static MapGenerateOption mgPeriod;
        static MapGenerateOption mgMinHexes;
        static MapGenerateOption mgMaxHexes;
        static MapGenerateOption mgMinerals;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneNewPlanet), "OnActivate")]
        static void SSceneNewPlanet_OnActivate(SSceneNewPlanet __instance)
        {
            if (mgPeriod == null)
            {
                var tr = __instance.inputPlanetName.gameObject.transform.parent.parent.parent.parent.parent;

                mgPeriod = new MapGenerateOption(allGenerationPeriodAdd, -1, 0, 100, "Mineral frequency");
                mgPeriod.InstantiateUI(tr);

                mgMinHexes = new MapGenerateOption(allMinHexesAdd, 1, 0, 100, "Mineral patch size minimum");
                mgMinHexes.InstantiateUI(tr);

                mgMaxHexes = new MapGenerateOption(allMaxHexesAdd, 1, 0, 100, "Mineral patch size maximum");
                mgMaxHexes.InstantiateUI(tr);

                mgMinerals = new MapGenerateOption(allMineralMaxAdd, 1, 0, 10000, "Mineral amount maximum");
                mgMinerals.InstantiateUI(tr);
            }
        }

        internal class MapGenerateOption : COptionFloat
        {
            readonly ConfigEntry<int> config;
            readonly float scale;
            readonly string title;
            readonly float min;
            readonly float max;

            internal MapGenerateOption(ConfigEntry<int> config, float scale, float min, float max, string title)
            {
                this.config = config;
                this.scale = scale;
                this.title = title;
                this.min = min;
                this.max = max;
            }

            protected override float GetSavedValue()
            {
                return config.Value / scale;
            }

            protected override void DoSave()
            {

            }

            public override void Apply()
            {
                config.Value = Mathf.RoundToInt(value * scale);
            }

            public override void RefreshUI()
            {
                _slider.value = value;
                _textValue.text = Mathf.RoundToInt(value).ToString();
            }

            public override void InstantiateUI(Transform uiOptionsContainer, bool isInLauncher = false)
            {
                var v = config.Value / scale;
                base.InstantiateUI(uiOptionsContainer, isInLauncher);
                _slider.minValue = min;
                _slider.maxValue = max;

                var component = _slider.gameObject.transform.parent.parent.Find("Label").GetComponent<Text>();
                component.text = title;
                value = v;
                _slider.value = value;
                _textValue.text = Mathf.RoundToInt(value).ToString();
            }
        }
    }
}
