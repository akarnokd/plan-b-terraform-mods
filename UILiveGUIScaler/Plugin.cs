using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UILiveGUIScaler
{
    [BepInPlugin("akarnokd.planbterraformmods.uiliveguiscaler", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> minScale;
        static ConfigEntry<int> maxScale;
        static ConfigEntry<int> stepSize;

        static ManualLogSource logger;

        static MethodInfo setUiScaling;

        static bool homeScreenReached;

        static GameObject scalingText;

        static int scalingPercent;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            minScale = Config.Bind("General", "MinScale", 50, "The minimum percent for scaling.");
            maxScale = Config.Bind("General", "MaxScale", 300, "The maximum percent for scaling.");
            stepSize = Config.Bind("General", "Step", 5, "Step percent of scaling when changing it");

            setUiScaling = AccessTools.Method(typeof(SScenesManager), "SetUiScaling", new Type[] { typeof(float) });

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);

        }

        void Update()
        {
            if (modEnabled.Value && setUiScaling != null && homeScreenReached)
            {
                if (scalingPercent == 0)
                {
                    scalingPercent = (int)(GUIScalingSupport.currentScale * 100);
                }
                var scrollDelta = Input.mouseScrollDelta.y;

                var changeScale = false;

                if ((GUITools.IsKeyDown(KeyCode.KeypadPlus) || scrollDelta > 0)
                    && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    // && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                )
                {
                    scalingPercent += stepSize.Value;
                    changeScale = true;
                }

                if ((GUITools.IsKeyDown(KeyCode.KeypadMinus) || scrollDelta < 0)
                    && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                // && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                )
                {
                    scalingPercent -= stepSize.Value;
                    changeScale = true;
                }

                if (changeScale)
                {
                    var scale = scalingPercent / 100f;
                    scale = Mathf.Clamp(minScale.Value / 100f, scale, maxScale.Value / 100f);
                    setUiScaling.Invoke(SSingleton<SScenesManager>.Inst, new object[] { scale });

                    if (scalingText != null)
                    {
                        Destroy(scalingText);
                    }

                    scalingText = new GameObject("UILiveGUIScaler");
                    var canvas = scalingText.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 1000;

                    var txt = GUITools.CreateBox(scalingText, "UILiveGUIScaler_Text", "     " + (scalingPercent) + " %     ", 50, new Color(0, 0, 0, 0.95f), Color.yellow);

                    GUITools.SetLocalPosition(txt, 0, 0);

                    Destroy(scalingText, 1f);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHome), "OnActivate")]
        static void SScneHome_OnActivate()
        {
            homeScreenReached = true;
        }
    }
}
