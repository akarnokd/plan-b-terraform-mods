// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageActionChangeVehicleCount : MessageBase
    {
        const string messageCode = "MessageActionChangeVehicleCount";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal int delta;

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords);
            output.Write(delta);
        }

        void Decode(BinaryReader input)
        {
            coords = input.ReadInt2();
            delta = input.ReadInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionChangeVehicleCount();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
