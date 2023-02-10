using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        /// <summary>
        /// When a non-public field or method needs accessing.
        /// </summary>
        void InitHaxx()
        {
            Haxx.cDroneDroneDepotIndex = AccessTools.FieldRefAccess<CDrone, int>("droneDepotIndex");

            Haxx.sDronesAddDroneInGrid = AccessTools.Method(typeof(SDrones), "AddDroneInGrid", new Type[] { typeof(CDrone) });

            Haxx.cVehicleStopObjective = AccessTools.FieldRefAccess<CVehicle, int>("_stopObjective");

            Haxx.cVehicleLoadWait = AccessTools.FieldRefAccess<CVehicle, float>("_loadWait");

            Haxx.cItemContentFirstBuildCoords = AccessTools.FieldRefAccess<CItem_Content, int2>("_firstBuildCoords");

            Haxx.cItemContentBuild = AccessTools.Method(typeof(CItem_Content), "Build", new[] { typeof(int2), typeof(bool) });

            Haxx.sBlocksOnChangeItem = AccessTools.Method(typeof(SBlocks), "OnChangeItem", new[] { typeof(int2), typeof(bool), typeof(bool), typeof(bool) });
        }
    }

    /// <summary>
    /// Separate class to not depend on Plugin
    /// </summary>
    public class Haxx
    {
        internal static AccessTools.FieldRef<CDrone, int> cDroneDroneDepotIndex;

        internal static MethodInfo sDronesAddDroneInGrid;

        internal static AccessTools.FieldRef<CVehicle, int> cVehicleStopObjective;

        internal static AccessTools.FieldRef<CVehicle, float> cVehicleLoadWait;

        internal static AccessTools.FieldRef<CItem_Content, int2> cItemContentFirstBuildCoords;

        internal static MethodInfo cItemContentBuild;

        internal static MethodInfo sBlocksOnChangeItem;
    }
}
