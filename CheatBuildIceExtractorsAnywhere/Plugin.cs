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

        static bool isIceOverride;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), nameof(CItem_ContentIceExtractor.Update01s))]
        static void CItem_ContentIceExtractor_Update01s()
        {
            isIceOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), "IsBuildable")]
        static void CItem_ContentIceExtractor_IsBuildable()
        {
            isIceOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), "IsExtracting")]
        static void CItem_ContentIceExtractor_IsExtracting()
        {
            isIceOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentIceExtractor), "GetBuildFailedInfosMessage")]
        static void CItem_ContentIceExtractor_GetBuildFailedInfosMessage()
        {
            isIceOverride = modEnabled.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SPlanet), nameof(SPlanet.IsIce))]
        static bool SPlanet_IsIce(ref bool __result)
        {
            if (isIceOverride)
            {
                isIceOverride = false;
                __result = true;
                return false;
            }
            return true;
        }
    }
}
