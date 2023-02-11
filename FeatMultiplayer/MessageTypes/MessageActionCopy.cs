// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageActionCopy : MessageBase
    {
        const string messageCode = "ActionCopy";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal string codeName;
        internal int2 fromCoords;
        internal int2 toCoords;

        public override void Encode(BinaryWriter output)
        {
            output.Write(codeName);
            output.Write(fromCoords);
            output.Write(toCoords);
        }

        void Decode(BinaryReader input)
        {
            codeName = input.ReadString();
            fromCoords = input.ReadInt2();
            toCoords = input.ReadInt2();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionCopy();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
