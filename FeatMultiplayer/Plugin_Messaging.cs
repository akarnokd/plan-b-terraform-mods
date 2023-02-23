// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Registry for message decoders and receive actions.
        /// </summary>
        static readonly Dictionary<string, MessageBase> messageRegistry = new();

        /// <summary>
        /// Holds live update messages that arrive during the initial full sync, to be
        /// replayed right after the client entered the game world.
        /// </summary>
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

            AddMessageRegistry<MessageActionBuild>(ReceiveMessageActionBuild);
            AddMessageRegistry<MessageActionDestroy>(ReceiveMessageActionDestroy);
            AddMessageRegistry<MessageActionRenameLandmark>(ReceiveMessageActionRenameLandmark);
            AddMessageRegistry<MessageActionReverseLine>(ReceiveMessageActionReverseLine);
            AddMessageRegistry<MessageActionCopy>(ReceiveMessageActionCopy);
            AddMessageRegistry<MessageUpdateDepotDrones>(ReceiveMessageUpdateDepotDrones);

            AddMessageRegistry<MessageUpdateStackAt>(ReceiveMessageUpdateStackAt);
            AddMessageRegistry<MessageUpdateStacksAt>(ReceiveMessageUpdateStacksAt);
            AddMessageRegistry<MessageUpdateRecipeAt>(ReceiveMessageUpdateRecipeAt);
            AddMessageRegistry<MessageUpdateTransportedAt>(ReceiveMessageUpdateTransportedAt);

            AddMessageRegistry<MessageUpdateContentData>(ReceiveMessageUpdateContentData);
            AddMessageRegistry<MessageUpdateDatasAt>(ReceiveMessageUpdateDatasAt);
            AddMessageRegistry<MessageUpdatePlanetGasses>(ReceiveMessageUpdatePlanetGasses);
            AddMessageRegistry<MessageUpdateForest>(ReceiveMessageUpdateForest);
            AddMessageRegistry<MessageUpdateCity>(ReceiveMessageUpdateCity);

            AddMessageRegistry<MessageUpdateTime>(ReceiveMessageUpdateTime);
            AddMessageRegistry<MessageUpdatePlanet>(ReceiveMessageUpdatePlanet);
            AddMessageRegistry<MessageUpdateLine>(ReceiveMessageUpdateLine);
            AddMessageRegistry<MessageUpdateLines>(ReceiveMessageUpdateLines);
            AddMessageRegistry<MessageUpdateDrones>(ReceiveMessageUpdateDrones);
            AddMessageRegistry<MessageUpdateTransportStacks>(ReceiveMessageUpdateTransportStacks);
            AddMessageRegistry<MessageUpdateItems>(ReceiveMessageUpdateItems);

            AddMessageRegistry<MessageActionBeginLine>(ReceiveMessageActionBeginLine);
            AddMessageRegistry<MessageUpdateStartLine>(ReceiveMessageUpdateStartLine);
            AddMessageRegistry<MessageActionFinishLine>(ReceiveMessageActionFinishLine);
            AddMessageRegistry<MessageUpdateFinishLine>(ReceiveMessageUpdateFinishLine);
            AddMessageRegistry<MessageActionRemoveLine>(ReceiveMessageActionRemoveLine);
            AddMessageRegistry<MessageActionChangeVehicleCount>(ReceiveMessageActionChangeVehicleCount);
        }

        static void AddMessageRegistry<T>(Action<T> handler) where T : MessageBase, new()
        {
            var m = new T();

            if (!m.GetType().Name.EndsWith(m.MessageCode()))
            {
                throw new InvalidOperationException("MessageCode (by convention) mismatch: " + m.GetType().Name + " vs *" + m.MessageCode());
            }

            m.onReceive = m => handler((T)m);
            messageRegistry.Add(m.MessageCode(), m);
        }

        static void DispatchMessageLoop()
        {
            // process messages that were deferred during the ClientJoin phase
            if (multiplayerMode == MultiplayerMode.Client)
            {
                while (deferredMessages.Count != 0)
                {
                    var m = deferredMessages.Dequeue();
                    messageTelemetry.GetAndReset();
                    m.onReceive(m);
                    messageTelemetry.AddTelemetry(m.MessageCode());
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
                        messageTelemetry.GetAndReset();
                        receiver.onReceive(receiver);
                        messageTelemetry.AddTelemetry(receiver.MessageCode());
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

        static readonly MessageTelemetry messageTelemetry = new("ReceiveMessage");
    }


    internal class MessageTelemetry
    {

        public static bool isEnabled = true;

        internal string name;

        internal readonly Dictionary<string, int> counts = new();
        internal readonly Dictionary<string, long> timesInTicks = new();

        internal long logTelemetry = 30000;
        internal Stopwatch stopWatch = new();

        internal Stopwatch timer = new();

        internal MessageTelemetry(string name)
        {
            this.name = name;
        }

        internal void Start()
        {
            stopWatch.Start();
        }

        internal long GetAndReset()
        {
            long v = timer.ElapsedTicks;
            timer.Restart();
            return v;
        }

        internal void AddTelemetry(string message)
        {
            if (isEnabled)
            {
                long length = GetAndReset();

                counts.TryGetValue(message, out var c);
                counts[message] = c + 1;
                timesInTicks.TryGetValue(message, out var t);
                timesInTicks[message] = t + length;

                var n = stopWatch.ElapsedMilliseconds;
                if (n >= logTelemetry)
                {
                    StringBuilder sb = new();
                    sb.Append("Telemetry < ").Append(name).Append(" >");

                    long sumCounts = 0;
                    long sumTicks = 0;

                    var pad = counts.Keys.Select(k => k.Length).Max();

                    foreach (var k in counts.Keys)
                    {
                        var bs = counts[k];
                        var ticks = timesInTicks[k];

                        sumCounts += bs;
                        sumTicks += ticks;

                        sb.AppendLine()
                            .Append("    ").Append(k.PadRight(pad)).Append(" x ").AppendFormat("{0,8}", bs)
                            .Append(" ~~~~ ").Append(string.Format("{0:#,##0.000} ms", ticks / 10000d).PadLeft(16))
                            .Append(" :::: ").Append(string.Format("{0:#,##0.000} ms / msg", ticks / 10000d / bs).PadLeft(16));
                    }

                    sb.AppendLine()
                    .Append("    =====").AppendLine()
                            .Append("    ").Append("Total".PadRight(pad)).Append(" x ").AppendFormat("{0,8}", sumCounts)
                            .Append(" ~~~~ ").Append(string.Format("{0:#,##0.000} ms", sumTicks / 10000d).PadLeft(16))
                            .Append(" :::: ").Append(string.Format("{0:#,##0.000} ms / msg", sumTicks / 10000d / sumCounts).PadLeft(16));

                    counts.Clear();
                    timesInTicks.Clear();

                    Plugin.LogDebug(sb.ToString());

                    stopWatch.Restart();
                }
            }
        }
    }
}
