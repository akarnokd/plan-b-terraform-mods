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

        static string currentWayBuilt;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Way), nameof(CItem_Way.BuildWay))]
        static void Patch_CItem_WayStop_BuildWay(CItem_Way __instance)
        {
            currentWayBuilt = __instance.codeName;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static bool Patch_SWays_CreateLine_Pre(CLine line, CLine lineModified, CLine lineCopied)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionFinishLine();
                msg.pickCoords = GScene3D.selectionCoords;
                msg.newLine.GetSnapshot(line);
                msg.newLine.itemStopOrigin = currentWayBuilt ?? "";
                msg.lineModifiedId = lineModified?.id ?? -1;
                msg.lineCopiedId = lineCopied?.id ?? -1;
                SendHost(msg);

                suppressLineCreateItemPicking = true;
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SWays), nameof(SWays.CreateLine))]
        static void Patch_SWays_CreateLine_Post(CLine line, CLine lineModified, CLine lineCopied)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(line, true);
                SendAllClients(msg);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Vehicle), "Update_TryToBuild")]
        static void Patch_CItem_Vehicle_Update_TryToBuild()
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
                    var cline = new CLine(msg.coords, null);
                    cline.id = msg.lineId;
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

                var cline = new CLine(msg.newLine.stops[0].coords, null);
                msg.newLine.ApplySnapshot(cline, itemLookup, true);

                int numVehiclesToCopy;
                if (lineLookup.TryGetValue(msg.lineModifiedId, out var oldLine))
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
                msgResponse.lineModifiedId = msg.lineModifiedId;
                msgResponse.lineCopiedId = msg.lineCopiedId;
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

                if (lineLookup.TryGetValue(msg.lineModifiedId, out var oldLine))
                {
                    oldLine.UpdateStopDataOrginEnd(true, true);
                    GWays.lines.Remove(oldLine);
                }

                var cline = new CLine(msg.line.stops[0].coords, null);
                msg.line.ApplySnapshot(cline, itemLookup, true);
                GWays.lines.Add(cline);
                cline.ComputePath_Positions(true);
                cline.UpdateStopDataOrginEnd(true, false);

                SSceneSingleton<SSceneHud_ItemsBars>.Inst.TryCancel(false);

                GScene3D.selectionCoordsLastFrame = int2.negative;
                GScene3D.selectionCoords = msg.line.stops[1].coords;
                GScene3D.selectedItem = cline.ItemStop;

                SSceneSingleton<SSceneHud_Selection>.Inst.RefreshSelectionPanel(true);

                // LogDebug("msg.pickItem " + msg.pickItem);
                if (msg.pickItem)
                {
                    SSceneSingleton<SScenePopup>.Inst.ActivateAndShow(false, true, SLoc.Get("Popup_ItemPicking"), null);
                    SSceneSingleton<SScenePopup>.Inst.ShowItemsPickUp(msg.line.stops[1].coords);
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
                var line = new CLine(msg.line.stops[0].coords, null);
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
