using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageSyncAllFlags : MessageSyncEnumArray<GHexes.Flag>
    {
        const string messageCode = "SyncAllFlags";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override GHexes.Flag[,] GetData()
        {
            return GHexes.flags;
        }

        public override string MessageCode() => messageCode;

        public override byte[] MessageCodeBytes() => messageCodeBytes;

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllFlags();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
