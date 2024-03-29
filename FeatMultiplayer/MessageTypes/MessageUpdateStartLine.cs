﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateStartLine : MessageBase
    {
        const string messageCode = "UpdateStartLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int lineId;
        internal int2 coords;
        internal bool reverse;

        public override void Encode(BinaryWriter output)
        {
            output.Write(lineId);
            output.Write(coords);
            output.Write(reverse);
        }

        void Decode(BinaryReader input)
        {
            lineId = input.ReadInt32();
            coords = input.ReadInt2();
            reverse = input.ReadBoolean();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateStartLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
