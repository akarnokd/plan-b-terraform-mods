// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateItem : MessageBase
    {
        const string messageCode = "UpdateItem";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly SnapshotItem item = new();

        internal void GetSnapshot(CItem item)
        {
            this.item.GetSnapshot(item);
        }

        internal void ApplySnapshot()
        {
            var itemLookup = Plugin.GetItemsDictionary();

            if (itemLookup.TryGetValue(this.item.codeName, out var item))
            {
                item.nbOwned = this.item.count;
                item.nbOwnedMax = this.item.max;
            }
            else
            {
                LogError("MessageSyncAllItems: Unknown item when applying snapshot " + this.item.codeName);
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(item.codeName);
            output.Write(item.count);
            output.Write(item.max);
        }


        void Decode(BinaryReader input)
        {
            item.codeName = input.ReadString();
            item.count = input.ReadInt32();
            item.max = input.ReadInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateItem();

            msg.Decode(input);

            message = msg;
            return true;
        }
    }
}
