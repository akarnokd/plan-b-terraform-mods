using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageUpdateFlags : MessageUpdate
    {
        const string messageCode = "UpdateFlags";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int value;

        public override void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            value = (int)GHexes.flags[coords.x, coords.y];
        }

        public override void ApplySnapshot()
        {
            GHexes.flags[coords.x, coords.y] = (GHexes.Flag)value;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(value);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateFlags();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());
            msg.value = input.ReadInt32();
            message = msg;
            return true;
        }
    }
}
