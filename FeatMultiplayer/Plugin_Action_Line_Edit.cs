// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressLineCreateItemPicking;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static bool Patch_SWays_CreateLine_Pre(CLine line, CLine lineOld)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionFinishLine();
                msg.pickCoords = GScene3D.selectionCoords;
                msg.newLine.GetSnapshot(line);
                msg.oldLineId = lineOld?.id ?? -1;
                SendHost(msg);

                suppressLineCreateItemPicking = true;
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

        /*
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
        static void Patch_CItem_WayStop_StartBuildMode_Post(CLine ____buildLine)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(____buildLine, false);
                SendAllClients(msg);
            }
        }
        */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_WayStop), "Update_IfSelected")]
        static void Patch_CItem_WayStop_Update_IfSelected()
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                suppressLineCreateItemPicking = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SScenePopup), nameof(SScenePopup.ActivateAndShow))]
        static bool Patch_SScenePopup_ActivateAndShow()
        {
            return !suppressLineCreateItemPicking;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SScenePopup), nameof(SScenePopup.ShowItemsPickUp))]
        static bool Patch_SScenePopup_ShowItemsPickUp()
        {
            return !suppressLineCreateItemPicking;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.RemoveLine))]
        static bool Patch_SWays_RemoveLine_Pre(CLine line)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionRemoveLine();
                msg.lineId = line.id;
                SendHost(msg);
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.RemoveLine))]
        static void Patch_SWays_RemoveLine_Post(CLine line)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageActionRemoveLine();
                msg.lineId = line.id;
                SendAllClients(msg);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CLine), nameof(CLine.Inverse))]
        static bool Patch_CLine_Inverse_Pre(CLine __instance)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionReverseLine();
                msg.lineId = __instance.id;
                SendHost(msg);
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CLine), nameof(CLine.Inverse))]
        static void Patch_CLine_Inverse_Post(CLine __instance)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(__instance, false);
                SendAllClients(msg);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud_Selection), "OnClick_ModifiedValue")]
        static bool Patch_SSceneHud_Selection_OnClick_ModifiedValue_Pre(int nb)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var n = nb;
                if (GInputs.shift.IsKey())
                {
                    n *= 10;
                }
                var msg = new MessageActionChangeVehicleCount();
                msg.coords = GScene3D.selectionCoords;
                msg.delta = n;
                SendHost(msg);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud_Selection), "OnClick_ModifiedValue")]
        static void Patch_SSceneHud_Selection_OnClick_ModifiedValue_Post(int nb)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                if (GScene3D.selectedItem is CItem_WayStop)
                {
                    CLine line = SSingleton<SWays>.Inst.GetLine(GScene3D.selectionCoords);
                    if (line != null)
                    {
                        var msgResp = new MessageUpdateLine();
                        msgResp.line.GetSnapshot(line);
                        SendAllClients(msgResp);
                    }
                }
            }
        }

        // ------------------------------------------------------------------------------
        // Message receivers
        // ------------------------------------------------------------------------------

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
                msg.newLine.ApplySnapshot(cline, itemLookup, true);

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
                msgResponse.pickCoords = msg.pickCoords;
                msgResponse.oldLineId = msg.oldLineId;
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
                var lineLookup = GetLineDictionary();

                if (lineLookup.TryGetValue(msg.oldLineId, out var oldLine))
                {
                    oldLine.UpdateStopDataOrginEnd(true, true);
                    GWays.lines.Remove(oldLine);
                }

                var cline = new CLine(msg.line.stops[0].coords);
                msg.line.ApplySnapshot(cline, itemLookup, true);
                GWays.lines.Add(cline);
                cline.ComputePath_Positions(true);
                cline.UpdateStopDataOrginEnd(true, false);

                if (msg.pickItem)
                {
                    SSceneSingleton<SScenePopup>.Inst.ActivateAndShow(false, true, SLoc.Get("Popup_ItemPicking"), null);
                    SSceneSingleton<SScenePopup>.Inst.ShowItemsPickUp(msg.pickCoords);
                }
            }
            else
            {
                LogWarning("MessageUpdateFinishLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageActionRemoveLine(MessageActionRemoveLine msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionRemoveLine: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageActionRemoveLine: Handling " + msg.GetType());

                GWays.lines.RemoveAll(x =>
                {
                    if (x != null && x.id == msg.lineId)
                    {
                        CancelBuildLine(x);

                        x.UpdateStopDataOrginEnd(true, true);
                        return true;
                    }
                    return false;
                });
            }
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionRemoveLine: Handling " + msg.GetType());

                var lineLookup = GetLineDictionary();
                if (lineLookup.TryGetValue(msg.lineId, out var line))
                {
                    CancelBuildLine(line);
                    SSingleton<SWays>.Inst.RemoveLine(line);
                }
                else 
                {
                    LogWarning("ReceiveMessageActionRemoveLine: Unknown line " + msg.lineId);
                }
            }
            else
            {
                LogWarning("ReceiveMessageActionRemoveLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void CancelBuildLine(CLine line)
        {
            foreach (var stop in line.stops)
            {
                var content = ContentAt(stop.coords);
                if (content is CItem_WayStop wayStop)
                {
                    if (Haxx.cItemWayStopBuildLine.Invoke(wayStop) == line)
                    {
                        Haxx.cItemWayStopBuildLine.Invoke(wayStop) = null;
                    }
                }
            }
        }

        static void ReceiveMessageActionReverseLine(MessageActionReverseLine msg)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionReverseLine: Handling " + msg.GetType());

                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    CLine line = GWays.lines[i];
                    if (line.id == msg.lineId)
                    {
                        line.Inverse();

                        break;
                    }
                }
            }
            else
            {
                LogWarning("ReceiveMessageActionReverseLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateLine(MessageUpdateLine msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateLine: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateLine: Handling " + msg.GetType());

                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    CLine cline = GWays.lines[i];
                    if (cline.id == msg.line.id)
                    {
                        msg.ApplySnapshot(cline);

                        cline.UpdateStopDataOrginEnd(true, false);
                        cline.ComputePath_Positions(msg.computePath);

                        return;
                    }
                }

                // create a new line
                var line = new CLine(msg.line.stops[0].coords);
                msg.ApplySnapshot(line);
                GWays.lines.Add(line);
            }
            else
            {
                LogWarning("ReceiveMessageUpdateLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageActionChangeVehicleCount(MessageActionChangeVehicleCount msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionChangeVehicleCount: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionChangeVehicleCount: Handling " + msg.GetType());

                var content = ContentAt(msg.coords);
                if (content is CItem_WayStop)
                {
                    CLine line = SSingleton<SWays>.Inst.GetLine(msg.coords);
                    if (line != null)
                    {
                        int nb = msg.delta;
                        if (nb > 0)
                        {
                            nb = GGame.debugAllUnlocked ? nb : Mathf.Min(nb, line.ItemVehicle.nbOwned);
                        }
                        else if (nb < 0)
                        {
                            nb = Mathf.Min(nb, line.vehicles.Count);
                        }
                        line.ChangeNbVehicles(nb);

                        var msgResp = new MessageUpdateLine();
                        msgResp.line.GetSnapshot(line);
                        SendAllClients(msgResp);

                        if (GScene3D.selectionCoords == msg.coords)
                        {
                            SSceneSingleton<SSceneHud_Selection>.Inst.RefreshSelectionPanel(true);
                        }

                    }
                    else
                    {
                        LogWarning("ReceiveMessageActionChangeVehicleCount: No line at " + msg.coords.x + ", " + msg.coords.y);
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageActionChangeVehicleCount: CItem_WayStop " + msg.coords.x + ", " + msg.coords.y);
                }
            }
            else
            {
                LogWarning("ReceiveMessageActionChangeVehicleCount: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
