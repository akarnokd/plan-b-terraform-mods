// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressBlocksOnChange;

        static bool blocksOnChangeCalledWhileSuppressed;

        static readonly List<int2> worldTexturesToUpdate = new();

        static void SendUpdateStacksAndContentData(int2 coords, bool updateBlocks)
        {
            var msg = new MessageUpdateDatasAt();
            msg.GetSnapshot(coords, updateBlocks);
            SendAllClients(msg);
        }

        /// <summary>
        /// Add the ability to suppress such OnChangeItem events so they don't
        /// trigger further messages down the line.
        /// </summary>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SBlocks), "OnChangeItem")]
        static bool Patch_SBlocks_OnChangeItem()
        {
            if (suppressBlocksOnChange)
            {
                blocksOnChangeCalledWhileSuppressed = true;
            }
            return !suppressBlocksOnChange;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewWorld), nameof(SViewWorld.OnBuildItem_UpdateTxWorld))]
        static bool Patch_SViewWorld_OnBuildItem_UpdateTxWorld(int2 coords)
        {
            if (suppressBlocksOnChange)
            {
                worldTexturesToUpdate.Add(coords);
            }
            return !suppressBlocksOnChange;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static bool Patch_CItem_ContentExtractor_Update01s_Pre()
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            } 
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                // vanilla calls SBlocks.OnChangeItem which my trigger other messages before we send
                // the content data update message, so defer it and call SBlocks.OnChangeItem in post.
                suppressBlocksOnChange = true;
                blocksOnChangeCalledWhileSuppressed = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void Patch_CItem_ContentExtractor_Update01s_Post(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                suppressBlocksOnChange = false;
                SendUpdateStacksAndContentData(coords, blocksOnChangeCalledWhileSuppressed);
                if (blocksOnChangeCalledWhileSuppressed)
                {
                    Haxx.SBlocks_OnChangeItem(coords, false, false, true);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractorDeep), nameof(CItem_ContentExtractorDeep.Update01s))]
        static bool Patch_CItem_ContentExtractorDeep_Update01s_Pre()
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                // vanilla calls SBlocks.OnChangeItem which my trigger other messages before we send
                // the content data update message, so defer it and call SBlocks.OnChangeItem in post.
                suppressBlocksOnChange = true;
                blocksOnChangeCalledWhileSuppressed = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractorDeep), nameof(CItem_ContentExtractorDeep.Update01s))]
        static void Patch_CItem_ContentExtractorDeep_Update01s_Post(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                suppressBlocksOnChange = false;
                SendUpdateStacksAndContentData(coords, blocksOnChangeCalledWhileSuppressed);
                if (blocksOnChangeCalledWhileSuppressed)
                {
                    Haxx.SBlocks_OnChangeItem(coords, false, false, true);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static bool Patch_CItem_ContentFactory_Update01s_Pre()
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                // vanilla calls SBlocks.OnChangeItem which my trigger other messages before we send
                // the content data update message, so defer it and call SBlocks.OnChangeItem in post.
                suppressBlocksOnChange = true;
                blocksOnChangeCalledWhileSuppressed = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static void Patch_CItem_ContentFactory_Update01s_Post(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                suppressBlocksOnChange = false;
                SendUpdateStacksAndContentData(coords, blocksOnChangeCalledWhileSuppressed);
                if (blocksOnChangeCalledWhileSuppressed)
                {
                    Haxx.SBlocks_OnChangeItem(coords, false, false, true);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentGreenHouse), nameof(CItem_ContentGreenHouse.Update01s))]
        static bool Patch_CItem_ContentGreenHouse_Update01s_Pre(CItem_ContentGreenHouse __instance, int2 coords, out int __state)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                __state = 0;
                return false;
            }
            // we need to detect the progress change to send the client about the gas stats
            __state = __instance.dataProgress.GetValue(coords);
            // in multiplayer, the Patch_CItem_ContentFactory will handle this
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentGreenHouse), nameof(CItem_ContentGreenHouse.Update01s))]
        static void Patch_CItem_ContentGreenHouse_Update01s_Post(CItem_ContentGreenHouse __instance, int2 coords, int __state)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var after = __instance.dataProgress.GetValue(coords);
                if (__state != after && after == 0)
                {
                    var msg = new MessageUpdatePlanetGasses();
                    msg.GetSnapshot();
                    SendAllClients(msg);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentForest), nameof(CItem_ContentForest.Update10s_Planet))]
        static bool Patch_CItem_ContentForest_Update10s_Planet_Pre(CItem_ContentForest __instance, int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                // vanilla calls SBlocks.OnChangeItem which my trigger other messages before we send
                // the content data update message, so defer it and call SBlocks.OnChangeItem in post.
                suppressBlocksOnChange = true;
                blocksOnChangeCalledWhileSuppressed = false;
                worldTexturesToUpdate.Clear();
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentForest), nameof(CItem_ContentForest.Update10s_Planet))]
        static void Patch_CItem_ContentForest_Update10s_Planet_Post(CItem_ContentForest __instance, int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                suppressBlocksOnChange = false;

                var msg = new MessageUpdateForest();
                msg.GetSnapshot(coords, worldTexturesToUpdate);
                SendAllClients(msg);

                SViewWorld sViewWorld = SSingleton<SViewWorld>.Inst;

                foreach (var c in worldTexturesToUpdate)
                {
                    Haxx.SBlocks_OnChangeItem(c, false, false, false);
                    sViewWorld.OnBuildItem_UpdateTxWorld(c);
                }

                if (blocksOnChangeCalledWhileSuppressed)
                {

                    Haxx.SBlocks_OnChangeItem(coords, false, false, true);
                }
            }
        }

        static readonly Dictionary<int2, float> extractorMainAngles = new();
        static readonly Dictionary<int2, float> extractorBucketAngles = new();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update_IfVisible))]
        static bool Patch_CItem_ContentExtractor_Update_IfVisible(
            CItem_ContentExtractor __instance,
            in int2 coords, List<Transform> modelsTrs)
        {
            // prevent jittering due to having the arm position as a dataArmAngle
            if (multiplayerMode == MultiplayerMode.Client)
            {
                Transform child = modelsTrs[0].GetChild(0);

                int targetValue = __instance.dataArmAngle.GetValue(coords);
                var targetAngle = 360f * targetValue / 63f;

                // get the current angle or set it to the target angle the first time
                if (!extractorMainAngles.TryGetValue(coords, out var currentAngle))
                {
                    currentAngle = targetAngle;
                    extractorMainAngles[coords] = targetAngle;
                }

                var newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Time.deltaTime * __instance.animArmSpeed);
                newAngle = SMisc.Mod(newAngle, 360f);
                child.localEulerAngles = new Vector3(0f, newAngle, 0f);
                extractorMainAngles[coords] = newAngle;

                var bucket = child.GetChild(0);
                extractorBucketAngles.TryGetValue(coords, out var currentBucketAngle);

                if (GHexes.water[coords.x, coords.y] < GItems.waterLevelStopBuildings 
                    && SSingleton<SWorld>.Inst.GetGround(coords) is CItem_GroundMineral 
                    && GHexes.groundData[coords.x, coords.y] > 0 
                    && __instance.GetStack(coords, 0).nb < __instance.GetStack(coords, 0).nbMax)
                {
                    // remember the bucket's rotation angle too.

                    var newBucketAngle = currentBucketAngle - Time.deltaTime * __instance.animSpeedWheel;
                    bucket.localEulerAngles = new Vector3(newBucketAngle, 0, 0);
                    extractorBucketAngles[coords] = newBucketAngle;

                    // bucket.Rotate(Vector3.right, -Time.deltaTime * __instance.animSpeedWheel, Space.Self);
                    CSoundLooped sound = __instance.sound;
                    sound?.Play(GHexes.Pos(coords), 1f, 1);
                }
                else
                {
                    // just restore the last angle when it was moving
                    bucket.localEulerAngles = new Vector3(currentBucketAngle, 0, 0);
                }

                return false;
            }
            return true;
        }

        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        static void ReceiveMessageUpdateContentData(MessageUpdateContentData msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateContentData: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateContentData: Handling " + msg.GetType());

                msg.ApplySnapshot();

                var content = ContentAt(msg.coords);
                if (content is CItem_ContentCityInOut inout)
                {
                    CCityInOutData inOutData = inout.GetInOutData(msg.coords, false);
                    int num = inOutData?.recipeIndex ?? 0;
                    inout.ChangeRecipeIFN(msg.coords, num);
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateContentData: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        public static bool logDebugStacksAndContentMessages;

        static void ReceiveMessageUpdateDatasAt(MessageUpdateDatasAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugStacksAndContentMessages)
                {
                    LogDebug("ReceiveMessageUpdateStacksAndContentDataAt: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugStacksAndContentMessages)
                {
                    LogDebug("ReceiveMessageUpdateStacksAndContentDataAt: Handling " + msg.GetType());
                }

                msg.ApplySnapshot();

                var coords = msg.coords;

                var content = ContentAt(coords);
                if (content is CItem_ContentExtractor)
                {
                    if (GHexes.groundData[coords.x, coords.y] == 0)
                    {
                        GItems.itemDirt.Create(coords, true);
                        SSingleton<SViewWorld>.Inst.OnBuildItem_UpdateTxWorld(coords);
                        SSingleton<SViewWorld>.Inst.OnAltitudeTxChange(coords);
                    }
                }

                if (msg.updateBlocks)
                {
                    Haxx.SBlocks_OnChangeItem(msg.coords, false, false, true);
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateStacksAndContentDataAt: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdatePlanetGasses(MessageUpdatePlanetGasses msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdatePlanetGasses: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdatePlanetGasses: Handling " + msg.GetType());

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdatePlanetGasses: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateForest(MessageUpdateForest msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateForest: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateForest: Handling " + msg.GetType());

                msg.ApplySnapshot();

                SViewWorld sViewWorld = SSingleton<SViewWorld>.Inst;

                foreach (var c in msg.contents)
                {
                    Haxx.SBlocks_OnChangeItem(c.coords, false, false, false);
                    sViewWorld.OnBuildItem_UpdateTxWorld(c.coords);
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateForest: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        public static bool logDebugUpdateItemsMessage;

        static void ReceiveMessageUpdateItems(MessageUpdateItems msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugUpdateItemsMessage)
                {
                    LogDebug("ReceiveMessageUpdateItems: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugUpdateItemsMessage)
                {
                    LogDebug("ReceiveMessageUpdateItems: Handling " + msg.GetType());
                }

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateItems: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
