// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SMain), nameof(SMain.Update))]
        static bool Patch_SMain_Update(SMain __instance, 
            ref float ____resourcesUnloadingWait,
            ref float ____lastTimeInit)
        {
            bool isHost = multiplayerMode == MultiplayerMode.Host;
            if (multiplayerMode == MultiplayerMode.Client || isHost)
            {
                MultiplayerSMainUpdate(__instance, ref ____resourcesUnloadingWait, ref ____lastTimeInit, isHost);
                return false;
            }
            return true;
        }

        static void MultiplayerSMainUpdate(SMain __instance,
            ref float ____resourcesUnloadingWait,
            ref float ____lastTimeInit,
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
                if (isHost)
                {
                    MultiplayerSMainUpdate_UpdateTime(__instance);
                    MultiplayerSMainUpdate_ApplyTimeScale(__instance);

                    var msgt = new MessageUpdateTime();
                    msgt.GetSnapshot();
                    SendAllClients(msgt);
                }

                SSingleton<SGame>.Inst.Update();
                SSingleton<SViewWorld>.Inst.Update();
                SSingleton<SViewBlocks>.Inst.Update();
                SSingleton<SViewOverlay>.Inst.Update();
                SSingleton<SWater>.Inst.Update();

                // TODO the Update methods
                if (!__instance.IsPausedInGame())
                {
                    if (isHost)
                    {
                        SSingleton<SPlanet>.Inst.Update();

                        var msgp = new MessageUpdatePlanet();
                        msgp.GetSnapshot();
                        SendAllClients(msgp);
                    }
                    // FIXME GWater.supergridWater updates, too large for now
                    SSingleton<SRain>.Inst.Update();
                    if (isHost)
                    {
                        // SWays.Update() can remove lines
                        var before = new HashSet<int>(GWays.lines.Select(x => x?.id ?? 0));

                        SSingleton<SWays>.Inst.Update();

                        var msgw = new MessageUpdateLines();
                        msgw.GetSnapshot(before);
                        SendAllClients(msgw);

                        // NOTE this Update() method is currently empty
                        SSingleton<SWays_PF>.Inst.Update();

                        SSingleton<SDrones>.Inst.Update();

                        var msgd = new MessageUpdateDrones();
                        msgd.GetSnapshot();
                        SendAllClients(msgd);

                        SSingleton<SCities>.Inst.Update();
                    }
                    // FIXME probably can suppress the full call, no need to check on individual Update01s
                    if (SMisc.CheckSimuUnitsTime(0.1f))
                    {
                        SSingleton<SItems>.Inst.Update01s_Constructions();
                    }
                    // Currently, this performs a stack corruption check, let it be
                    SSingleton<SItems>.Inst.Update();
                    // Currently, this updates the forrest info, let it be
                    SSingleton<SItems>.Inst.Update10s_Planet();

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
                }
                SSingleton<SWays>.Inst.Draw();
                SSingleton<SDrones>.Inst.Draw();
                SSingleton<SCamera>.Inst.Update();

            }
            SSingleton<SMusics>.Inst.Update();
            SSingleton<SSounds>.Inst.Update();
            SSingleton<SScenesManager>.Inst.LateUpdateAllScenes();

            MultiplayerSMainUpdate_ReleaseResources(ref ____resourcesUnloadingWait, ref ____lastTimeInit);
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
            if (GInputs.debug.IsKeyDown())
            {
                GGame.debugInfos = !GGame.debugInfos;
                GGame.debugAllUnlocked = GGame.debugInfos && Application.isEditor;
            }
        }

        static void MultiplayerSMainUpdate_ReleaseResources(
            ref float ____resourcesUnloadingWait,
            ref float ____lastTimeInit)
        {
            ____resourcesUnloadingWait += Time.unscaledDeltaTime;
            if (____resourcesUnloadingWait > 60f)
            {
                ____resourcesUnloadingWait = 0f;
                Resources.UnloadUnusedAssets();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSave), nameof(SSave.Save))]
        static bool Patch_SSave_Save()
        {
            return multiplayerMode != MultiplayerMode.Client;
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
