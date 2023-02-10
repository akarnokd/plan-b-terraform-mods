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

        internal void GetSnapshot(CDrone drone)
        {
            id = drone.id;
            depotIndex = Haxx.cDroneDroneDepotIndex.Invoke(drone);
            depotCoords = drone.depotCoords;
            state = drone.state;
            depotItem = drone.depotItem?.codeName ?? "";
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
            itemLookup.TryGetValue(depotItem, out drone.depotItem);
            Haxx.cDroneStartTransform.Invoke(drone) = startTransform;
            Haxx.cDroneEndTransform.Invoke(drone) = endTransform;
            Haxx.cDroneStartTime.Invoke(drone) = startTime;
            Haxx.cDroneEndTime.Invoke(drone) = endTime;
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(id);
            output.Write(depotIndex);
            output.Write(depotCoords);
            output.Write((byte)state);
            output.Write(depotItem);
            output.Write(startTransform);
            output.Write(endTransform);
            output.Write(startTime);
            output.Write(endTime);
        }

        internal void Decode(BinaryReader input)
        {
            id = input.ReadInt32();
            depotIndex = input.ReadInt32();
            depotCoords = input.ReadInt2();
            state = (CDrone.State)input.ReadByte();
            depotItem = input.ReadString();
            startTransform = input.ReadCTransform();
            endTransform = input.ReadCTransform();
            startTime = input.ReadDouble();
            endTime = input.ReadDouble();
        }
    }
}
