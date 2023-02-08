using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageSyncAllGroundData : MessageSyncUShortArray
    {
        const string messageCode = "SyncAllGroundData";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override ushort[,] GetData()
        {
            return GHexes.groundData;
        }

        public override string MessageCode() => messageCode;

        public override byte[] MessageCodeBytes() => messageCodeBytes;

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllGroundData();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
