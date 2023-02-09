using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageActionRenameLandmark : MessageBase
    {
        const string messageCode = "RenameLandmark";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal string name;

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(name);
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            name = input.ReadString();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionRenameLandmark();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
