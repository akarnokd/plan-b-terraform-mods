// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateWater : MessageUpdate
    {
        const string messageCode = "UpdateWater";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal float value;

        public override void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            value = GHexes.water[coords.x, coords.y];
        }

        public override void ApplySnapshot()
        {
            GHexes.water[coords.x, coords.y] = value;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(value);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateWater();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());
            msg.value = input.ReadSingle();
            message = msg;
            return true;
        }
    }
}
