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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static bool Patch_SWays_CreateLine_Pre(CLine line, CLine lineOld)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionFinishLine();
                msg.newLine.GetSnapshot(line);
                msg.oldLineId = lineOld?.id ?? -1;
                SendHost(msg);

                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static void Patch_SWays_CreateLine_Post(CLine line, CLine lineOld)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(line, true);
                SendAllClients(msg);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_WayStop), nameof(CItem_WayStop.StartBuildMode))]
        static bool Patch_CItem_WayStop_StartBuildMode_Pre(int2 coordsOrigin, bool isReverse)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionBeginLine();
                msg.coords = coordsOrigin;
                msg.reverse = isReverse;
                SendHost(msg);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_WayStop), nameof(CItem_WayStop.StartBuildMode))]
        static bool Patch_CItem_WayStop_StartBuildMode_Post(int2 coordsOrigin, 
            bool isReverse, CLine ____buildLine)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(____buildLine, false);
                SendAllClients(msg);
                return false;
            }
            return true;
        }

        static void ReceiveMessageActionBeginLine(MessageActionBeginLine msg)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionBeginLine: Handling " + msg.GetType());

                var response = new MessageUpdateStartLine();
                response.lineId = ++CLine.idMax; // reserve an id
                response.coords = msg.coords;
                response.reverse = msg.reverse;

                msg.sender.Send(response);
            }
            else
            {
                LogWarning("ReceiveMessageActionBeginLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateStartLine(MessageUpdateStartLine msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateStartLine: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateStartLine: Handling " + msg.GetType());

                var content = ContentAt(msg.coords);

                if (content is CItem_WayStop stop)
                {
                    var cline = new CLine(msg.coords);
                    cline.id = msg.lineId;

                    Haxx.cItemWayStopBuildLine(stop) = cline;
                    Haxx.cItemWayStopIsReverse(stop) = msg.reverse;
                    Haxx.cItemWayStopBuildModeLastFrame(stop) = Time.frameCount;
                }
                else
                {
                    LogWarning("ReceiveMessageUpdateStartLine: CItem_WayStop not found at " 
                        + msg.coords.x + ", " + msg.coords.y + "; found " + (content?.codeName ?? "null"));
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateStartLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageActionFinishLine(MessageActionFinishLine msg)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionFinishLine: Handling " + msg.GetType());

                var lineLookup = GetLineDictionary();
                var itemLookup = GetItemsDictionary();

                var cline = new CLine(msg.newLine.stops[0].coords);
                msg.newLine.ApplySnapshot(cline, itemLookup);

                int numVehiclesToCopy;
                if (lineLookup.TryGetValue(msg.oldLineId, out var oldLine))
                {
                    cline.itemTransported = oldLine.itemTransported;
                    numVehiclesToCopy = oldLine.vehicles.Count;

                    SSingleton<SWays>.Inst.RemoveLine(oldLine);
                }
                else
                {
                    numVehiclesToCopy = Mathf.Clamp(cline.ItemVehicle.nbOwned, 0, 1);
                }

                GWays.lines.Add(cline);
                cline.ComputePath_Positions(true);
                cline.ChangeNbVehicles(numVehiclesToCopy);
                cline.UpdateStopDataOrginEnd(true, false);

                var msgResponse = new MessageUpdateFinishLine();
                msgResponse.pickItem = cline.itemTransported == null || oldLine == null;
                msgResponse.line.GetSnapshot(cline);
                msg.sender.Send(msgResponse);

                var msgRest = new MessageUpdateLine();
                msgRest.GetSnapshot(cline, true);

                SendAllClientsExcept(msg.sender, msgRest);
            }
            else
            {
                LogWarning("ReceiveMessageActionFinishLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateFinishLine(MessageUpdateFinishLine msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("MessageUpdateFinishLine: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("MessageUpdateFinishLine: Handling " + msg.GetType());

                var itemLookup = GetItemsDictionary();

                var cline = new CLine(msg.line.stops[0].coords);
                msg.line.ApplySnapshot(cline, itemLookup);
                GWays.lines.Add(cline);
                cline.ComputePath_Positions(true);
                cline.UpdateStopDataOrginEnd(true, false);

                SSceneSingleton<SSceneHud_ItemsBars>.Inst.TryCancel(false);
                SSceneSingleton<SSceneHud_Selection>.Inst.RefreshSelectionPanel(true);

                if (msg.pickItem)
                {
                    SSceneSingleton<SScenePopup>.Inst.ActivateAndShow(false, true, SLoc.Get("Popup_ItemPicking"), null);
                    SSceneSingleton<SScenePopup>.Inst.ShowItemsPickUp(cline.stops[0].coords);
                }
            }
            else
            {
                LogWarning("MessageUpdateFinishLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
