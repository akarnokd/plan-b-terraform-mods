// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotDroneLive
    {
        internal int id;
        internal int depotIndex;
        internal int2 depotCoords;
        internal CDrone.State state;
        internal string depotItem;

        internal CTransform startTransform;
        internal CTransform endTransform;
        internal double startTime;
        internal double endTime;

        internal bool HasChangedSince(SnapshotDroneLive other)
        {
            return this.state != other.state
                || this.depotItem != other.depotItem
                || this.startTransform.pos != other.startTransform.pos
                || this.startTransform.rot != other.startTransform.rot
                || this.endTransform.pos != other.endTransform.pos
                || this.endTransform.rot != other.endTransform.rot
                || this.startTime != other.startTime
                || this.endTime != other.endTime;
        }

        internal void GetSnapshot(CDrone drone)
        {
            id = drone.id;
            depotIndex = Haxx.cDroneDroneDepotIndex.Invoke(drone);
            depotCoords = drone.depotCoords;
            state = drone.state;
            depotItem = Haxx.cDroneDepot.Invoke(drone).GetStack(depotCoords).item ?.codeName ?? "";
            startTransform = Haxx.cDroneStartTransform.Invoke(drone);
            endTransform = Haxx.cDroneEndTransform.Invoke(drone);
            startTime = Haxx.cDroneStartTime.Invoke(drone);
            endTime = Haxx.cDroneEndTime.Invoke(drone);
        }

        internal void ApplySnapshot(CDrone drone, Dictionary<string, CItem> itemLookup)
        {
            drone.id = id;
            Haxx.cDroneDroneDepotIndex.Invoke(drone) = depotIndex;
            drone.depotCoords = depotCoords;
            drone.state = state;
            // no longer matters???
            // itemLookup.TryGetValue(depotItem, out drone.depotItem);
            Haxx.cDroneStartTransform.Invoke(drone) = startTransform;
            Haxx.cDroneEndTransform.Invoke(drone) = endTransform;
            Haxx.cDroneStartTime.Invoke(drone) = startTime;
            Haxx.cDroneEndTime.Invoke(drone) = endTime;

            //Plugin.LogDebug("Drone " + id + ", State = " + state);
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(id);
            output.Write((byte)(depotIndex * 16 + state));
            output.WriteShort(depotCoords);
            output.Write(depotItem);
            output.Write(startTransform);
            output.Write(endTransform);
            output.Write(startTime);
            output.Write(endTime);
        }

        internal void Decode(BinaryReader input)
        {
            id = input.ReadInt32();
            byte depotIndexState = input.ReadByte();
            depotIndex = (depotIndexState & 0xF0) >> 4;
            depotCoords = input.ReadInt2Short();
            state = (CDrone.State)(depotIndexState & 0x0F);
            depotItem = input.ReadString();
            startTransform = input.ReadCTransform();
            endTransform = input.ReadCTransform();
            startTime = input.ReadDouble();
            endTime = input.ReadDouble();
        }
    }
}
