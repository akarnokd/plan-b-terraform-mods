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

        void InitMessageDispatcher()
        {
            AddMessageRegistry<MessageLogin>(ReceiveMessageLogin);
            AddMessageRegistry<MessageLoginResponse>(ReceiveMessageLoginResponse);
        }

        static void AddMessageRegistry<T>(Action<T> handler) where T : MessageBase, new()
        {
            var m = new T();
            m.onReceive = m => handler((T)m);
            messageRegistry.Add(m.MessageCode(), m);
        }

        static void DispatchMessageLoop()
        {
            while (multiplayerMode == MultiplayerMode.Host 
                || multiplayerMode == MultiplayerMode.Client
                || multiplayerMode == MultiplayerMode.ClientLogin
                || multiplayerMode == MultiplayerMode.ClientLoading) 
            {

                try
                {
                    if (receiverQueue.TryDequeue(out var receiver))
                    {
                        receiver.onReceive(receiver);
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
