// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateDepotDrones : MessageBase
    {
        const string messageCode = "UpdateDepotDrones";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotDrone> drones = new();
        internal int2 coords;

        internal void GetSnapshot(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                var snp = new SnapshotDrone();
                snp.GetSnapshot(GDrones.drones[i]);
                drones.Add(snp);
            }
        }

        internal void ApplySnapshot()
        {
            var sworld = SSingleton<SWorld>.Inst;
            var sdrones = SSingleton<SDrones>.Inst;
            var addDroneInGrid = AccessTools.MethodDelegate<Action<CDrone>>(Haxx.sDronesAddDroneInGrid, sdrones);

            foreach (var ds in drones)
            {
                var drone = ds.Create(sworld);
                GDrones.drones.Add(drone);
                addDroneInGrid(drone);
            }
            if (coords.Positive)
            {
                for (int i = 0; i < GDrones.drones.Count; i++)
                {
                    GDrones.drones[i].OnBuildingStackItemChanged(coords);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords);
            output.Write(drones.Count);
            foreach (var drone in drones)
            {
                drone.Encode(output);
            }
        }

        void Decode(BinaryReader input)
        {
            coords = input.ReadInt2();
            int c = input.ReadInt32();

            for (int i = 0; i < c; i++)
            {
                var drone = new SnapshotDrone();
                drone.Decode(input);
                drones.Add(drone);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateDepotDrones();

            msg.Decode(input);

            message = msg;
            return true;
        }

    }
}
