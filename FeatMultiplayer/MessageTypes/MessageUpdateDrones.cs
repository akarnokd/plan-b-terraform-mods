// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateDrones : MessageSync
    {
        const string messageCode = "UpdateDrones";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotDroneLive> drones = new();

        internal override void GetSnapshot()
        {
            foreach (var drone in GDrones.drones)
            {
                var snp = new SnapshotDroneLive();
                snp.GetSnapshot(drone);
                drones.Add(snp);
            }
        }

        internal override void ApplySnapshot()
        {
            var droneLookup = Plugin.GetDronesDictionary();
            var itemLookup = Plugin.GetItemsDictionary();

            foreach (var drone in drones)
            {
                if (droneLookup.TryGetValue(drone.id, out var cdrone))
                {
                    drone.ApplySnapshot(cdrone, itemLookup);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(drones.Count);
            foreach (var drone in drones)
            {
                drone.Encode(output);
            }
        }

        void Decode(BinaryReader input)
        {
            int c = input.ReadInt32();

            for (int i = 0; i < c; i++)
            {
                var drone = new SnapshotDroneLive();
                drone.Decode(input);
                drones.Add(drone);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateDrones();

            msg.Decode(input);

            message = msg;
            return true;
        }

    }
}
