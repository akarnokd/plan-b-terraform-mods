﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateCity : MessageBase
    {
        const string messageCode = "UpdateCity";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly SnapshotCity city = new();
        internal readonly List<SnapshotContentAt> updatedHexes = new();

        public void GetSnapshot(CCity city, IEnumerable<int2> updatedHexes)
        {
            this.city.GetSnapshot(city);

            foreach (var coords in updatedHexes)
            {
                var snp = new SnapshotContentAt();
                snp.GetSnapshot(coords);
                this.updatedHexes.Add(snp);
            }
        }

        public void ApplySnapshot(CCity city)
        {
            this.city.ApplySnapshot(city);
        }

        internal bool HasChanged(MessageUpdateCity other)
        {
            return this.city.HasChanged(other.city)
                || HaveHexesChanged(other.updatedHexes);
        }

        internal bool HaveHexesChanged(List<SnapshotContentAt> updatedHexes)
        {
            if (this.updatedHexes.Count != updatedHexes.Count)
            {
                return true;
            }

            for (int i = 0; i < this.updatedHexes.Count; i++)
            {
                var h1 = this.updatedHexes[i];
                var h2 = updatedHexes[i];

                if (h1.HasChanged(h2))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Encode(BinaryWriter output)
        {
            city.Write(output);
            output.Write(updatedHexes.Count);
            foreach (var h in updatedHexes)
            {
                h.Encode(output);
            }
        }

        void Decode(BinaryReader input)
        {
            city.Read(input);
            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var snp = new SnapshotContentAt();
                snp.Decode(input);
                updatedHexes.Add(snp);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateCity();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
