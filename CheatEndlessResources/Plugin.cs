using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

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
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            minResources = Config.Bind("General", "MinResources", 500, "Minimum resource amount.");


            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CITem_ContentExtractor_Update01s(ref int2 coords)
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
        [HarmonyPatch(typeof(CItem_ContentExtractorDeep), nameof(CItem_ContentExtractorDeep.Update01s))]
        static void CITem_ContentExtractorDeep_Update01s(ref int2 coords)
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
    }
}
