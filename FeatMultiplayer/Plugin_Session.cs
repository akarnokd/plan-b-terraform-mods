using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// The client name joining a game. Used for logging into dedicated files.
        /// </summary>
        static volatile string clientName = "";

        static readonly Dictionary<string, ClientSession> loggedInClients = new();
    }

    /// <summary>
    /// Represents all information regarding a connecting or connected client.
    /// </summary>
    public class ClientSession
    {
        public readonly int id;

        public volatile string clientName;

        public volatile bool loginSuccess;

        public TcpClient tcpClient;

        public readonly CancellationToken disconnectToken = new();

        internal readonly ConcurrentQueue<BaseMessage> senderQueue = new();

        internal readonly AutoResetEvent signal = new(false);

        public ClientSession(int id) 
        {
            this.id = id;
        }

        public void Send(BaseMessage message, bool signal = true)
        {
            senderQueue.Enqueue(message);
            if (signal)
            {
                this.signal.Set();
            }
        }
    }
}
