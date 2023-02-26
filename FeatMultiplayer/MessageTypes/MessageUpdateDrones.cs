// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateDrones : MessageBase
    {
        const string messageCode = "UpdateDrones";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotDroneLive> drones = new();
        internal HashSet<int> removedIds;

        internal void GetSnapshot(HashSet<int> removedIds)
        {
            this.removedIds = removedIds;
            foreach (var drone in GDrones.drones)
            {
                var snp = new SnapshotDroneLive();
                snp.GetSnapshot(drone);
                drones.Add(snp);
            }
        }

        internal void GetDiffSnapshot(HashSet<int> removedIds, Dictionary<int, SnapshotDroneLive> before)
        {
            this.removedIds = removedIds;
            foreach (var drone in GDrones.drones)
            {
                var snp = new SnapshotDroneLive();
                snp.GetSnapshot(drone);

                before.TryGetValue(drone.id, out var b);

                if (b == null || snp.HasChangedSince(b))
                {
                    drones.Add(snp);
                }
            }
        }

        internal void ApplySnapshot()
        {
            var itemLookup = Plugin.GetItemsDictionary();

            var droneLookup = new Dictionary<int, CDrone>();

            List<CDrone> allDrones = GDrones.drones;
            for (int i = allDrones.Count - 1; i >= 0; i--)
            {
                var drone = GDrones.drones[i];
                if (removedIds.Contains(drone.id))
                {
                    allDrones.RemoveAt(i);
                }
                else
                {
                    droneLookup.Add(drone.id, drone);
                }
            }

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
            output.Write(removedIds.Count);
            foreach (var id in removedIds)
            {
                output.Write(id);
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

            c = input.ReadInt32();
            removedIds = new();
            for (int i = 0; i < c; i++)
            {
                removedIds.Add(input.ReadInt32());
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
