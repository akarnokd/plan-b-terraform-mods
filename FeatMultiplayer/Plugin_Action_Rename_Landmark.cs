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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud_Selection), "OnValueChange_Input")]
        static void Patch_SSceneHud_Selection_OnValueChange_Input(string value)
        {
            if (multiplayerMode == MultiplayerMode.Client || multiplayerMode == MultiplayerMode.Host)
            {
                var coords = GScene3D.selectionCoords;
                if (SSingleton<SWorld>.Inst.GetContent(coords) is CItem_ContentLandmark)
                {
                    var msg = new MessageActionRenameLandmark();
                    msg.coords = coords;
                    msg.name = value;
                    if (multiplayerMode == MultiplayerMode.Client)
                    {
                        SendHost(msg);
                    }
                    else
                    {
                        SendAllClients(msg);
                    }
                    LogDebug("MessageActionRenameLandmark: Request at " + msg.coords.x + ", " + msg.coords.y);
                }
            }
        }

        static void ReceiveMessageActionRenameLandmark(MessageActionRenameLandmark msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionRenameLandmark: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageActionRenameLandmark: Handling " + msg.GetType());

                GGame.dicoLandmarks[msg.coords] = msg.name;
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    SendAllClientsExcept(msg.sender, msg);
                }
            }
        }
    }
}
