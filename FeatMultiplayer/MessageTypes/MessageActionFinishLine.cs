// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageActionFinishLine : MessageBase
    {
        const string messageCode = "ActionFinishLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly SnapshotLine newLine = new();
        internal int lineModifiedId;
        internal int lineCopiedId;
        internal int2 pickCoords;

        public override void Encode(BinaryWriter output)
        {
            newLine.Encode(output);
            output.Write(lineModifiedId);
            output.Write(lineCopiedId);
            output.Write(pickCoords);
        }

        void Decode(BinaryReader input)
        {
            newLine.Decode(input);
            lineModifiedId = input.ReadInt32();
            lineCopiedId = input.ReadInt32();
            pickCoords = input.ReadInt2();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionFinishLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
