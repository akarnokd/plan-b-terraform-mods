// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateLine : MessageBase
    {
        const string messageCode = "UpdateLine";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal bool computePath;
        internal readonly SnapshotLine line = new();

        internal void GetSnapshot(CLine line, bool computePath)
        {
            this.computePath = computePath;
            this.line.GetSnapshot(line);
        }

        internal void ApplySnapshot(CLine cline)
        {
            var itemLookup = Plugin.GetItemsDictionary();
            line.ApplySnapshot(cline, itemLookup);
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(computePath);
            line.Encode(output);
        }

        void Decode(BinaryReader input)
        {
            computePath = input.ReadBoolean();
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
