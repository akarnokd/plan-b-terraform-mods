﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressDestroyNotification;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Content), "Destroy")]
        static bool Patch_CItem_Content_Destroy_Pre(CItem_Content __instance, int2 coords)
        {
            if (!suppressDestroyNotification && multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionDestroy();
                msg.coords = coords;
                SendHost(msg);
                LogDebug("MessageActionDestroy: Request at " + msg.coords.x + ", " + msg.coords.y);
                
                __instance.nbOwned--; // the caller in SScene3D.OnUpdate always increments
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Content), "Destroy")]
        static void Patch_CItem_Content_Destroy_Post(int2 coords)
        {
            if (!suppressDestroyNotification && multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageActionDestroy();
                msg.coords = coords;
                SendAllClients(msg);
                LogDebug("MessageActionDestroy: Command at " + msg.coords.x + ", " + msg.coords.y);
            }
        }

        // we need to override because the vanilla nulls out the stock field and causes NRE.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentStock), "Destroy")]
        static bool Patch_CItem_ContentStock_Destroy_Pre(CItem_ContentStock __instance, int2 coords)
        {
            if (!suppressDestroyNotification && multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionDestroy();
                msg.coords = coords;
                SendHost(msg);
                LogDebug("MessageActionDestroy: Request at " + msg.coords.x + ", " + msg.coords.y);

                __instance.nbOwned--; // the caller in SScene3D.OnUpdate always increments
                return false;
            }
            return true;
        }

        static void ReceiveMessageActionDestroy(MessageActionDestroy msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionDestroy: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageActionDestroy: Handling " + msg.GetType());
                var content = ContentAt(msg.coords);
                if (content != null)
                {
                    suppressDestroyNotification = multiplayerMode == MultiplayerMode.Client;
                    try
                    {
                        LogDebug("ReceiveMessageActionDestroy: Destroying " + content.codeName + " at " + msg.coords.x + ", " + msg.coords.y);
                        if (GScene3D.selectionCoords == msg.coords)
                        {
                            GScene3D.selectionCoords = int2.negative;
                            GScene3D.selectedItem = null;
                        }

                        content.Destroy(msg.coords);
                        content.nbOwned++;

                        if (content is CItem_ContentExtractor)
                        {
                            extractorMainAngles.Remove(msg.coords);
                            extractorBucketAngles.Remove(msg.coords);
                        }
                    }
                    finally
                    {
                        suppressDestroyNotification = false;
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageActionDestroy: CItem_Content not found at " + msg.coords.x + ", " + msg.coords.y);
                }
            }
        }
    }
}
