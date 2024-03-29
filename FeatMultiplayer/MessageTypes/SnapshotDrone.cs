﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotDrone
    {
        internal int id;
        internal int depotIndex;
        internal int2 depotCoords;

        internal void GetSnapshot(CDrone drone)
        {
            id = drone.id;
            depotIndex = Haxx.cDroneDroneDepotIndex.Invoke(drone);
            depotCoords = drone.depotCoords;
        }

        internal CDrone Create(SWorld sworld)
        {
            CItem_Content content = sworld.GetContent(depotCoords);
            var depot = content as CItem_ContentDepot;
            if (depot == null)
            {
                throw new InvalidOperationException("Depot error: " + (content != null ? content.GetType() : "null") + " at " + depotCoords);
            }
            var result = new CDrone(depot, depotCoords, depotIndex);
            result.id = id;
            return result;
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(id);
            output.Write(depotCoords);
            output.Write(depotIndex);
        }

        internal void Decode(BinaryReader input) 
        {
            id = input.ReadInt32();
            depotCoords = new int2(input.ReadInt32(), input.ReadInt32());
            depotIndex = input.ReadInt32();
        }
    }
}
