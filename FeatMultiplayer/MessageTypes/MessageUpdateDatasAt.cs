using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageUpdateDatasAt : MessageBase
    {
        const string messageCode = "UpdateDatasAt";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal bool updateBlocks;
        internal ushort groundData;
        internal uint contentData;

        public void GetSnapshot(int2 coords, bool updateBlocks)
        {
            this.coords = coords;
            this.updateBlocks = updateBlocks;
            groundData = GHexes.groundData[coords.x, coords.y];
            contentData = GHexes.contentData[coords.x, coords.y];
        }

        public void ApplySnapshot()
        {
            GHexes.groundData[coords.x, coords.y] = groundData;
            GHexes.contentData[coords.x, coords.y] = contentData;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(updateBlocks);
            output.Write(groundData);
            output.Write(contentData);
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            updateBlocks = input.ReadBoolean();
            groundData = input.ReadUInt16();
            contentData = input.ReadUInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateDatasAt();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
