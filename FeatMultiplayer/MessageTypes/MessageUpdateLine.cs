using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageUpdateLine : MessageBase
    {
        const string messageCode = "UpdateLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal bool create;
        internal readonly SnapshotLine line = new();

        internal void GetSnapshot(CLine line, bool create)
        {
            this.create = create;
            this.line.GetSnapshot(line);
        }

        internal void ApplySnapshot(CLine cline)
        {
            var itemLookup = Plugin.GetItemsDictionary();
            line.ApplySnapshot(cline, itemLookup);

            cline.UpdateStopDataOrginEnd(true, false);
            cline.ComputePath_Positions(create);

        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(create);
            line.Encode(output);
        }

        void Decode(BinaryReader input)
        {
            create = input.ReadBoolean();
            line.Decode(input);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateLine();

            msg.Decode(input);

            message = msg;
            return true;
        }

    }
}
