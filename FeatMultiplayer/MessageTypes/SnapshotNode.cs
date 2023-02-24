// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using UnityEngine;

namespace FeatMultiplayer
{
    internal class SnapshotNode
    {
        internal int2 coords;
        internal float tExit;
        internal Vector3 posMiddle;
        internal Vector3 posExit;

        internal void GetSnapshot(CLine.Node node)
        {
            coords = node.coords;
            tExit = node.tExit;
            posMiddle = node.posMiddle;
            posExit = node.posExit;
        }

        internal CLine.Node Create()
        {
            var result = new CLine.Node(coords, tExit);
            result.posMiddle = posMiddle;
            result.posExit = posExit;
            return result;
        }

        internal void ApplySnapshot(CLine.Node node)
        {
            node.coords = coords;
            node.tExit = tExit;
            node.posMiddle = posMiddle;
            node.posExit = posExit;
        }

        internal void Encode(BinaryWriter output)
        {
            output.WriteShort(coords);
            output.Write(tExit);
            output.Write(posMiddle);
            output.Write(posExit);
        }

        internal void Decode(BinaryReader input)
        {
            coords = input.ReadInt2Short();
            tExit = input.ReadSingle();
            posMiddle = input.ReadVector3();
            posExit = input.ReadVector3();
        }
    }
}
