﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;

namespace FeatMultiplayer
{
    internal abstract class MessageSyncUShortArray : MessageSync
    {
        internal ushort[] data;

        internal abstract ushort[,] GetData();

        internal override void GetSnapshot()
        {
            var s = GWorld.size;
            data = new ushort[s.x * s.y];
            Buffer.BlockCopy(GetData(), 0, data, 0, s.x * s.y * 2);
        }

        internal override void ApplySnapshot()
        {
            var s = GWorld.size;
            Buffer.BlockCopy(data, 0, GetData(), 0, s.x * s.y * 2);
        }

        public override void Encode(BinaryWriter output)
        {
            RLE.Encode(data, output);
        }

        internal void Decode(BinaryReader input)
        {
            RLE.Decode(input, ref data);
        }
    }
}
