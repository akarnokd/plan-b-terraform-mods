// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateStackAt : MessageBase
    {
        const string messageCode = "UpdateStackAt";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal int index;
        internal readonly SnapshotStack stack = new();

        public void GetSnapshot(int2 coords, int index)
        {
            this.coords = coords;
            this.index = index;
            var gstacks = GHexes.stacks[coords.x, coords.y];
            stack.GetSnapshot(in gstacks.stacks[index]);
        }

        public void CreateRequest(int2 coords, int index, CItem item, int count, int booked)
        {
            this.coords = coords;
            this.index = index;
            stack.codeName = item?.codeName ?? "";
            stack.count = count;
            stack.booked = booked;
        }

        public void ApplySnapshot()
        {
            var lookup = Plugin.GetItemsDictionary();
            var gstacks = GHexes.stacks[coords.x, coords.y];
            if (gstacks != null)
            {
                if (index < gstacks.stacks.Length)
                {
                    stack.ApplySnapshot(ref gstacks.stacks[index], lookup);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(index);
            stack.Encode(output);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateStackAt();
            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            index = input.ReadInt32();
            stack.Decode(input);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb
                .Append(nameof(coords)).Append(" = ").Append(coords).Append(", ")
                .Append(nameof(index)).Append(" = ").Append(index).Append(", ")
                .Append(nameof(stack)).Append(" = ").Append(stack)
            ;
            return sb.ToString();
        }
    }
}
