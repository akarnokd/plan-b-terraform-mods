// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateContentId : MessageUpdate
    {
        const string messageCode = "UpdateContentId";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal byte value;

        public override void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            value = GHexes.contentId[coords.x, coords.y];
        }

        public override void ApplySnapshot()
        {
            GHexes.contentId[coords.x, coords.y] = value;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(value);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateContentId();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());
            msg.value = input.ReadByte();
            message = msg;
            return true;
        }
    }
}
