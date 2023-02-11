// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;

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
                if (ContentAt(coords) is CItem_ContentLandmark)
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
