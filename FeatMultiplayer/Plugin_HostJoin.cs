// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Threading;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud), "OnActivate")]
        static void Patch_SSceneHud_OnActivate()
        {
            if (hostMode.Value)
            {
                if (multiplayerMode == MultiplayerMode.MainMenu)
                {
                    LogInfo("Entering Multiplayer Host mode");
                    multiplayerMode = MultiplayerMode.Host;

                    StartServer();

                    messageTelemetry.Start();
                }
            }
            else
            {
                if (multiplayerMode == MultiplayerMode.MainMenu)
                {
                    LogInfo("Entering SinglePlayer mode");
                    multiplayerMode = MultiplayerMode.SinglePlayer;
                }
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
                //LogInfo(Environment.StackTrace);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                hostSession.Send(MessageDisconnect.Instance);
            }
            CleanupOnQuit();
        }

        static void CleanupOnQuit()
        {
            extractorMainAngles.Clear();
            extractorBucketAngles.Clear();
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
