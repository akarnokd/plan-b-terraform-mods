// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        static readonly CallTelemetry mainTelemetry = new("Main");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SMain), nameof(SMain.Update))]
        static bool Patch_SMain_Update(SMain __instance, 
            ref float ____resourcesUnloadingWait,
            ref float ____lastTimeInit)
        {
            bool isHost = multiplayerMode == MultiplayerMode.Host;
            if (multiplayerMode == MultiplayerMode.Client || isHost)
            {
                try
                {
                    MultiplayerSMainUpdate(__instance, ref ____resourcesUnloadingWait, isHost);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
                return false;
            }
            return true;
        }

        static void MultiplayerSMainUpdate(SMain __instance,
            ref float ____resourcesUnloadingWait,
            bool isHost)
        {
            if (GMain.initializationProgress != 100)
            {
                return;
            }
            SSingleton<SInputs>.Inst.Update();
            SSingleton<SScenesManager>.Inst.UpdateAllScenes();

            MultiplayerSMainUpdate_Frametimes();

            if (GGame.isPlaying)
            {
                mainTelemetry.GetAndReset();
                if (isHost)
                {
                    MultiplayerSMainUpdate_UpdateTime(__instance);
                    MultiplayerSMainUpdate_ApplyTimeScale(__instance);

                    var msgt = new MessageUpdateTime();
                    msgt.GetSnapshot();
                    SendAllClients(msgt);
                }

                SSingleton<SGame>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SGame.Update");
                SSingleton<SViewWorld>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SViewWorld.Update");
                SSingleton<SViewBlocks>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SViewBlocks.Update");
                SSingleton<SViewOverlay>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SViewOverlay.Update");
                SSingleton<SWater>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SWater.Update");
                SSingleton<SViewWater>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SViewWater.Update");
                SSingleton<SViewWaterflow>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SViewWaterflow.Update");

                bool paused = isHost && __instance.IsPausedInGame();
                // TODO the Update methods
                if (!paused)
                {
                    if (isHost)
                    {
                        SSingleton<SPlanet>.Inst.Update();
                        mainTelemetry.AddTelemetryCheckpoint("SPlanet.Update");

                        SyncPlanetAllClients();
                    }
                    // FIXME GWater.supergridWater updates, too large for now
                    SSingleton<SRain>.Inst.Update();
                    mainTelemetry.AddTelemetryCheckpoint("SRain.Update");
                    if (isHost)
                    {
                        MultiplayerSMainUpdate_SWaysUpdate();

                        // NOTE this Update() method is currently empty
                        SSingleton<SWays_PF>.Inst.Update();
                        mainTelemetry.AddTelemetryCheckpoint("SWays_PF.Update");

                        MultiplayerSMainUpdate_DronesUpdate();
                        mainTelemetry.AddTelemetryCheckpoint("Drones.Update");

                    }
                    // Recipe changes needed on the client, the rest are individually supressed
                    SSingleton<SCities>.Inst.Update();
                    mainTelemetry.AddTelemetryCheckpoint("SCities.Update");
                    // FIXME probably can suppress the full call, no need to check on individual Update01s
                    if (SMisc.CheckSimuUnitsTime(0.1f))
                    {
                        SSingleton<SItems>.Inst.Update01s_Constructions();
                        mainTelemetry.AddTelemetryCheckpoint("SItems.Update01s_Constructions");
                    }
                    if (isHost)
                    {
                        // Currently, this performs a stack corruption check
                        // Crashes on the client because the drone state is not complete
                        // We don't sync the CDrone.TransferStep fields as they are not needed
                        // on the client.
                        SSingleton<SItems>.Inst.Update();
                        mainTelemetry.AddTelemetryCheckpoint("SItems.Update");
                    }
                    // Currently, this updates the forrest info, let it be
                    SSingleton<SItems>.Inst.Update10s_Planet();
                    mainTelemetry.AddTelemetryCheckpoint("SItems.Update10s_Planet");

                    if (isHost)
                    {
                        var msgi = new MessageUpdateItems();
                        msgi.GetSnapshot();
                        SendAllClients(msgi);
                    }
                }
                if (!__instance.IsPausedInMenu())
                {
                    SSingleton<SMouse>.Inst.Update();
                    mainTelemetry.AddTelemetryCheckpoint("SMouse.Update");
                }
                SSingleton<SWays>.Inst.Draw();
                mainTelemetry.AddTelemetryCheckpoint("SWays.Draw");
                SSingleton<SDrones>.Inst.Draw();
                mainTelemetry.AddTelemetryCheckpoint("SDrones.Draw");
                SSingleton<SCamera>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SCamera.Update");

                mainTelemetry.AddTelemetry("Main");
            }
            SSingleton<SMusics>.Inst.Update();
            SSingleton<SSounds>.Inst.Update();
            SSingleton<SScenesManager>.Inst.LateUpdateAllScenes();

            MultiplayerSMainUpdate_ReleaseResources(ref ____resourcesUnloadingWait);
        }

        static void MultiplayerSMainUpdate_UpdateTime(SMain __instance)
        {
            float num = Mathf.Min(0.02f, Time.unscaledDeltaTime);
            GMain.simuUnitsTime_LastFrame = GMain.simuUnitsTime;
            GMain.simuUnitsDeltaTime = num * GMain.simuUnitsTime_Scale * GMain.userGameSpeed * (float)(__instance.IsPausedInMenu() ? 0 : 1);
            GMain.simuUnitsDeltaTime = Mathf.Min(0.1f, GMain.simuUnitsDeltaTime);
            GMain.simuUnitsTime += (double)GMain.simuUnitsDeltaTime;
            GMain.simuPlanetTime_LastFrame = GMain.simuPlanetTime;
            float num2 = num * GMain.simuPlanetTime_Scale * GMain.userGameSpeed * (float)(__instance.IsPausedInMenu() ? 0 : 1);
            num2 = Mathf.Min(0.1f, num2);
            GMain.simuPlanetTime += (double)num2;
        }

        static void MultiplayerSMainUpdate_ApplyTimeScale(SMain __instance)
        {
            Time.timeScale = GMain.userGameSpeed * (float)(__instance.IsPausedInMenu() ? 0 : 1);
            GMain.timePlayed += Time.unscaledDeltaTime;
        }

        static void MultiplayerSMainUpdate_Frametimes()
        {
            GMain.frameRealTime_LastFrame = GMain.frameRealTime;
            GMain.frameRealTime = (double)Time.realtimeSinceStartup;
            GMain.frameRealTime_DeltaTime = (float)(GMain.frameRealTime - GMain.frameRealTime_LastFrame);
            if (GInputs.debug_v2.IsKeyDown())
            {
                GGame.debugInfos = !GGame.debugInfos;
                GGame.debugAllUnlocked = GGame.debugInfos && Application.isEditor;
            }
        }

        static void MultiplayerSMainUpdate_ReleaseResources(
            ref float ____resourcesUnloadingWait)
        {
            ____resourcesUnloadingWait += Time.unscaledDeltaTime;
            if (____resourcesUnloadingWait > 60f)
            {
                ____resourcesUnloadingWait = 0f;
                Resources.UnloadUnusedAssets();
            }
        }

        static readonly Dictionary<int, SnapshotDroneLive> droneStateSnapshot = new();

        static void MultiplayerSMainUpdate_DronesUpdate()
        {
            if (syncDroneDiff.Value)
            {
                droneStateSnapshot.Clear();
                HashSet<int> dronesBefore = new();

                foreach (var drone in GDrones.drones)
                {
                    dronesBefore.Add(drone.id);
                    var snp = new SnapshotDroneLive();
                    snp.GetSnapshot(drone);
                    droneStateSnapshot.Add(drone.id, snp);
                }

                SSingleton<SDrones>.Inst.Update();

                foreach (var drone in GDrones.drones)
                {
                    dronesBefore.Remove(drone.id);
                }

                var msgd = new MessageUpdateDrones();
                msgd.GetDiffSnapshot(dronesBefore, droneStateSnapshot);
                SendAllClients(msgd);
            }
            else
            {
                HashSet<int> dronesBefore = new();
                foreach (var drone in GDrones.drones)
                {
                    dronesBefore.Add(drone.id);
                }


                SSingleton<SDrones>.Inst.Update();

                foreach (var drone in GDrones.drones)
                {
                    dronesBefore.Remove(drone.id);
                }

                var msgd = new MessageUpdateDrones();
                msgd.GetSnapshot(dronesBefore);
                SendAllClients(msgd);
            }
        }

        static Dictionary<int, List<SnapshotNode>> lineNodes = new();
        static void MultiplayerSMainUpdate_SWaysUpdate()
        {
            if (syncLineDiff.Value) {
                // SWays.Update() can remove lines
                var before = new HashSet<int>();
                lineNodes.Clear();
                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    var line = GWays.lines[i];
                    before.Add(line.id);

                    var nodeSnp = new List<SnapshotNode>();
                    foreach (var n in line.nodes)
                    {
                        var snp = new SnapshotNode();
                        snp.GetSnapshot(n);
                        nodeSnp.Add(snp);
                    }
                    lineNodes.Add(line.id, nodeSnp);
                }

                SSingleton<SWays>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SWays.Update");

                var msgw = new MessageUpdateLines();
                msgw.GetSnapshotDiff(before, lineNodes);
                SendAllClients(msgw);
            }
            else
            {
                // SWays.Update() can remove lines
                var before = new HashSet<int>();
                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    var line = GWays.lines[i];
                    before.Add(line.id);
                }

                SSingleton<SWays>.Inst.Update();
                mainTelemetry.AddTelemetryCheckpoint("SWays.Update");

                var msgw = new MessageUpdateLines();
                msgw.GetSnapshot(before);
                SendAllClients(msgw);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSave), nameof(SSave.Save))]
        static bool Patch_SSave_Save()
        {
            return multiplayerMode != MultiplayerMode.Client;
        }

        static void SyncPlanetAllClients()
        {
            int idx = GPlanet.dailyTemperature.Count;

            foreach (var session in sessions.Values)
            {
                var curr = session.planetDataSync;
                if (idx != curr)
                {
                    session.planetDataSync = idx;

                    var msgp = new MessageUpdatePlanet();
                    msgp.GetSnapshot(curr);
                    session.Send(msgp);
                }
            }
        }

        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        public static bool logDebugMainMessages;

        static void ReceiveMessageUpdateTime(MessageUpdateTime msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugMainMessages)
                {
                    LogDebug("ReceiveMessageUpdateTime: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugMainMessages)
                {
                    LogDebug("ReceiveMessageUpdateTime: Handling " + msg.GetType());
                }

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateTime: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdatePlanet(MessageUpdatePlanet msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugMainMessages)
                {
                    LogDebug("ReceiveMessageUpdatePlanet: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugMainMessages)
                {
                    LogDebug("ReceiveMessageUpdatePlanet: Handling " + msg.GetType());
                }
                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdatePlanet: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
