// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using LibCommon;

namespace FeatMultiplayer
{
    /// <summary>
    /// The Multiplayer Mod.
    /// </summary>
    [BepInPlugin("akarnokd.planbterraformmods.featmultiplayer", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Access to the single instance of this plugin.
        /// </summary>
        internal static Plugin thePlugin;

        void Awake()
        {
            Logger.LogInfo($"Plugin is loading!");

            thePlugin = this;

            InitConfig();
            InitHaxx();
            InitLogging();
            InitIngameGUI();
            InitMessageDispatcher();

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);

            Logger.LogInfo($"Plugin loaded!");
        }

        void Update()
        {
            DispatchMessageLoop();
        }
    }
}
