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
        /// Use this to name GameObjects with a mod-specific common prefix.
        /// </summary>
        /// <param name="subname"></param>
        /// <returns></returns>
        static string Naming(string subname)
        {
            return "FeatMultiplayer_" + subname;
        }

    }
}
