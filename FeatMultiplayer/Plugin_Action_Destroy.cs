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
                        content.Destroy(msg.coords);
                        content.nbOwned++;
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
