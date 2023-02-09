using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static readonly Dictionary<string, MessageBase> messageRegistry = new Dictionary<string, MessageBase>();

        static readonly Queue<MessageBase> deferredMessages = new();

        void InitMessageDispatcher()
        {
            AddMessageRegistry<MessageLogin>(ReceiveMessageLogin);
            AddMessageRegistry<MessageLoginResponse>(ReceiveMessageLoginResponse);

            AddMessageRegistry<MessageSyncAllAltitude>(ReceiveMessageSyncAllAltitude);
            AddMessageRegistry<MessageSyncAllContentData>(ReceiveMessageSyncAllContentData);
            AddMessageRegistry<MessageSyncAllContentId>(ReceiveMessageSyncAllContentId);
            AddMessageRegistry<MessageSyncAllFlags>(ReceiveMessageSyncAllFlags);
            AddMessageRegistry<MessageSyncAllGroundData>(ReceiveMessageSyncAllGroundData);
            AddMessageRegistry<MessageSyncAllGroundId>(ReceiveMessageSyncAllGroundId);
            AddMessageRegistry<MessageSyncAllWater>(ReceiveMessageSyncAllWater);

            AddMessageRegistry<MessageSyncAllMain>(ReceiveMessageSyncAllMain);
            AddMessageRegistry<MessageSyncAllGame>(ReceiveMessageSyncAllGame);
            AddMessageRegistry<MessageSyncAllPlanet>(ReceiveMessageSyncAllPlanet);
            AddMessageRegistry<MessageSyncAllItems>(ReceiveMessageSyncAllItems);
            AddMessageRegistry<MessageSyncAllWaterInfo>(ReceiveMessageSyncAllWaterInfo);
            AddMessageRegistry<MessageSyncAllDrones>(ReceiveMessageSyncAllDrones);
            AddMessageRegistry<MessageSyncAllWays>(ReceiveMessageSyncAllWays);
            AddMessageRegistry<MessageSyncAllCamera>(ReceiveMessageSyncAllCamera);
        }

        static void AddMessageRegistry<T>(Action<T> handler) where T : MessageBase, new()
        {
            var m = new T();
            m.onReceive = m => handler((T)m);
            messageRegistry.Add(m.MessageCode(), m);
        }

        static void DispatchMessageLoop()
        {
            // process messages that were deferred during the ClientLoading phase
            if (multiplayerMode == MultiplayerMode.Client)
            {
                while (deferredMessages.Count != 0)
                {
                    var m = deferredMessages.Dequeue();
                    m.onReceive(m);
                }
            }
            while (multiplayerMode == MultiplayerMode.Host 
                || multiplayerMode == MultiplayerMode.Client
                || multiplayerMode == MultiplayerMode.ClientJoin) 
            {

                try
                {
                    if (receiverQueue.TryDequeue(out var receiver))
                    {
                        receiver.onReceive(receiver);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }
    }
}
