// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateLines : MessageBase
    {
        const string messageCode = "UpdateLines";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly List<SnapshotLine> lines = new();
        internal HashSet<int> linesRemoved;

        internal void GetSnapshot(HashSet<int> linesBefore)
        {
            for (int i = 1; i < GWays.lines.Count; i++)
            {
                CLine line = GWays.lines[i];

                var snp = new SnapshotLine();
                snp.GetSnapshot(line);
                lines.Add(snp);
                linesBefore.Remove(line.id);
            }

            linesRemoved = linesBefore;
        }

        internal void GetSnapshotDiff(HashSet<int> linesBefore, Dictionary<int, List<SnapshotNode>> nodesBefore)
        {
            for (int i = 1; i < GWays.lines.Count; i++)
            {
                CLine line = GWays.lines[i];

                var snp = new SnapshotLine();
                snp.GetSnapshot(line);
                lines.Add(snp);
                linesBefore.Remove(line.id);

                nodesBefore.TryGetValue(line.id, out var nodes);
                if (nodes != null && !snp.HaveNodesChanged(nodes))
                {
                    snp.updateNodes = false;
                    snp.nodes.Clear();
                }
            }

            linesRemoved = linesBefore;
        }

        internal void ApplySnapshot()
        {
            var lineLookup = Plugin.GetLineDictionary();
            var itemLookup = Plugin.GetItemsDictionary();

            foreach (var line in lines)
            {
                if (lineLookup.TryGetValue(line.id, out var cline))
                {
                    line.ApplySnapshot(cline, itemLookup, false);
                }
            }
            var sWays = SSingleton<SWays>.Inst;
            foreach (var id in linesRemoved)
            {
                if (lineLookup.TryGetValue(id, out var cline))
                {
                    sWays.RemoveLine(cline);
                }
            }
        }

        public override void Encode(BinaryWriter output)
        {
            if (Plugin.compressNetwork.Value)
            {
                using (var ds = new DeflateStream(output.BaseStream, CompressionLevel.Optimal, true)) {
                    EncodeInternal(new BinaryWriter(ds));
                }
            }
            else
            {
                EncodeInternal(output);
            }
        }

        public void EncodeInternal(BinaryWriter output)
        {
            output.Write(lines.Count);
            foreach (var line in lines)
            {
                line.Encode(output);
            }
            output.Write(linesRemoved.Count);
            foreach (var i in linesRemoved)
            {
                output.Write(i);
            }
        }

        void Decode(BinaryReader input)
        {
            if (Plugin.compressNetwork.Value)
            {
                using (var ds = new DeflateStream(input.BaseStream, CompressionMode.Decompress, true))
                {
                    DecodeInternal(new BinaryReader(ds));
                }
            }
            else
            {
                DecodeInternal(input);
            }
        }

        void DecodeInternal(BinaryReader input)
        {
            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var snp = new SnapshotLine();
                snp.Decode(input);
                lines.Add(snp);
            }
            linesRemoved = new();
            c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                linesRemoved.Add(input.ReadInt32());
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateLines();

            msg.Decode(input);

            message = msg;
            return true;
        }

    }
}
