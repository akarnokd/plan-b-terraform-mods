// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllFlags : MessageSync
    {
        const string messageCode = "SyncAllFlags";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int[] data;

        internal override void GetSnapshot()
        {
            var s = GWorld.size;
            var x = s.x;
            var y = s.y;
            var src = GHexes.flags;
            data = new int[x * y];
            var k = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    int v = (int)src[i, j];
                    data[k++] = v;
                }
            }
        }

        internal override void ApplySnapshot()
        {
            var s = GWorld.size;
            var y = s.y;
            var dst = GHexes.flags;

            var row = 0;
            var col = 0;
            for (int a = 0; a < data.Length; a++)
            {
                dst[row, col] = (GHexes.Flag)data[a];

                if (++col == y)
                {
                    row++;
                    col = 0;
                }
            }
        }


        public override void Encode(BinaryWriter output)
        {
            RLE.Encode(data, output);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllFlags();

            RLE.Decode(input, ref msg.data);
            message = msg;
            return true;
        }

    }
}
