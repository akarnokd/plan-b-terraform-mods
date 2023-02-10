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
        [HarmonyPatch(typeof(SWays), nameof(SWays.RemoveLine))]
        static bool Patch_SWays_RemoveLine_Pre(CLine line)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                // TODO
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.RemoveLine))]
        static void PPatch_SWays_RemoveLine_Post(CLine line)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                // TODO
            }
        }
    }
}
