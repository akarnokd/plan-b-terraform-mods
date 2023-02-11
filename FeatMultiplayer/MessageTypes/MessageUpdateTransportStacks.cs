// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateTransportStacks : MessageBase
    {
        const string messageCode = "UpdateTransportStacks";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal const byte TakeFromValid = 1;
        internal const byte GiveToValid = 2;
        internal const byte UpdateFrom = 4;
        internal const byte UpdateTo = 8;
        internal const byte All = TakeFromValid + GiveToValid + UpdateFrom + UpdateTo;

        internal byte flags;

        internal readonly SnapshotTransportStack takeFrom = new();

        internal readonly SnapshotTransportStack giveTo = new();

        public void GetSnapshot(in CDrone.TransportStep src, in CDrone.TransportStep dst, byte flags)
        {
            this.flags = flags;
            if ((flags & TakeFromValid) != 0)
            {
                takeFrom.GetSnapshot(src);
            }
            if ((flags & GiveToValid) != 0)
            {
                giveTo.GetSnapshot(dst);
            }
        }

        public void ApplySnapshot()
        {
            var itemLookup = Plugin.GetItemsDictionary();
            var vehicleLookup = Plugin.GetVehiclesDictionary();

            if ((flags & TakeFromValid) != 0)
            {
                takeFrom.ApplySnapshot(vehicleLookup, itemLookup);
            }

            if ((flags & GiveToValid) != 0)
            {
                giveTo.ApplySnapshot(vehicleLookup, itemLookup);
            }

            if ((flags & TakeFromValid) != 0 && (flags & UpdateFrom) != 0)
            {
                Haxx.SBlocks_OnChangeItem(takeFrom.coords, false, false, true);
            }

            if ((flags & GiveToValid) != 0 && (flags & UpdateTo) != 0)
            {
                Haxx.SBlocks_OnChangeItem(giveTo.coords, false, false, true);
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(flags);
            if ((flags & TakeFromValid) != 0)
            {
                takeFrom.Encode(output);
            }
            if ((flags & GiveToValid) != 0)
            {
                giveTo.Encode(output);
            }
        }

        void Decode(BinaryReader input)
        {
            flags = input.ReadByte();
            if ((flags & TakeFromValid) != 0)
            {
                takeFrom.Decode(input);
            }

            if ((flags & GiveToValid) != 0)
            {
                giveTo.Decode(input);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateTransportStacks();
            msg.Decode(input);

            message = msg;
            return true;
        }
    }
}
