// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        
    }

    internal class NetworkTelemetry
    {

        public static bool isEnabled = true;

        internal string name;
        internal long logTelemetry = 30000;
        internal Stopwatch stopWatch = new();

        internal readonly ConcurrentDictionary<string, long> bytes = new();
        internal readonly ConcurrentDictionary<string, long> messages = new();

        internal NetworkTelemetry(string name)
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

    internal class CallTelemetry
    {

        public static bool isEnabled = true;

        internal string name;

        internal readonly Dictionary<string, int> counts = new();
        internal readonly Dictionary<string, long> timesInTicks = new();

        internal long logTelemetry = 30000;
        internal Stopwatch stopWatch = new();

        internal Stopwatch timer = new();

        internal long lastCheckpoint;

        internal CallTelemetry(string name)
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
            lastCheckpoint = 0L;
            return v;
        }

        internal void AddTelemetry(string message, long length)
        {
            if (!isEnabled)
            {
                return;
            }

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

        internal void AddTelemetry(string message)
        {
            AddTelemetry(message, GetAndReset());
        }

        internal void AddTelemetryCheckpoint(string message)
        {
            var cp = lastCheckpoint;
            var curr = timer.ElapsedTicks;
            lastCheckpoint = curr;
            AddTelemetry(message, curr - cp);
        }
    }
}
