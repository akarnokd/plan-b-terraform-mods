using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FeatMultiplayer
{
    [BepInPlugin("akarnokd.planbterraformmods.featmultiplayer", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            Logger.LogInfo($"Plugin is loading!");

            InitConfig();
            InitLogging();
            InitMessageDispatcher();

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);

            Logger.LogInfo($"Plugin loaded!");
        }
    }
}
