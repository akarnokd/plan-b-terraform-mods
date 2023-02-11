// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateFinishLine : MessageBase
    {
        const string messageCode = "UpdateFinishLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal bool pickItem;
        internal int2 pickCoords;
        internal int oldLineId;
        internal readonly SnapshotLine line = new();

        public override void Encode(BinaryWriter output)
        {
            output.Write(pickItem);
            output.Write(pickCoords);
            output.Write(oldLineId);
            line.Encode(output);
        }

        void Decode(BinaryReader input)
        {
            pickItem = input.ReadBoolean();
            pickCoords = input.ReadInt2();
            oldLineId = input.ReadInt32();
            line.Decode(input);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateFinishLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
