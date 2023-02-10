using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressBlocksOnChange;

        static bool blocksOnChangeCalledWhileSuppressed;

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

        static void SendUpdateContentData(int2 coords)
        {
            var msg = new MessageUpdateContentData();
            msg.GetSnapshot(coords);
            SendAllClients(msg);
        }

        static void SendUpdateGroundAndContentData(int2 coords, bool updateBlocks)
        {
            var msg = new MessageUpdateDatasAt();
            msg.GetSnapshot(coords, updateBlocks);
            SendAllClients(msg);
        }

        static void SendUpdateStacksAndContentData(int2 coords, bool updateBlocks)
        {
            var msg = new MessageUpdateStacksAndContentDataAt();
            msg.GetSnapshot(coords, updateBlocks);
            SendAllClients(msg);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.Update01s))]
        static bool Patch_CItem_ContentCityInOut_Update01s_Pre(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.Update01s))]
        static void Patch_CItem_ContentCityInOut_Update01s_Post(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                SendUpdateContentData(coords);
            }
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
                SendUpdateGroundAndContentData(coords, blocksOnChangeCalledWhileSuppressed);
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
                SendUpdateGroundAndContentData(coords, blocksOnChangeCalledWhileSuppressed);
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

                var content = ContentAt(msg.coords);
                if (content is CItem_ContentCityInOut inout)
                {
                    CCityInOutData inOutData = inout.GetInOutData(msg.coords, true);
                    int num = inOutData?.recipeIndex ?? 0;
                    inout.ChangeRecipeIFN(msg.coords, num);
                }

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateContentData: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateGroundAndContentData(MessageUpdateDatasAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateGroundAndContentData: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateGroundAndContentData: Handling " + msg.GetType());

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
                    if (msg.updateBlocks)
                    {
                        Haxx.SBlocks_OnChangeItem(coords, false, false, true);
                    }
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateGroundAndContentData: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateStacksAndContentDataAt(MessageUpdateStacksAndContentDataAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateStacksAndContentDataAt: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateStacksAndContentDataAt: Handling " + msg.GetType());

                msg.ApplySnapshot();

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
    }
}
