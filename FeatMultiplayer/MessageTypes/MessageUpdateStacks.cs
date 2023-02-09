using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FeatMultiplayer.MessageSyncAllItems;

namespace FeatMultiplayer
{
    internal class MessageUpdateStacks : MessageUpdate
    {
        const string messageCode = "UpdateStacks";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<StackSnapshot> stacks = new();
        public override void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            var gstacks = GHexes.stacks[coords.x, coords.y];

            if (gstacks != null)
            {
                for (int i = 0; i < gstacks.stacks.Length; i++)
                {
                    var ssnp = new StackSnapshot();
                    ssnp.GetSnapshot(in gstacks.stacks[i]);
                    stacks.Add(ssnp);
                }
            }
        }

        public override void ApplySnapshot()
        {
            var lookup = GetItemsDictionary();
            var gstacks = GHexes.stacks[coords.x, coords.y];
            if (gstacks != null)
            {
                for (int i = 0; i < stacks.Count; i++)
                {
                    if (i < gstacks.stacks.Length)
                    {
                        var ssnp = stacks[i];
                        ssnp.ApplySnapshot(ref gstacks.stacks[i], lookup);
                    }
                    else
                    {
                        gstacks.stacks[i].Reset();
                    }
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(stacks.Count);
            foreach (var s in stacks)
            {
                s.Encode(output);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateStacks();
            msg.coords = new int2(input.ReadInt32(), input.ReadInt32());

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var s = new StackSnapshot();
                s.Decode(input);
                stacks.Add(s);
            }
        }
    }
}
