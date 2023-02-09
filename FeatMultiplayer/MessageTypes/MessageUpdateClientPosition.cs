using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageUpdateClientPosition : MessageBase
    {
        const string messageCode = "ClientPosition";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;

        public void GetSnapshot()
        {
            coords = GScene3D.mouseoverCoords;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
        }


        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateClientPosition();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());
            message = msg;
            return true;
        }
    }
}
