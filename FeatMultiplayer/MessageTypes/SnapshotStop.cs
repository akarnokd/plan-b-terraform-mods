// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotStop
    {
        internal int2 coords;
        internal float t;

        internal void GetSnapshot(CLine.Stop stop)
        {
            coords = stop.coords;
            t = stop.t;
        }

        internal CLine.Stop Create()
        {
            var result = new CLine.Stop(coords);
            result.t = t;
            return result;
        }

        internal void ApplySnapshot(CLine.Stop stop)
        {
            stop.coords = coords;
            stop.t = t;
        }

        internal void Encode(BinaryWriter output)
        {
            output.WriteShort(coords);
            output.Write(t);
        }

        internal void Decode(BinaryReader input)
        {
            coords = input.ReadInt2Short();
            t = input.ReadInt32();
        }
    }
}
