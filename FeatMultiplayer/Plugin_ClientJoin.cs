using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        static MessageLoginResponse loginResponse;

        static IEnumerator ClientJoin(string userName, string password)
        {
            if (multiplayerMode != MultiplayerMode.MainMenu)
            {
                yield break;
            }

            clientName = userName;
            clientPassword = password;

            multiplayerMode = MultiplayerMode.ClientLogin;

            var sload = SSingleton<SLoad>.Inst;
            yield return sload.LoadingStep(1f, "Joining as " + userName, 0);

            SSceneSingleton<SSceneUIOverlay>.Inst.ShowLoading();
            SSceneSingleton<SSceneLauncher>.Inst.Deactivate();

            StartClient();

            SendHost(new MessageLogin() { userName = userName, password = password });

            int dots = 0;
            while (loginResponse == null)
            {
                string progress = "";
                for (int j = 0; j < dots; j++)
                {
                    progress += ".";
                }
                dots++;
                if (dots == 20)
                {
                    dots = 0;
                }
                yield return sload.LoadingStep(1f, "Joining as " + userName + " " + progress, 0);
                yield return new WaitForSeconds(0.1f);
            }

            if (loginResponse.reason != "Welcome")
            {
                LogError("Error joining: " + loginResponse.reason + "\n");

                SSceneSingleton<SSceneUIOverlay>.Inst.ShowMessage(SLoc.Get(Naming(loginResponse.reason)), SSceneUIOverlay.MessageType.Error, 10f);
                SSceneSingleton<SSceneHome>.Inst.Activate();

                multiplayerMode = MultiplayerMode.MainMenu;

                yield break;
            }

            loginResponse = null;

            multiplayerMode = MultiplayerMode.ClientLoading;

            yield return sload.LoadingStep(5f, new Action(SSingleton<SWorld_Generation>.Inst.InitGlobalData), 0);
            yield return sload.LoadingStep(10f, new Action(SSingleton<SWorld_Generation>.Inst.Compute_HexesStandards), 0);
            yield return sload.LoadingStep(13f, new Action(SSingleton<SPlanet>.Inst.Reset), 0);
            yield return sload.LoadingStep(15f, new Action(SSingleton<SGame>.Inst.Reset), 0);
            yield return sload.LoadingStep(16f, new Action(SMain.Inst.Reset), 0);
            yield return sload.LoadingStep(17f, new Action(SSingleton<SDrones>.Inst.Reset_DroneGrid), 0);

            yield return sload.LoadingStep(10f, "Waiting for GHexes.altitude", 0);

            // ------------------------------------------------------------------------------------
            // TODO: the various loading steps
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------

            yield return sload.LoadingStep(100f, "Loading complete", 0);

            SSceneSingleton<SSceneUIOverlay>.Inst.ShowMessage(SLoc.Get("Message_LoadingOk"), SSceneUIOverlay.MessageType.Normal, 5f);
            SSceneSingleton<SSceneDialog>.Inst.Activate();

            SSceneSingleton<SSceneCharts>.Inst.Deactivate();
            SSceneSingleton<SScene3D>.Inst.Activate();
            SSceneSingleton<SScene3D_Overlay>.Inst.Activate();
            SSceneSingleton<SSceneHud>.Inst.Activate();
            SSceneSingleton<SSceneHud_Selection>.Inst.Activate();
            SSceneSingleton<SSceneHud_ItemsBars>.Inst.Activate();
            SSceneSingleton<SSceneConsole>.Inst.Activate();
            SSceneSingleton<SSceneTooltip>.Inst.Activate();

            GGame.isPlaying = true;

            SSceneSingleton<SSceneUIOverlay>.Inst.HideLoading();

            multiplayerMode = MultiplayerMode.Client;

            yield return new WaitForSeconds(0.1f);
        }

        static void ReceiveMessageLoginResponse(MessageLoginResponse mlr)
        {
            if (multiplayerMode != MultiplayerMode.ClientLogin)
            {
                return;
            }

            loginResponse = mlr;
        }
    }
}
