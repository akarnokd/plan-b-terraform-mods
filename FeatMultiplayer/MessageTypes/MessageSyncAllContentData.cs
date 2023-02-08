using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageSyncAllContentData : MessageSyncUIntArray
    {
        const string messageCode = "SyncAllContentData";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override uint[,] GetData()
        {
            return GHexes.contentData;
        }

        public override string MessageCode() => messageCode;

        public override byte[] MessageCodeBytes() => messageCodeBytes;

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllContentData();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
