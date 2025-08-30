using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace CheatProgressSpeed
{
    [BepInPlugin("akarnokd.planbterraformmods.cheatprogressspeed", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.featproductionstats", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> extractorSpeed;
        static ConfigEntry<int> extractorDeepSpeed;
        static ConfigEntry<int> factorySpeed;
        static ConfigEntry<int> citySpeed;
        static ConfigEntry<float> droneSpeed;
        static ConfigEntry<float> droneTakeoffDuration;
        static ConfigEntry<float> vehicleSpeedLow;
        static ConfigEntry<float> vehicleSpeedMedium;
        static ConfigEntry<float> vehicleSpeedMax;

        static MethodInfo IsExtracting;
        static MethodInfo CheckStocks;
        static MethodInfo ProcessStocks;

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

            vehicleSpeedLow = Config.Bind("General", "VehicleSpeedLowAdd", 0f, "Adds to the vehicle's low speed.");
            vehicleSpeedMedium = Config.Bind("General", "VehicleSpeedMediumAdd", 0f, "Adds to the vehicle's medium speed.");
            vehicleSpeedMax = Config.Bind("General", "VehicleSpeedMaxAdd", 0f, "Adds to the vehicle's medium speed.");

            IsExtracting = AccessTools.Method(typeof(CItem_ContentExtractor), "IsExtracting", new Type[] { typeof(int2) });

            CheckStocks = AccessTools.Method(typeof(CItem_ContentFactory), "CheckStocks", new Type[] { typeof(int2), typeof(CRecipe), typeof(int), typeof(bool) });
            ProcessStocks = AccessTools.Method(typeof(CItem_ContentFactory), "ProcessStocks", new Type[] { typeof(int2), typeof(CRecipe), typeof(int) });

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
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static bool CItem_ContentFactory_Update01s(CItem_ContentFactory __instance, ref int2 coords)
        {
            if (!modEnabled.Value)
            {
                return true;
            }
            if (__instance is CItem_ContentSpaceLift
                || __instance is CItem_ContentConstructionSite
                || __instance is CItem_ContentLabo)
            {
                return true;
            }
            int c = factorySpeed.Value;
            if (GHexes.water[coords.x, coords.y] < __instance.waterLevelStopBuildings)
            {
                if (!__instance.IsValidFrame(coords))
                {
                    return false;
                }
                for (int i = 0; i < c; i++) { 
                    CRecipe recipe = __instance.GetRecipe(coords);
                    int value = __instance.dataProgress.GetValue(coords);
                    if ((bool)CheckStocks.Invoke(__instance, [coords, recipe, value, false]))
                    {
                        ProcessStocks.Invoke(__instance, [coords, recipe, value]);
                    }
                }
            }
            else
            {
                SSingleton<SWarnings>.Inst.ShowWarning("Warning_BuildingDrowned", coords, 2f);
            }
            return false;
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
        [HarmonyPatch(typeof(CDrone), "SetupMove")]
        static void CDrone_SetupMove(
            ref double ___startTime, 
            ref double ___endTime,
            int ___droneDepotIndex,
            int2 coordsStart, int2 coordsend)
        {
            if (!modEnabled.Value)
            {
                return;
            }

            float magnitude = (GHexes.Pos(coordsStart) - GHexes.Pos(coordsend)).magnitude;
            float num = 0.01f * (float)___droneDepotIndex;
            ___endTime = ___startTime 
                + (double)(GDrones.durationTakeOff + droneTakeoffDuration.Value) 
                + (double)(magnitude / (GDrones.speed + droneSpeed.Value)) 
                + (double)num;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Vehicle), nameof(CItem_Vehicle.Init))]
        static void CItem_Vehicle_Init(CItem_Vehicle __instance)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            __instance.speedLow += vehicleSpeedLow.Value;
            __instance.speedMedium += vehicleSpeedMedium.Value;
            __instance.speedMax += vehicleSpeedMax.Value;
        }
    }
}
