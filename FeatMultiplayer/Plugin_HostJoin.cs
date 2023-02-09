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
        static void Patch_SSceneHud_OnActivate()
        {
            if (multiplayerMode == MultiplayerMode.MainMenu && hostMode.Value)
            {
                LogInfo("Entering multiplayer host mode");
                multiplayerMode = MultiplayerMode.Host;

                StartServer();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoad), nameof(SLoad.QuitGameAndUI))]
        static void Patch_SLoad_QuitGameAndUI()
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                LogInfo("Terminating sessions: " + sessions.Count);
                foreach (var cc in new Dictionary<int, ClientSession>(sessions))
                {
                    LogInfo("  Bye " + cc.Key + " < " + cc.Value.clientName + " >");
                    cc.Value.Send(MessageDisconnect.Instance);
                }
                LogInfo("Terminating host listener");
                stopHostAcceptor.Cancel();
                LogInfo(Environment.StackTrace);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                hostSession.Send(MessageDisconnect.Instance);
            }
        }

        void OnApplicationQuit()
        {
            if (multiplayerMode != MultiplayerMode.MainMenu && multiplayerMode != MultiplayerMode.None)
            {
                stopNetwork.Cancel();

                Thread.Sleep(2000); // FIXME find a better way
            }
        }

    }
}
