using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageActionFinishLine : MessageBase
    {
        const string messageCode = "ActionFinishLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly SnapshotLine newLine = new();
        internal int oldLineId;

        public override void Encode(BinaryWriter output)
        {
            newLine.Encode(output);
            output.Write(oldLineId);
        }

        void Decode(BinaryReader input)
        {
            newLine.Decode(input);
            oldLineId = input.ReadInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionFinishLine();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
