using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace FeatMoreCities
{
    [BepInPlugin("akarnokd.planbterraformmods.featmorecities", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<int> cityCount;

        static CityCounterOption cityCounterOption;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWorld_GenerationLua), nameof(SWorld_GenerationLua.GenerateCities))]
        static void SWorld_GenerationLua_GenerateCities_Post()
        {
            // initial city names
            List<string> cityNames = new(GGame.cityNames);
            cityNames.Shuffle();

            // if there are more cities to generate then names, cycle through the names again
            // but append a counter
            int counter = 2;
            while (cityNames.Count < GGame.cities.Count)
            {
                var set = new List<string>(GGame.cityNames);
                set.Shuffle();
                for (int i = 0; i < set.Count; i++)
                {
                    set[i] = set[i] + " " + counter;
                }

                cityNames.AddRange(set);
                counter++;
            }

            int j = 0;
            foreach (var city in GGame.cities)
            {
                city.name = "New " + cityNames[j];
                j++;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneNewPlanet), "OnActivate")]
        static void SSceneNewPlanet_OnActivate(SSceneNewPlanet __instance)
        {
            if (cityCounterOption == null)
            {
                var tr = __instance.inputPlanetName.gameObject.transform.parent.parent.parent.parent.parent;

                cityCounterOption = new CityCounterOption();
                cityCounterOption.InstantiateUI(tr, false);
            }
        }

        internal class CityCounterOption : COptionFloat
        {
            protected override float GetSavedValue()
            {
                return cityCount.Value;
            }

            protected override void DoSave()
            {
                
            }

            public override void Apply()
            {
                cityCount.Value = Mathf.RoundToInt(value);
            }

            public override void RefreshUI()
            {
                _slider.value = value;
                _textValue.text = Mathf.RoundToInt(value).ToString();
            }

            public override void InstantiateUI(Transform uiOptionsContainer, bool isInLauncher = false)
            {
                var v = cityCount.Value;
                base.InstantiateUI(uiOptionsContainer, isInLauncher);
                _slider.minValue = 0;
                _slider.maxValue = 20;

                var component = _slider.gameObject.transform.parent.parent.Find("Label").GetComponent<Text>();
                component.text = "Number of additional cities";
                value = v;
                _slider.value = value;
                _textValue.text = Mathf.RoundToInt(value).ToString();
            }
        }
    }
}
