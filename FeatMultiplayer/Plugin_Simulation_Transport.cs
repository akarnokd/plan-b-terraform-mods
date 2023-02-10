using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CDrone), "ChangeState_Taking")]
        static void Patch_CDrone_ChangeState_Taking(
            ref CDrone.TransportStep ___takeFrom,
            ref CDrone.TransportStep ___giveTo)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateTransportStacks();
                msg.GetSnapshot(___takeFrom, ___giveTo,
                    MessageUpdateTransportStacks.All
                );
                SendAllClients(msg);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CDrone), "ChangeState_ReturningHome")]
        static void Patch_CDrone_ChangeState_ReturningHome(
            ref CDrone.TransportStep ___takeFrom,
            ref CDrone.TransportStep ___giveTo)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateTransportStacks();
                msg.GetSnapshot(___takeFrom, ___giveTo,
                    MessageUpdateTransportStacks.TakeFromValid
                    + MessageUpdateTransportStacks.GiveToValid
                    + MessageUpdateTransportStacks.UpdateTo
                );
                SendAllClients(msg);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CDrone), "CancellingMove")]
        static void Patch_CDrone_CancellingMove(
            bool takeFromValid, 
            bool giveToValid,
            ref CDrone.TransportStep ___takeFrom,
            ref CDrone.TransportStep ___giveTo)
        {
            if (multiplayerMode == MultiplayerMode.Host && (takeFromValid || giveToValid))
            {
                var msg = new MessageUpdateTransportStacks();
                msg.GetSnapshot(___takeFrom, ___giveTo, 
                    (byte)(
                    (takeFromValid ? MessageUpdateTransportStacks.TakeFromValid : 0) 
                    + (giveToValid ? MessageUpdateTransportStacks.GiveToValid : 0)
                    + MessageUpdateTransportStacks.UpdateFrom
                    )
                );
                SendAllClients(msg);
            }
        }

        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        static void ReceiveMessageUpdateLines(MessageUpdateLines msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateLines: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateLines: Handling " + msg.GetType());

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateLines: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateDrones(MessageUpdateDrones msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateDrones: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateDrones: Handling " + msg.GetType());

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateDrones: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateTransportStacks(MessageUpdateTransportStacks msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateTransportStacks: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateTransportStacks: Handling " + msg.GetType());

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateTransportStacks: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        
    }
}
