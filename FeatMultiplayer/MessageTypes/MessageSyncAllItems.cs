﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllItems : MessageSync
    {
        const string messageCode = "SyncAllItems";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotItem> items = new();
        internal readonly Dictionary<int2, List<SnapshotStack>> stacks = new();

        internal override void GetSnapshot()
        {
            // slot zero is reserved
            for (int i = 1; i < GItems.items.Count; i++)
            {
                CItem item = GItems.items[i];
                var isnap = new SnapshotItem();
                isnap.GetSnapshot(item);
                items.Add(isnap);
            }

            foreach (var coords in GItems.itemsDynamicCoords)
            {
                var gstacks = GHexes.stacks[coords.x, coords.y];
                if (gstacks != null)
                {
                    var stacksAt = new List<SnapshotStack>();
                    stacks.Add(coords, stacksAt);

                    foreach (var s in gstacks.stacks)
                    {
                        var ssn = new SnapshotStack();
                        ssn.GetSnapshot(in s);
                        stacksAt.Add(ssn);
                    }
                }
            }
        }

        internal override void ApplySnapshot()
        {
            Dictionary<string, CItem> itemsDictionary = Plugin.GetItemsDictionary();

            foreach (var isnp in items)
            {
                if (itemsDictionary.TryGetValue(isnp.codeName, out var item))
                {
                    item.nbOwned = isnp.count;
                    item.nbOwnedMax = isnp.max;
                }
                else
                {
                    LogError("MessageSyncAllItems: Unknown item when applying snapshot " + isnp.codeName);
                }
            }

            var sworld = SSingleton<SWorld>.Inst;

            var dync = GItems.itemsDynamicCoords;
            dync.Clear();
            foreach (var kv in stacks)
            {
                var coords = kv.Key;
                dync.Add(coords);

                if (sworld.GetContent(coords) is CItem_ContentStock contentStock)
                {
                    var cStacks = new CStacks(contentStock);
                    GHexes.stacks[coords.x, coords.y] = cStacks;

                    contentStock.RefreshStacksInfos(coords, cStacks);

                    var sstack = kv.Value;

                    for (int i = 0; i < cStacks.stacks.Count(); i++)
                    {
                        sstack[i].ApplySnapshot(ref cStacks.stacks[i], itemsDictionary);
                    }

                    contentStock.RefreshStacksInfos(coords, cStacks);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            Dictionary<string, byte> codeNameTable = new(); // FIXME adjust when there are more than 256 item types
            codeNameTable[""] = 0;

            output.Write(items.Count);
            foreach (var item in items)
            {
                output.Write(item.codeName);
                output.Write(item.count);
                output.Write(item.max);

                codeNameTable.Add(item.codeName, (byte)codeNameTable.Count);
            }
            output.Write(stacks.Count);
            foreach (var stack in stacks) 
            {
                var coords = stack.Key;
                output.Write(coords.x);
                output.Write(coords.y);

                var sstack = stack.Value;
                output.Write((byte)sstack.Count); // FIXME in case large amount of stacks per coords
                foreach (var sstackItem in sstack)
                {
                    output.Write(codeNameTable[sstackItem.codeName]);
                    output.Write(sstackItem.count);
                    output.Write(sstackItem.booked);
                }
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllItems();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            Dictionary<byte, string> codeNameTable = new(); // FIXME adjust when there are more than 256 item types
            codeNameTable[0] = "";

            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var isnp = new SnapshotItem();
                items.Add(isnp);
                isnp.codeName = input.ReadString();
                isnp.count = input.ReadInt32();
                isnp.max = input.ReadInt32();

                codeNameTable.Add((byte)codeNameTable.Count, isnp.codeName);
            }

            c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var x = input.ReadInt32();
                var y = input.ReadInt32();
                var coords = new int2(x, y);

                var sstacks = new List<SnapshotStack>();

                stacks.Add(coords, sstacks);

                int d = input.ReadByte();

                for (int j = 0; j < d; j++)
                {
                    var sst = new SnapshotStack();
                    sstacks.Add(sst);

                    sst.codeName = codeNameTable[input.ReadByte()];
                    sst.count = input.ReadInt32();
                    sst.booked = input.ReadInt32();
                }
            }
        }
    }
}
