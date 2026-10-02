using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;

namespace CheatEndlessResources
{
    [BepInPlugin("akarnokd.planbterraformmods.cheatendlessresources", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;

        static ConfigEntry<int> minResources;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo("Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            minResources = Config.Bind("General", "MinResources", 500, "Minimum resource amount.");


            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        private static void CITem_ContentExtractor_Update01s(ref int2 coords)
        {
            if (!modEnabled.Value)
            {
                return;
            }

            ushort grnd = GHexes.groundData[coords.x, coords.y];
            if (grnd > 0)
            {
                GHexes.groundData[coords.x, coords.y] = (ushort)Math.Max(grnd, minResources.Value);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentDroneExtractor), nameof(CItem_ContentDroneExtractor.Update01s))]
        private static void CItem_ContentDroneExtractor_Update01s(ref int2 coords)
        {
            if (!modEnabled.Value)
            {
                return;
            }

            var drone = SSingleton<SDronesExtractors>.Inst.GetDrone(coords);
            if (drone is null)
                return;

            var grnd = GHexes.groundData[drone.coordsToMine.x, drone.coordsToMine.y];
            if (grnd > 0)
            {
                GHexes.groundData[drone.coordsToMine.x, drone.coordsToMine.y] = (ushort)Math.Max(grnd, minResources.Value);
            }
        }
    }
}