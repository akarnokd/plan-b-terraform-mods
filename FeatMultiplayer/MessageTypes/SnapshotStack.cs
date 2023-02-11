// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotStack
    {
        internal string codeName;
        internal int count;
        internal int booked;

        internal void GetSnapshot(in CStack stack)
        {
            codeName = stack.item?.codeName ?? "";
            count = stack.nb;
            booked = stack.nbBooked;
        }

        internal void ApplySnapshot(ref CStack stack, Dictionary<string, CItem> lookup)
        {
            lookup.TryGetValue(codeName, out stack.item);
            stack.nb = count;
            stack.nbBooked = booked;
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(codeName);
            output.Write(count);
            output.Write(booked);
        }

        internal void Decode(BinaryReader input)
        {
            codeName = input.ReadString();
            count = input.ReadInt32();
            booked = input.ReadInt32();
        }
    }
}
