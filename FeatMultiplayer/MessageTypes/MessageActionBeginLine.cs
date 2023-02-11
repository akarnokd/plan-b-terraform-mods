// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageActionBeginLine : MessageBase
    {
        const string messageCode = "ActionBeginLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal bool reverse;

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords);
            output.Write(reverse);
        }

        void Decode(BinaryReader input)
        {
            coords = input.ReadInt2();
            reverse = input.ReadBoolean();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionBeginLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
