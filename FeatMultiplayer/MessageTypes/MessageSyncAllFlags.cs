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
            var x = s.x;
            var y = s.y;
            var dst = GHexes.flags;
            var k = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    dst[i, j] = (GHexes.Flag)data[k++];
                }
            }
        }


        public override void Encode(BinaryWriter output)
        {
            RLE.Encode(data, output);
            /*
            output.Write(data.Length);
            foreach (var d in data)
            {
                output.Write(d);
            }
            */
        }

        void Decode(BinaryReader input)
        {
            RLE.Decode(input, ref data);
            /*
            int c = input.ReadInt32();
            data = new int[c];
            for (int i = 0; i < c; i++)
            {
                data[i] = input.ReadInt32();
            }
            */
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllFlags();
            msg.Decode(input);
            message = msg;
            return true;
        }

    }
}
