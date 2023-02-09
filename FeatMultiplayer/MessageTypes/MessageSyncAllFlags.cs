using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    data[i * x + j] = (int)src[i, j];
                }
            }
        }

        internal override void ApplySnapshot()
        {
            var s = GWorld.size;
            var x = s.x;
            var dst = GHexes.flags;

            var row = 0;
            var col = 0;
            for (int a = 0; a < data.Length; a++)
            {
                dst[row, col] = (GHexes.Flag)data[a];

                if (++col == x)
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

            var s = GWorld.size;
            var x = s.x;
            var y = s.y;
            msg.data = new int[x * y];

            RLE.Decode(input, msg.data);
            message = msg;
            return true;
        }

    }
}
