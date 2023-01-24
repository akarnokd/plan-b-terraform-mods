using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace ProgressSpeed
{
    [BepInPlugin("akarnokd.planbterraformmods.progressspeed", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> extractorSpeed;
        static ConfigEntry<int> extractorDeepSpeed;
        static ConfigEntry<int> factorySpeed;
        static ConfigEntry<int> citySpeed;
        static ConfigEntry<float> droneSpeed;
        static ConfigEntry<float> droneTakeoffDuration;

        static MethodInfo IsExtracting;
        static MethodInfo IsExtractingDeep;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");

            extractorSpeed = Config.Bind("General", "ExtractorSpeed", 1, "The speed multiplier of Extractors.");
            extractorDeepSpeed = Config.Bind("General", "DeepExtractorSpeed", 1, "The speed multiplier of Deep Extractors.");
            factorySpeed = Config.Bind("General", "FactorySpeed", 1, "The speed multiplier of Factories (includes Assemblers, Greenhouses, Ice Extractors).");
            citySpeed = Config.Bind("General", "CitySpeed", 1, "The speed multiplier of Cities.");
            droneSpeed = Config.Bind("General", "DroneSpeedAdd", 0f, "Adds to the global drone speed.");
            droneTakeoffDuration = Config.Bind("General", "DroneTakeoffDurationAdd", 0f, "Adds to the global drone takeoff duration. Use negative to speed it up.");

            IsExtracting = AccessTools.Method(typeof(CItem_ContentExtractor), "IsExtracting", new Type[] { typeof(int2) });
            IsExtractingDeep = AccessTools.Method(typeof(CItem_ContentExtractorDeep), "IsExtracting", new Type[] { typeof(int2) });

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CITem_ContentExtractor_Update01s(CItem_ContentExtractor __instance, int2 coords)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            if ((bool)IsExtracting.Invoke(__instance, new object[] { coords }))
            {
                int c = extractorSpeed.Value;
                for (int i = 1; i < c; i++)
                {
                    __instance.dataProgress.IncrementIFP(coords);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractorDeep), nameof(CItem_ContentExtractorDeep.Update01s))]
        static void CITem_ContentExtractorDeep_Update01s(CItem_ContentExtractorDeep __instance, int2 coords)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            if ((bool)IsExtractingDeep.Invoke(__instance, new object[] { coords }))
            {
                int c = extractorDeepSpeed.Value;
                for (int i = 1; i < c; i++)
                {
                    __instance.dataProgress.IncrementIFP(coords);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "ProcessStocks")]
        static void CItem_ContentFactory_ProcessStocks(CItem_ContentFactory __instance, int2 coords, int progressFrame)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            int c = factorySpeed.Value - 1;
            if (c > 0)
            {

                int newProgress = progressFrame + c;
                if (newProgress >= __instance.dataProgress.valueMax)
                {
                    newProgress = 0;
                }
                __instance.dataProgress.SetValue(coords, newProgress);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.Update01s))]
        static void CItem_ContentCityInOut_Update01s_Post(CItem_ContentCityInOut __instance, int2 coords)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            int valueAfter = __instance.dataProgress.GetValue(coords);
            if (valueAfter > 0)
            {
                int c = citySpeed.Value;
                for (int i = 1; i < c; i++)
                {
                    __instance.dataProgress.IncrementIFP(coords);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CDrone), "SetTimes")]
        static void CDrone_SetTimes(
            ref double ___startTime, 
            ref double ___endTime, 
            ref CTransform ___startTransform, 
            ref CTransform ___endTransform)
        {
            if (!modEnabled.Value)
            {
                return;
            }

            float magnitude = (___startTransform.pos - ___endTransform.pos).magnitude;
            
            ___endTime = ___startTime 
                + (double)(GDrones.durationTakeOff + droneTakeoffDuration.Value)
                + (double)(magnitude / (GDrones.speed + droneSpeed.Value));
        }
    }
}
