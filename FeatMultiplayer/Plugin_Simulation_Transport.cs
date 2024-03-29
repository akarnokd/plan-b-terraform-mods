﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CDrone), nameof(CDrone.OnBuildingStackItemChanged))]
        static bool Patch_CDrone_OnBuildingStackItemChanged()
        {
            return multiplayerMode != MultiplayerMode.Client;
        }

        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        public static bool logDebugDroneMessages;
        public static bool logDebugLineMessages;

        static void ReceiveMessageUpdateLines(MessageUpdateLines msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugLineMessages)
                {
                    LogDebug("ReceiveMessageUpdateLines: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugLineMessages)
                {
                    LogDebug("ReceiveMessageUpdateLines: Handling " + msg.GetType());
                }

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
                if (logDebugDroneMessages)
                {
                    LogDebug("ReceiveMessageUpdateDrones: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugDroneMessages)
                {
                    LogDebug("ReceiveMessageUpdateDrones: Handling " + msg.GetType());
                }

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("ReceiveMessageUpdateDrones: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        public static bool logDebugTransportStacks;

        static void ReceiveMessageUpdateTransportStacks(MessageUpdateTransportStacks msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugTransportStacks)
                {
                    LogDebug("ReceiveMessageUpdateTransportStacks: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugTransportStacks)
                {
                    LogDebug("ReceiveMessageUpdateTransportStacks: Handling " + msg.GetType());
                }

                msg.ApplySnapshot();

                /*
                var content = ContentAt(msg.giveTo.coords);
                if (content is CItem_ContentCityInOut)
                {
                    LogDebug("ReceiveMessageUpdateTransportStacks\r\n    " + msg.giveTo);
                }
                */
            }
            else
            {
                LogWarning("ReceiveMessageUpdateTransportStacks: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        
    }
}
