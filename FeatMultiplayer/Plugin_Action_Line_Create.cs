using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static bool Patch_SWays_CreateLine_Pre(CLine line, CLine lineOld)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                // TODO
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static void PPatch_SWays_CreateLine_Post(CLine line, CLine lineOld)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                // TODO
            }
        }
    }
}
