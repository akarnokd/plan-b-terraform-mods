// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageActionRemoveLine : MessageBase
    {
        const string messageCode = "ActionRemoveLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int lineId;

        public override void Encode(BinaryWriter output)
        {
            output.Write(lineId);
        }

        void Decode(BinaryReader input)
        {
            lineId = input.ReadInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionRemoveLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
