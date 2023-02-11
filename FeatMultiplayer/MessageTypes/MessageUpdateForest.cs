// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateForest : MessageBase
    {
        const string messageCode = "UpdateForest";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotContentAt> contents = new();

        public void GetSnapshot(int2 mainCoords, IEnumerable<int2> changedCoords)
        {
            foreach (var coords in changedCoords)
            {
                var snp = new SnapshotContentAt();
                snp.GetSnapshot(coords);
                contents.Add(snp);
            }

            var msnp = new SnapshotContentAt();
            msnp.GetSnapshot(mainCoords);
            contents.Add(msnp);
        }

        public void ApplySnapshot()
        {
            foreach (var snp in contents)
            {
                snp.ApplySnapshot();
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(contents.Count);
            foreach (var snp in contents)
            {
                snp.Encode(output);
            }
        }

        void Decode(BinaryReader input)
        {
            int c = input.ReadInt32();
            for (int i = 0; i < c; i++) 
            {
                var snp = new SnapshotContentAt();
                snp.Decode(input);
                contents.Add(snp);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateForest();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
