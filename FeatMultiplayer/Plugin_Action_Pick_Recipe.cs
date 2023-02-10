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
        [HarmonyPatch(typeof(SScenePopup), "OnClickItem")]
        static bool Patch_SScenePopup_OnClickItem(SScenePopup __instance, CUiItem uiItem, ref int2 ____pickCoords)
        {
            if (multiplayerMode == MultiplayerMode.Client || multiplayerMode == MultiplayerMode.Host)
            {

                var content = ContentAt(____pickCoords); ;

                if (content is CItem_ContentDepot)
                {
                    var msg = new MessageUpdateStackAt();
                    msg.CreateRequest(____pickCoords, 0, uiItem.item, 0, 0);

                    if (multiplayerMode == MultiplayerMode.Client)
                    {
                        SendHost(msg);
                    }
                    else
                    {
                        SendAllClients(msg);
                    }
                }
                else if (content is CItem_ContentFactory)
                {
                    var msg = new MessageUpdateRecipeAt();
                    msg.CreateRequest(____pickCoords, uiItem.item.codeName);

                    if (multiplayerMode == MultiplayerMode.Client)
                    {
                        SendHost(msg);
                    }
                    else
                    {
                        SendAllClients(msg);
                    }
                }
                else if (content is CItem_WayStop)
                {
                    var msg = new MessageUpdateTransportedAt();
                    msg.CreateRequest(____pickCoords, uiItem.item.codeName);
                    if (multiplayerMode == MultiplayerMode.Client)
                    {
                        SendHost(msg);
                    }
                    else
                    {
                        SendAllClients(msg);
                    }
                }

                if (multiplayerMode == MultiplayerMode.Client)
                {
                    __instance.Deactivate();
                    __instance.Reset();
                    return false;
                }
            }
            return true;
        }

        static void PickRecipeNotifyBlocks(int2 coords)
        {
            // if currently looking at that particular spot
            if (GScene3D.selectionCoords == coords)
            {
                SSceneSingleton<SSceneHud_Selection>.Inst.RefreshSelectionPanel(true);
            }

            Haxx.SBlocks_OnChangeItem(coords, true, false, false);
        }

        static void ReceiveMessageUpdateStackAt(MessageUpdateStackAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateStackAt: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageUpdateStackAt: Handling " + msg.GetType());

                msg.ApplySnapshot();

                if (multiplayerMode == MultiplayerMode.Host)
                {
                    SendAllClientsExcept(msg.sender, msg);

                    for (int i = 0; i < GDrones.drones.Count; i++)
                    {
                        GDrones.drones[i].OnBuildingStackItemChanged(msg.coords);
                    }

                }

                PickRecipeNotifyBlocks(msg.coords);
            }
        }

        static void ReceiveMessageUpdateRecipeAt(MessageUpdateRecipeAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateRecipeAt: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageUpdateRecipeAt: Handling " + msg.GetType());

                msg.ApplySnapshot();

                if (multiplayerMode == MultiplayerMode.Host)
                {
                    SendAllClientsExcept(msg.sender, msg);
                }

                PickRecipeNotifyBlocks(msg.coords);
            }
        }

        static void ReceiveMessageUpdateTransportedAt(MessageUpdateTransportedAt msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateTransportedAt: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageUpdateTransportedAt: Handling " + msg.GetType());

                msg.ApplySnapshot();

                if (multiplayerMode == MultiplayerMode.Host)
                {
                    SendAllClientsExcept(msg.sender, msg);
                }

                PickRecipeNotifyBlocks(msg.coords);
            }
        }
    }
}
