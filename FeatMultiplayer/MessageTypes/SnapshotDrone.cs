// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

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
            var depot = sworld.GetContent(depotCoords) as CItem_ContentDepot;
            var result = new CDrone(depot, depotCoords, depotIndex);
            result.id = id;
            return result;
        }
    }
}
