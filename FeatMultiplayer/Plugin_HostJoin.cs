using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud), "OnActivate")]
        static void SSceneHud_OnActivate()
        {
            if (multiplayerMode == MultiplayerMode.MainMenu && hostMode.Value)
            {
                multiplayerMode = MultiplayerMode.Host;

                StartServer();
            }
        }

        void OnApplicationQuit()
        {
            stopNetwork.Cancel();

            Thread.Sleep(2000); // FIXME find a better way
        }

    }
}
