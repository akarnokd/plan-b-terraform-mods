using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FeatAddCityNames
{
    [BepInPlugin("akarnokd.planbterraformmods.feataddcitynames", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static string[] defaultCityNames = new[]
        {
            "Budapest",
            "Vienna",
            "Bucharest",
            "Bratislava",
            "Ljubljana",
            "Prague",
            "Zagreb",
            "Belgrade",
            "Warsaw",
            "Lisbon",
            "Rome",
            "Brussels",
            "Athens",
            "Berlin",
        };

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            if (Config.Bind("General", "Enabled", true, "Is the mod enabled?").Value)
            {
                bool additive = Config.Bind("General", "Additive", true, "If true, the city names will be added to the pool. If false, only the city names will be in the pool.").Value;

                string cityNamesStr = Config.Bind("General", "Names", string.Join(",", defaultCityNames), "The comma separated list of city names. Whitespaces around commas are ignored").Value.Trim();

                if (cityNamesStr.Length != 0)
                {
                    string[] cityNamesSplit = cityNamesStr.Split(',');

                    for (int i = 0; i < cityNamesSplit.Length; i++)
                    {
                        cityNamesSplit[i] = cityNamesSplit[i].Trim();
                    }

                    if (additive)
                    {
                        var oldNames = GGame.cityNames;
                        GGame.cityNames = new string[oldNames.Length + cityNamesSplit.Length];
                        Array.Copy(oldNames, 0, GGame.cityNames, 0, oldNames.Length);
                        Array.Copy(cityNamesSplit, 0, GGame.cityNames, oldNames.Length, cityNamesSplit.Length);
                    }
                    else
                    {
                        GGame.cityNames = cityNamesSplit;
                    }
                }
            }

            // Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        
    }
}
