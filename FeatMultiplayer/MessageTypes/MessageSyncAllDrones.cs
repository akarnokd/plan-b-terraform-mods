﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllDrones : MessageSync
    {
        const string messageCode = "SyncAllDrones";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int maxId;
        internal readonly List<SnapshotDrone> drones = new();

        internal override void GetSnapshot()
        {
            maxId = CDrone.idMax;
            foreach (var drone in GDrones.drones)
            {
                var ds = new SnapshotDrone();
                ds.GetSnapshot(drone);
                drones.Add(ds);
            }
        }

        internal override void ApplySnapshot()
        {
            GDrones.modelsPool.Clear();
            GDrones.drones.Clear();

            var sworld = SSingleton<SWorld>.Inst;
            var sdrones = SSingleton<SDrones>.Inst;

            foreach (var ds in drones)
            {
                var drone = ds.Create(sworld);
                GDrones.drones.Add(drone);
            }
            // set max here because the CDrone constructor we use increments
            CDrone.idMax = maxId;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(maxId);
            output.Write(drones.Count);
            foreach (var drone in drones)
            {
                drone.Encode(output);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllDrones();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            maxId = input.ReadInt32();

            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var drone = new SnapshotDrone();
                drone.Decode(input);
                drones.Add(drone);
            }
        }

    }
}
