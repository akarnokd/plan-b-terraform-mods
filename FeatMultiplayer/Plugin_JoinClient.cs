﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System;
using System.Collections;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        static MessageLoginResponse loginResponse;

        static MessageSyncAllFlags syncAllFlags;
        static MessageSyncAllAltitude syncAllAltitude;
        static MessageSyncAllWater syncAllWater;
        static MessageSyncAllContentId syncAllContentId;
        static MessageSyncAllContentData syncAllContentData;
        static MessageSyncAllGroundId syncAllGroundId;
        static MessageSyncAllGroundData syncAllGroundData;

        static MessageSyncAllMain syncAllMain;
        static MessageSyncAllGame syncAllGame;
        static MessageSyncAllPlanet syncAllPlanet;
        static MessageSyncAllItems syncAllItems;
        static MessageSyncAllWaterInfo syncAllWaterInfo;
        static MessageSyncAllDrones syncAllDrones;
        static MessageSyncAllWays syncAllWays;
        static MessageSyncAllCamera syncAllCamera;

        static IEnumerator ClientJoin(string userName, string password)
        {
            if (multiplayerMode != MultiplayerMode.MainMenu)
            {
                yield break;
            }

            var sload = SSingleton<SLoad>.Inst;


            clientName = userName;
            clientPassword = password;

            // reset sync message holders

            syncAllFlags = null;
            syncAllAltitude = null;
            syncAllWater = null;
            syncAllContentId = null;
            syncAllContentData = null;
            syncAllGroundId = null;
            syncAllGroundData = null;

            syncAllMain = null;
            syncAllGame = null;
            syncAllPlanet = null;
            syncAllItems = null;
            syncAllWaterInfo = null;
            syncAllDrones = null;
            syncAllWays = null;
            syncAllCamera = null;

            multiplayerMode = MultiplayerMode.ClientJoin;

            sload.QuitGameAndUI();

            yield return sload.LoadingStep(1f, "Joining as " + userName, 0);

            SSceneSingleton<SSceneUIOverlay>.Inst.ShowLoading();
            SSceneSingleton<SSceneLauncher>.Inst.Deactivate();

            StartClient();

            SendHost(new MessageLogin() { userName = userName, password = password });

            int remaining = 300;
            int dots = 0;
            while (loginResponse == null)
            {
                LogDebug("Login Wait " + remaining);
                if (remaining-- == 0)
                {
                    LogError("Error logging in as " + userName + ": Login timeout.\n");

                    SSceneSingleton<SSceneUIOverlay>.Inst.ShowMessage(SLoc.Get("FeatMultiplayer.Error_LoginTimeout"), SSceneUIOverlay.MessageType.Error, 10f);
                    SSceneSingleton<SSceneHome>.Inst.Activate();
                    SSceneSingleton<SSceneUIOverlay>.Inst.HideLoading();

                    multiplayerMode = MultiplayerMode.MainMenu;

                    yield break;
                }
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
                //yield return new WaitForSeconds(0.1f);
            }

            if (loginResponse.reason != "Welcome")
            {
                LogError("Error joining: " + loginResponse.reason + "\n");

                SSceneSingleton<SSceneUIOverlay>.Inst.ShowMessage(SLoc.Get("FeatMultiplayer." + loginResponse.reason), SSceneUIOverlay.MessageType.Error, 10f);
                SSceneSingleton<SSceneHome>.Inst.Activate();
                SSceneSingleton<SSceneUIOverlay>.Inst.HideLoading();

                multiplayerMode = MultiplayerMode.MainMenu;

                yield break;
            }

            LogDebug("Login Response " + loginResponse.reason);

            loginResponse = null;

            yield return sload.LoadingStep(5f, new Action(SSingleton<SWorld_Generation>.Inst.InitGlobalData), 0);
            yield return sload.LoadingStep(10f, new Action(SSingleton<SWorld_Generation>.Inst.Compute_HexesStandards), 0);
            yield return sload.LoadingStep(13f, new Action(SSingleton<SPlanet>.Inst.Reset), 0);
            yield return sload.LoadingStep(15f, new Action(SSingleton<SGame>.Inst.Reset), 0);
            yield return sload.LoadingStep(16f, new Action(SMain.Inst.Reset), 0);
            yield return sload.LoadingStep(17f, new Action(SSingleton<SDrones>.Inst.Reset_DroneGrid), 0);

            /*
            for (int i = 0; i < 60; i++)
            {
                LogDebug(i + " FlagsDebug: Check " + FlagsToStr((uint)GHexes.flags[1270, 406]));
                yield return null;
            }
            */

            // ------------------------------------------------------------------------------------
            // The various loading steps
            // ------------------------------------------------------------------------------------

            yield return sload.LoadingStep(20f, "Waiting for GHexes.flags", 0);

            yield return WaitForField(() => syncAllFlags, () => syncAllFlags = null);

            yield return sload.LoadingStep(22f, "Waiting for GHexes.altitude", 0);

            yield return WaitForField(() => syncAllAltitude, () => syncAllAltitude = null);

            yield return sload.LoadingStep(24f, "Waiting for GHexes.water", 0);

            yield return WaitForField(() => syncAllWater, () => syncAllWater = null);

            yield return sload.LoadingStep(26f, "Waiting for GHexes.contentId", 0);

            yield return WaitForField(() => syncAllContentId, () => syncAllContentId = null);

            yield return sload.LoadingStep(28f, "Waiting for GHexes.contentData", 0);

            yield return WaitForField(() => syncAllContentData, () => syncAllContentData = null);

            yield return sload.LoadingStep(30f, "Waiting for GHexes.groundId", 0);

            yield return WaitForField(() => syncAllGroundId, () => syncAllGroundId = null);

            yield return sload.LoadingStep(32f, "Waiting for GHexes.groundData", 0);

            yield return WaitForField(() => syncAllGroundData, () => syncAllGroundData = null);

            yield return sload.LoadingStep(34f, "Computing Hexes Altitude Data", 0);

            yield return SSingleton<SWorld_Generation>.Inst.Compute_HexesAltitudeData(true, 36);

            yield return sload.LoadingStep(44f, "Waiting for SMain data", 0);

            yield return WaitForField(() => syncAllMain, () => syncAllMain = null);

            yield return sload.LoadingStep(46f, "Waiting for SGame data", 0);

            yield return WaitForField(() => syncAllGame, () => syncAllGame = null);

            // Just indicates if the tutorial panel is open or not.
            // yield return sload.LoadingStep(40f, "Waiting for SSceneDialog data", 0);

            yield return sload.LoadingStep(48f, "Waiting for SPlanet data", 0);

            yield return WaitForField(() => syncAllPlanet, () => syncAllPlanet = null);

            yield return sload.LoadingStep(50f, "Waiting for SItems data", 0);

            yield return WaitForField(() => syncAllItems, () => syncAllItems = null);

            yield return sload.LoadingStep(52f, "Counting trees", 0);

            CountForestHexes();

            yield return sload.LoadingStep(54f, "Waiting for SWater data", 0);

            yield return WaitForField(() => syncAllWaterInfo, () => syncAllWaterInfo = null);

            yield return sload.LoadingStep(56f, "Waiting for SDrones data", 0);

            yield return WaitForField(() => syncAllDrones, () => syncAllDrones = null);

            yield return sload.LoadingStep(58f, "Waiting for SWays data", 0);

            yield return WaitForField(() => syncAllWays, () => syncAllWays = null);

            yield return sload.LoadingStep(60f, "Waiting for SCamera data", 0);

            yield return WaitForField(() => syncAllCamera, () => syncAllCamera = null);

            // ------------------------------------------------------------------------------------

            // remake the terrain

            yield return sload.LoadingStep(70f, new Action(SSingleton<SBlocks>.Inst.GenerateData), 0);

            yield return sload.LoadingStep(72f, new Action(SSingleton<SPlanet>.Inst.ComputeTemperaturesInitial), 0);

            yield return sload.LoadingStep(74f, new Action(SSingleton<SRain>.Inst.Generate), 0);

            SSingleton<SCamera>.Inst.Reset(true);

            yield return sload.LoadingStep(82f, new Action(SSingleton<SViewWorld>.Inst.GenerateMeshes), 0);

            yield return sload.LoadingStep(84f, new Action(SSingleton<SViewBlocks>.Inst.GenerateMeshes), 0);

            yield return sload.LoadingStep(85f, new Action(SSingleton<SViewOverlay>.Inst.GenerateMeshes), 0);

            yield return sload.LoadingStep(86f, new Action(SSingleton<SWater>.Inst.GenerateMesh), 0);

            yield return sload.LoadingStep(90f, "Loading finalization", 0);

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

            messageTelemetry.Start();
            mainTelemetry.Start();
            viewBlocksTelemetry.Start();

            yield return new WaitForSeconds(0.1f);
        }

        static IEnumerator WaitForField<T>(Func<T> getter, Action clear) where T : MessageSync
        {
            for (; ; )
            {
                var v = getter();
                if (v != null)
                {
                    /*
                    if (UnityEngine.Random.RandomRangeInt(0, 2) == 1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    */
                    try
                    {
                        try
                        {
                            LogDebug(typeof(T) + ".ApplySnapshot() begin");
                            // LogDebug(" FlagsDebug: Check " + FlagsToStr((uint)GHexes.flags[1270, 406]));
                            v.ApplySnapshot();
                            // LogDebug(" FlagsDebug: Check " + FlagsToStr((uint)GHexes.flags[1270, 406]));
                        }
                        finally
                        {
                            clear();
                        }
                    } 
                    catch (Exception ex)
                    {
                        LogError(typeof(T) + ".ApplySnapshot() crashed\r\n" + ex);
                        throw ex;
                    }
                    yield break;
                }
                // LogDebug(" FlagsDebug: Check yield " + FlagsToStr((uint)GHexes.flags[1270, 406]));
                yield return new WaitForSeconds(0.1f);
            }
        }

        static void CountForestHexes()
        {
            int c = 0;
            for (int l = 0; l < GWorld.size.x; l++)
            {
                for (int m = GWorld.rowsMin[l]; m <= GWorld.rowsMax[l]; m++)
                {
                    var @int = new int2(l, m);
                    var content = ContentAt(@int);

                    if (content is CItem_ContentForest forest && forest.IsAliveAndVisible(@int))
                    {
                        c++;
                    }
                }
            }
            GItems.forestNbHexes = c;
        }

        static void ReceiveMessageLoginResponse(MessageLoginResponse msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }

            loginResponse = msg;
        }

        static void ReceiveMessageSyncAllFlags(MessageSyncAllFlags msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllFlags = msg;
        }

        static void ReceiveMessageSyncAllAltitude(MessageSyncAllAltitude msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllAltitude = msg;
        }

        static void ReceiveMessageSyncAllWater(MessageSyncAllWater msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllWater = msg;
        }

        static void ReceiveMessageSyncAllContentId(MessageSyncAllContentId msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllContentId = msg;
        }

        static void ReceiveMessageSyncAllContentData(MessageSyncAllContentData msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllContentData = msg;
        }

        static void ReceiveMessageSyncAllGroundId(MessageSyncAllGroundId msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllGroundId = msg;
        }

        static void ReceiveMessageSyncAllGroundData(MessageSyncAllGroundData msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllGroundData = msg;
        }

        static void ReceiveMessageSyncAllMain(MessageSyncAllMain msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllMain = msg;
        }

        static void ReceiveMessageSyncAllGame(MessageSyncAllGame msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllGame = msg;
        }

        static void ReceiveMessageSyncAllPlanet(MessageSyncAllPlanet msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllPlanet = msg;
        }

        static void ReceiveMessageSyncAllItems(MessageSyncAllItems msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllItems = msg;
        }

        static void ReceiveMessageSyncAllWaterInfo(MessageSyncAllWaterInfo msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllWaterInfo = msg;
        }

        static void ReceiveMessageSyncAllDrones(MessageSyncAllDrones msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllDrones = msg;
        }

        static void ReceiveMessageSyncAllWays(MessageSyncAllWays msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllWays = msg;
        }

        static void ReceiveMessageSyncAllCamera(MessageSyncAllCamera msg)
        {
            if (multiplayerMode != MultiplayerMode.ClientJoin)
            {
                LogDebug("Receive" + msg.GetType() + " ignored in " + multiplayerMode);
                return;
            }
            syncAllCamera = msg;
        }
    }
}
