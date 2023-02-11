// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateContentData : MessageUpdate
    {
        const string messageCode = "UpdateContentData";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal uint value;

        public override void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            value = GHexes.contentData[coords.x, coords.y];
        }

        public override void ApplySnapshot()
        {
            GHexes.contentData[coords.x, coords.y] = value;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(value);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateContentData();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());
            msg.value = input.ReadUInt32();
            message = msg;
            return true;
        }
    }
}
