// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        const int loopWakeupMillis = 500;
        const int sendBufferSize = 1024 * 1024;
        const bool fullResetBuffer = false;

        static CancellationTokenSource stopNetwork = new();
        static CancellationTokenSource stopHostAcceptor = new();

        static ConcurrentQueue<MessageBase> receiverQueue = new();

        static ClientSession hostSession;

        static int uniqueClientId;

        static readonly Dictionary<int, ClientSession> sessions = new();

        public static bool logDebugNetworkMessages;

        static Telemetry sendTelemetry = new("Send");
        static Telemetry receiveTelemetry = new("Receive");

        /// <summary>
        /// Send a message to the host.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signal"></param>
        public static void SendHost(MessageBase message, bool signal = true)
        {
            LogDebug("SendHost: " + message.GetType());
            hostSession?.Send(message, signal);
        }

        /// <summary>
        /// Send the same message to all clients.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signal"></param>
        public static void SendAllClients(MessageBase message, bool signal = true)
        {
            // No need for the log flood
            // LogDebug("SendAllClients: " + message.GetType());
            foreach (var sess in sessions.Values)
            {
                sess.Send(message, signal);
            }
        }

        /// <summary>
        /// Send the same message to all clients except one.
        /// This is needed when one client's message has to be routed to all other clients,
        /// but not back to itself.
        /// </summary>
        /// <param name="except"></param>
        /// <param name="message"></param>
        /// <param name="signal"></param>
        public static void SendAllClientsExcept(ClientSession except, MessageBase message, bool signal = true)
        {
            int id = except.id;
            LogDebug("SendAllClientsExcept: " + message.GetType() + " (except " + id + ")");
            foreach (var sess in sessions.Values)
            {
                if (sess.id != id)
                {
                    sess.Send(message, signal);
                }
            }
        }

        static void StartServer()
        {
            stopNetwork = new();
            stopHostAcceptor = new();
            Task.Factory.StartNew(HostAcceptor, TaskCreationOptions.LongRunning);
        }

        static void HostAcceptor()
        {
            var hostIp = hostServiceAddress.Value;
            IPAddress hostIPAddress = IPAddress.Any;
            if (hostIp == "")
            {
                hostIPAddress = IPAddress.Any;
            }
            else
            if (hostIp == "default")
            {
                hostIPAddress = GetMainIPv4();
            }
            else
            if (hostIp == "defaultv6")
            {
                hostIPAddress = GetMainIPv6();
            }
            else
            {
                try
                {
                    hostIPAddress = IPAddress.Parse(hostIp);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return;
                }
            }
            LogInfo("Starting HostAcceptor on " + hostIp + ":" + hostPort.Value + " (" + hostIPAddress + ")");
            try
            {
                TcpListener listener = new TcpListener(hostIPAddress, hostPort.Value);
                listener.Start();
                stopNetwork.Token.Register(listener.Stop);
                stopHostAcceptor.Token.Register(listener.Stop);
                try
                {
                    while (!stopNetwork.IsCancellationRequested && !stopHostAcceptor.IsCancellationRequested)
                    {
                        var client = listener.AcceptTcpClient();
                        ManageClient(client);
                    }
                }
                finally
                {
                    listener.Stop();
                    LogInfo("Stopping HostAcceptor on port " + hostPort.Value);
                }
            }
            catch (Exception ex)
            {
                if (!stopNetwork.IsCancellationRequested && !stopHostAcceptor.IsCancellationRequested)
                {
                    LogError(ex);
                }
            }
        }

        static void ManageClient(TcpClient client)
        {
            LogDebug("Accepting client from " + client.Client.RemoteEndPoint);
            var session = new ClientSession(Interlocked.Increment(ref uniqueClientId));
            session.tcpClient = client;
            sessions.Add(session.id, session);

            Task.Factory.StartNew(() => ReceiverLoop(session), TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => SenderLoop(session), TaskCreationOptions.LongRunning);
        }

        static void StartClient()
        {
            stopNetwork = new();
            hostSession = new ClientSession(0);
            hostSession.clientName = ""; // host

            Task.Run(ClientRunner);
        }

        static void ClientRunner()
        {
            LogInfo("Client connecting to " + clientConnectAddress.Value + ":" + clientPort.Value);

            try
            {
                var client = new TcpClient();
                hostSession.tcpClient = client;


                stopNetwork.Token.Register(client.Close);

                hostSession.tcpClient.Connect(clientConnectAddress.Value, clientPort.Value);
                LogInfo("Client connection success");

                Task.Factory.StartNew(() => ReceiverLoop(hostSession), TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() => SenderLoop(hostSession), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                var msg = new MessageLoginResponse();
                msg.reason = "Error_ConnectionRefused";
                receiverQueue.Enqueue(msg);

                if (!stopNetwork.IsCancellationRequested)
                {
                    LogError(ex);
                }
            }
        }

        static void SenderLoop(ClientSession session)
        {
            sendTelemetry.stopWatch.Start();

            var tcpClient = session.tcpClient;
            var stream = tcpClient.GetStream();

            LogDebug("SenderLoop Start for session " + session.id + " from " + tcpClient.Client.RemoteEndPoint);
            try
            {
                try
                {
                    var encodeBuffer = new MemoryStream(sendBufferSize);
                    var encodeWriter = new BinaryWriter(encodeBuffer, Encoding.UTF8);

                    while (!stopNetwork.IsCancellationRequested && !session.disconnectToken.IsCancellationRequested)
                    {
                        if (session.senderQueue.TryDequeue(out var msg))
                        {
                            if (msg == MessageDisconnect.Instance)
                            {
                                LogDebug("SenderLoop for session " + session.id + " < " + session.clientName + " > send message " + msg.MessageCode());
                                break;
                            }

                            if (logDebugNetworkMessages)
                            {
                                LogDebug("SenderLoop for session " + session.id + " < " + session.clientName + " > send message " + msg.MessageCode());
                            }
                            ClearMemoryStream(encodeBuffer, fullResetBuffer);

                            var code = msg.MessageCodeBytes();

                            // placeholder for the header sizes
                            encodeWriter.Write(0); // length of the message code + 1 for its size + the length of the message body
                            encodeWriter.Write((byte)code.Length);
                            encodeWriter.Write(code);

                            var pos = encodeBuffer.Position;
                            msg.Encode(encodeWriter);

                            var msgLength = (int)(encodeBuffer.Position - pos);
                            var messageTotalLength = code.Length + msgLength;
                            encodeBuffer.Position = 0;
                            encodeWriter.Write(messageTotalLength);
                            encodeBuffer.Position = 0;

                            if (logDebugNetworkMessages)
                            {
                                LogDebug("    Message sizes: 4 + 1 + " + code.Length + " + " + msgLength + " ?= " + encodeBuffer.Length);
                                StringBuilder sb = new();
                                sb.Append("        ");
                                for (int i = 0; i < 5 + code.Length; i++)
                                {
                                    sb.Append(string.Format("{0:000} ", encodeBuffer.GetBuffer()[i]));
                                }
                                LogDebug(sb.ToString());
                            }
                            sendTelemetry.AddTelemetry(msg.MessageCode(), messageTotalLength + 5);


                            encodeBuffer.WriteTo(stream);
                            stream.Flush();

                            if (logDebugNetworkMessages)
                            {
                                LogDebug("    Sent.");
                            }
                        }
                        else
                        {
                            session.signal.WaitOne(loopWakeupMillis); // wake up anyway
                        }
                    }
                }
                finally
                {
                    SessionTerminate(session);
                    tcpClient.Close();
                }
            }
            catch (Exception ex)
            {
                if (!stopNetwork.IsCancellationRequested && !session.disconnectToken.IsCancellationRequested)
                {
                    LogError("Crash in SenderLoop for session " + session.id + " < " + session.clientName + " > from "
                        + tcpClient.Client.RemoteEndPoint + "\r\n" + ex);
                }
            }
        }

        static void ReceiverLoop(ClientSession session)
        {
            receiveTelemetry.stopWatch.Start();

            var tcpClient = session.tcpClient;
            var stream = tcpClient.GetStream();
            session.disconnectToken.Register(tcpClient.Close);

            LogDebug("ReceiverLoop Start for client " + session.id + " from " + session.tcpClient.Client.RemoteEndPoint);
            try
            {
                try
                {
                    var encodeBuffer = new MemoryStream(sendBufferSize);
                    var encodeReader = new BinaryReader(encodeBuffer);

                    while (!stopNetwork.IsCancellationRequested && !session.disconnectToken.IsCancellationRequested)
                    {
                        ClearMemoryStream(encodeBuffer, fullResetBuffer);
                        encodeBuffer.SetLength(5);

                        // Read the header with the total and the message code lengths
                        var read = ReadFully(stream, encodeBuffer.GetBuffer(), 0, 5);
                        if (read != 5)
                        {
                            throw new IOException("ReceiverLoop expected 5 more bytes but got " + read);
                        }

                        if (logDebugNetworkMessages)
                        {
                            LogDebug("ReceiverLoop message incoming");
                            StringBuilder sb = new();
                            sb.Append("        ");
                            for (int i = 0; i < 5; i++)
                            {
                                sb.Append(string.Format("{0:000} ", encodeBuffer.GetBuffer()[i]));
                            }
                            LogDebug(sb.ToString());
                        }


                        var totalLength = encodeReader.ReadInt32();
                        var messageCodeLen = encodeReader.ReadByte();

                        if (logDebugNetworkMessages)
                        {
                            LogDebug("    Message totalLength = " + totalLength + ", messageCodeLen = " + messageCodeLen);
                        }

                        // make sure the buffer can hold the remaining of the message
                        if (encodeBuffer.Capacity < 5 + totalLength)
                        {
                            encodeBuffer.Capacity = 5 + totalLength;
                        }

                        encodeBuffer.SetLength(5 + totalLength);
                        // read the rest
                        read = ReadFully(stream, encodeBuffer.GetBuffer(), 5, totalLength);

                        // broken stream?
                        if (read != totalLength)
                        {
                            throw new IOException("ReceiverLoop expected " + totalLength + " more bytes but got " + read);
                        }

                        // make the buffer appear to hold all.
                        encodeBuffer.Position = 5;

                        // decode the the messageCode
                        var messageCode = Encoding.UTF8.GetString(encodeBuffer.GetBuffer(), 5, messageCodeLen);
                        encodeBuffer.Position = 5 + messageCodeLen;

                        if (logDebugNetworkMessages)
                        {
                            LogDebug("    Code: " + messageCode + " with length 4 + 1 + " + messageCodeLen + " + " + (totalLength - messageCodeLen));
                        }
                        receiveTelemetry.AddTelemetry(messageCode, totalLength + 5);

                        // lookup an actual code decoder
                        if (messageRegistry.TryGetValue(messageCode, out var msg))
                        {
                            try
                            {
                                if (msg.TryDecode(encodeReader, out var decoded))
                                {
                                    if (decoded.GetType() != msg.GetType())
                                    {
                                        LogError("    Decoder type bug. Expected = " + msg.GetType() + ", Actual = " + decoded.GetType());
                                    }
                                    else
                                    {
                                        if (logDebugNetworkMessages)
                                        {
                                            LogDebug("    Decode complete.");
                                        }
                                        decoded.sender = session;
                                        decoded.onReceive = msg.onReceive;
                                        receiverQueue.Enqueue(decoded);
                                    }
                                }
                                else
                                {
                                    LogWarning("ReceiverLoop failed to decode message of " + messageCode + " via " + msg.GetType());
                                }
                            } 
                            catch (Exception ex)
                            {
                                LogError("    Decoder crash " + msg.GetType() + "\r\n" + ex);
                            }
                        }
                        else
                        {
                            LogWarning("ReceiverLoop unsupported message type: " + messageCode);
                        }
                    }
                } 
                finally
                {
                    LogDebug("ReceiverLoop ending for " + session.id + " from " + session.tcpClient.Client.RemoteEndPoint);
                    SessionTerminate(session);
                    tcpClient.Close();
                    LogDebug("ReceiverLoop ended for " + session.id + " from " + session.tcpClient.Client.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                if (!stopNetwork.IsCancellationRequested && !session.disconnectToken.IsCancellationRequested)
                {
                    LogError("Crash in ReceiverLoop for client " + session.id + " < " + session.clientName + " > from "
                        + session.tcpClient.Client.RemoteEndPoint + "\r\n" + ex);
                }
            }
        }

        static int ReadFully(Stream stream, byte[] buffer, int offset, int length)
        {
            int read;
            int remaining = length;
            while (remaining > 0)
            {
                read = stream.Read(buffer, offset, remaining);
                if (read <= 0)
                {
                    break;
                }
                offset += read;
                remaining -= read;
            }
            return length - remaining;
        }

        static void ClearMemoryStream(MemoryStream ms, bool fullClear)
        {
            if (fullClear)
            {
                var arr = ms.GetBuffer();
                Array.Clear(arr, 0, arr.Length);
            }
            ms.Position = 0;
            ms.SetLength(0);
        }

        static bool IsIPv4(IPAddress ipa) => ipa.AddressFamily == AddressFamily.InterNetwork;

        static bool IsIPv6(IPAddress ipa) => ipa.AddressFamily == AddressFamily.InterNetworkV6;

        static IPAddress GetMainIPv4() => NetworkInterface.GetAllNetworkInterfaces()
            .Select((ni) => ni.GetIPProperties())
            .Where((ip) => ip.GatewayAddresses.Where((ga) => IsIPv4(ga.Address)).Count() > 0)
            .FirstOrDefault()?.UnicastAddresses?
            .Where((ua) => IsIPv4(ua.Address))?.FirstOrDefault()?.Address;

        static IPAddress GetMainIPv6() => NetworkInterface.GetAllNetworkInterfaces()
            .Select((ni) => ni.GetIPProperties())
            .Where((ip) => ip.GatewayAddresses.Where((ga) => IsIPv6(ga.Address)).Count() > 0)
            .FirstOrDefault()?.UnicastAddresses?
            .Where((ua) => IsIPv6(ua.Address))?.FirstOrDefault()?.Address;


        
    }

    /// <summary>
    /// Represents all information regarding a connecting or connected client.
    /// </summary>
    public class ClientSession
    {
        public readonly int id;

        public volatile string clientName;

        public volatile bool loginSuccess;

        public volatile bool disconnected;

        public TcpClient tcpClient;

        public readonly CancellationToken disconnectToken = new();

        internal readonly ConcurrentQueue<MessageBase> senderQueue = new();

        internal readonly AutoResetEvent signal = new(false);

        /// <summary>
        /// Remembers how may days worth of GPlanet.dailyXXX has been sent over during the full sync.
        /// </summary>
        internal int planetDataSync;

        public ClientSession(int id)
        {
            this.id = id;
        }

        public void Send(MessageBase message, bool signal = true)
        {
            if (!disconnected)
            {
                senderQueue.Enqueue(message);
                if (signal)
                {
                    this.signal.Set();
                }
            }
        }
    }

    internal class Telemetry
    {

        public static bool isEnabled = true;

        internal string name;
        internal long logTelemetry = 30000;
        internal Stopwatch stopWatch = new();

        internal readonly ConcurrentDictionary<string, long> bytes = new();
        internal readonly ConcurrentDictionary<string, long> messages = new();

        internal Telemetry(string name)
        {
            this.name = name;
        }

        internal void AddTelemetry(string message, long length)
        {
            if (isEnabled)
            {
                messages.AddOrUpdate(message, 1, (k, v) => v + 1);
                bytes.AddOrUpdate(message, length, (k, v) => v + length);

                var n = stopWatch.ElapsedMilliseconds;
                if (n >= logTelemetry)
                {
                    StringBuilder sb = new();
                    sb.Append("Telemetry < ").Append(name).Append(" >");

                    long sumBytes = 0;
                    long sumMsgs = 0;

                    var pad = messages.Keys.Select(k => k.Length).Max();

                    foreach (var k in messages.Keys)
                    {
                        var bs = bytes[k];
                        var ms = messages[k];

                        sumBytes += bs;
                        sumMsgs += ms;

                        sb.AppendLine()
                            .Append("    ").Append(k.PadRight(pad)).Append(" x ").AppendFormat("{0,8}", ms)
                            .Append(" ~~~~ ").Append(string.Format("{0:#,##0}", bs).PadLeft(12))
                            .Append(" bytes :::: ").Append(string.Format("{0:#,##0.00} kB/s", ((double)bs) * 1000 / n / 1024).PadLeft(16));
                    }

                    sb.AppendLine()
                    .Append("    =====").AppendLine()
                    .Append("    ").Append("Total".PadRight(pad)).Append(" x ").AppendFormat("{0,8}", sumMsgs)
                    .Append(" ~~~~ ").Append(string.Format("{0:#,##0}", sumBytes).PadLeft(12))
                    .Append(" bytes :::: ").Append(string.Format("{0:#,##0.00} kB/s", ((double)sumBytes) * 1000 / n / 1024).PadLeft(16));

                    messages.Clear();
                    bytes.Clear();

                    Plugin.LogDebug(sb.ToString());

                    stopWatch.Restart();
                }
            }
        }
    }
}
