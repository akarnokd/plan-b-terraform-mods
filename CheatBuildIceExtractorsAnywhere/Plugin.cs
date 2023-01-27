using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace CheatBuildIceExtractorsAnywhere
{
    [BepInPlugin("akarnokd.planbterraformmods.cheatbuildiceextractorsanywhere", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> modEnabled;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");


            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        static bool getIceValueOverride;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), nameof(CItem_ContentIceExtractor.Update01s))]
        static void CItem_ContentIceExtractor_Update01s()
        {
            getIceValueOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), "IsBuildable")]
        static void CItem_ContentIceExtractor_IsBuildable()
        {
            getIceValueOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), "IsExtracting")]
        static void CItem_ContentIceExtractor_IsExtracting()
        {
            getIceValueOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWorld), nameof(SWorld.GetIceValue))]
        static bool SWorld_GetIceValue(ref float __result)
        {
            if (getIceValueOverride)
            {
                getIceValueOverride = false;
                __result = 0.5f;
                return false;
            }
            return true;
        }
    }
}
