// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

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

            Haxx.cDroneStartTransform = AccessTools.FieldRefAccess<CDrone, CTransform>("startTransform");

            Haxx.cDroneEndTransform = AccessTools.FieldRefAccess<CDrone, CTransform>("endTransform");

            Haxx.cDroneStartTime = AccessTools.FieldRefAccess<CDrone, double>("startTime");

            Haxx.cDroneEndTime = AccessTools.FieldRefAccess<CDrone, double>("endTime");

            Haxx.sDronesAddDroneInGrid = AccessTools.Method(typeof(SDrones), "AddDroneInGrid", new Type[] { typeof(CDrone) });

            Haxx.cVehicleStopObjective = AccessTools.FieldRefAccess<CVehicle, int>("_stopObjective");

            Haxx.cVehicleLoadWait = AccessTools.FieldRefAccess<CVehicle, float>("_loadWait");

            Haxx.cItemContentFirstBuildCoords = AccessTools.FieldRefAccess<CItem_Content, int2>("_firstBuildCoords");

            Haxx.cItemContentBuild = AccessTools.Method(typeof(CItem_Content), "Build", new[] { typeof(int2), typeof(bool) });

            Haxx.cItemContentCopy = AccessTools.Method(typeof(CItem_Content), "Copy", new[] { typeof(int2), typeof(int2) });

            Haxx._sBlocksOnChangeItem = AccessTools.Method(typeof(SBlocks), "OnChangeItem", new[] { typeof(int2), typeof(bool), typeof(bool), typeof(bool) });

            Haxx.cItemWayStopBuildLine = AccessTools.FieldRefAccess<CItem_WayStop, CLine>("_buildLine");
            Haxx.cItemWayStopIsReverse = AccessTools.FieldRefAccess<CItem_WayStop, bool>("_isReverse");
            Haxx.cItemWayStopBuildModeLastFrame = AccessTools.FieldRefAccess<CItem_WayStop, int>("_buildModeLastFrame");
        }
    }

    /// <summary>
    /// Separate class to not depend on Plugin
    /// </summary>
    public class Haxx
    {
        internal static AccessTools.FieldRef<CDrone, int> cDroneDroneDepotIndex;

        internal static AccessTools.FieldRef<CDrone, CTransform> cDroneStartTransform;

        internal static AccessTools.FieldRef<CDrone, CTransform> cDroneEndTransform;

        internal static AccessTools.FieldRef<CDrone, double> cDroneStartTime;

        internal static AccessTools.FieldRef<CDrone, double> cDroneEndTime;

        internal static MethodInfo sDronesAddDroneInGrid;

        internal static AccessTools.FieldRef<CVehicle, int> cVehicleStopObjective;

        internal static AccessTools.FieldRef<CVehicle, float> cVehicleLoadWait;

        internal static AccessTools.FieldRef<CItem_Content, int2> cItemContentFirstBuildCoords;

        internal static MethodInfo cItemContentBuild;

        internal static MethodInfo cItemContentCopy;

        internal static MethodInfo _sBlocksOnChangeItem;

        static Action<int2, bool, bool, bool> _sBlocksOnChangeItemDelegate;

        internal static void SBlocks_OnChangeItem(int2 c, bool updateNeighbors = false, bool updateGroundToo = false, bool containersOnly = false)
        {
            _sBlocksOnChangeItemDelegate ??= AccessTools.MethodDelegate<Action<int2, bool, bool, bool>>(_sBlocksOnChangeItem, SSingleton<SBlocks>.Inst);
            _sBlocksOnChangeItemDelegate(c, updateNeighbors, updateGroundToo, containersOnly);
        }

        internal static AccessTools.FieldRef<CItem_WayStop, CLine> cItemWayStopBuildLine;

        internal static AccessTools.FieldRef<CItem_WayStop, bool> cItemWayStopIsReverse;

        internal static AccessTools.FieldRef<CItem_WayStop, int> cItemWayStopBuildModeLastFrame;
    }
}
