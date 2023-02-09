using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageSyncAllDrones : MessageSync
    {
        const string messageCode = "SyncAllDrones";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int maxId;
        internal readonly List<DroneSnapshot> drones = new();

        internal override void GetSnapshot()
        {
            maxId = CDrone.idMax;
            foreach (var drone in GDrones.drones)
            {
                var ds = new DroneSnapshot();
                ds.GetSnapshot(drone);
                drones.Add(ds);
            }
        }

        internal override void ApplySnapshot()
        {
            GDrones.modelsPool.Clear();
            GDrones.drones.Clear();

            var sworld = SSingleton<SWorld>.Inst;
            var sdrones = SSingleton<SDrones>.Inst;
            var addDroneInGrid = AccessTools.MethodDelegate<Action<CDrone>>(Haxx.sDronesAddDroneInGrid, sdrones);

            foreach (var ds in drones)
            {
                var drone = ds.Create(sworld);
                GDrones.drones.Add(drone);
                addDroneInGrid(drone);
            }
            // set max here because the CDrone constructor we use increments
            CDrone.idMax = maxId;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(maxId);
            output.Write(drones.Count);
            foreach (var drone in drones)
            {
                output.Write(drone.id);
                output.Write(drone.depotCoords.x);
                output.Write(drone.depotCoords.y);
                output.Write(drone.depotIndex);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllDrones();

            maxId = input.ReadInt32();

            int c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                var drone = new DroneSnapshot();
                drone.id = input.ReadInt32();
                drone.depotCoords = new int2(input.ReadInt32(), input.ReadInt32());
                drone.depotIndex = input.ReadInt32();
                drones.Add(drone);
            }

            message = msg;
            return true;
        }

        internal class DroneSnapshot
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
}
