// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateItems : MessageSync
    {
        const string messageCode = "UpdateItems";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotItem> items = new();

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
        }

        internal override void ApplySnapshot()
        {
            var itemLookup = Plugin.GetItemsDictionary();

            foreach (var isnp in items)
            {
                if (itemLookup.TryGetValue(isnp.codeName, out var item))
                {
                    item.nbOwned = isnp.count;
                }
                else
                {
                    LogError("MessageSyncAllItems: Unknown item when applying snapshot " + isnp.codeName);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(items.Count);
            foreach (var item in items)
            {
                output.Write(item.codeName);
                output.Write(item.count);
            }
        }


        void Decode(BinaryReader input)
        {
            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var isnp = new SnapshotItem();
                items.Add(isnp);
                isnp.codeName = input.ReadString();
                isnp.count = input.ReadInt32();
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateItems();

            msg.Decode(input);

            message = msg;
            return true;
        }
    }
}
