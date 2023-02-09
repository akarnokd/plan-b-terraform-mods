using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FeatMultiplayer.MessageSyncAllItems;

namespace FeatMultiplayer
{
    internal class MessageSyncAllWays : MessageSync
    {
        const string messageCode = "SyncAllWays";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int lineIdMax;
        internal int vehicleIdMax;
        internal readonly List<LineSnapshot> lines = new();

        internal override void GetSnapshot()
        {
            lineIdMax = CLine.idMax;
            vehicleIdMax = CVehicle.idMax;

            for (int i = 1; i < GWays.lines.Count; i++) 
            { 
                var ls = new LineSnapshot();
                ls.GetSnapshot(GWays.lines[i]);
                lines.Add(ls);
            }
        }

        internal override void ApplySnapshot()
        {
            GWays.lines.Clear();
            GWays.lines.Add(null);
            SMisc.DestroyChildren(CVehicle.vehiclesParentTransform, null);

            var itemDictionary = GetItemsDictionary();

            foreach (var line in lines)
            {
                GWays.lines.Add(line.Create(itemDictionary));
            }

            // Update it here because the constructors increment
            CLine.idMax = lineIdMax;
            CVehicle.idMax = vehicleIdMax;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(lineIdMax);
            output.Write(vehicleIdMax);
            output.Write(lines.Count);

            foreach (var line in lines)
            {
                line.Encode(output);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllWays();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            lineIdMax = input.ReadInt32();
            vehicleIdMax = input.ReadInt32();
            var linesCount = input.ReadInt32();

            for (int i = 0; i < linesCount; i++)
            {
                var ln = new LineSnapshot();
                ln.Decode(input);
                lines.Add(ln);
            }
        }

        internal class LineSnapshot
        {
            internal int id;
            internal string itemStopOrigin;
            internal string itemTransported;
            internal readonly List<StopSnapshot> stops = new();
            internal readonly List<NodeSnapshot> nodes = new();
            internal readonly List<VehicleSnapshot> vehicles = new();

            internal void GetSnapshot(CLine line)
            {
                id = line.id;
                itemStopOrigin = line.itemStopOrigin.codeName;
                itemTransported = line.itemTransported?.codeName ?? "";

                foreach (var stop in line.stops)
                {
                    var sstop = new StopSnapshot();
                    stops.Add(sstop);
                    sstop.GetSnapshot(stop);
                }

                foreach (var node in line.nodes)
                {
                    var snode = new NodeSnapshot();
                    nodes.Add(snode);
                    snode.GetSnapshot(node);
                }

                foreach (var vehicle in line.vehicles)
                {
                    var svehicle = new VehicleSnapshot();
                    vehicles.Add(svehicle);
                    svehicle.GetSnapshot(vehicle);
                }
            }

            internal CLine Create(Dictionary<string, CItem> itemDictionary)
            {
                var result = new CLine(int2.negative);
                result.stops.Clear();
                
                result.id = id;
                if (itemDictionary.TryGetValue(itemStopOrigin, out var item))
                {
                    result.itemStopOrigin = item as CItem_WayStop;
                }
                itemDictionary.TryGetValue(itemTransported, out result.itemTransported);

                foreach (var stop in stops)
                {
                    result.stops.Add(stop.Create());
                }

                foreach (var node in nodes)
                {
                    result.nodes.Add(node.Create());
                }

                foreach (var vehicle in vehicles)
                {
                    result.vehicles.Add(vehicle.Create(result, itemDictionary));
                }

                result.UpdateStopDataOrginEnd(false, false);

                return result;
            }

            internal void Encode(BinaryWriter output)
            {
                output.Write(id);
                output.Write(itemStopOrigin);
                output.Write(itemTransported);

                output.Write(stops.Count);

                foreach (var stop in stops)
                {
                    stop.Encode(output);
                }

                output.Write(nodes.Count);
                foreach (var node in nodes)
                {
                    node.Encode(output);
                }

                output.Write(vehicles.Count);

                foreach (var vehicle in vehicles)
                {
                    vehicle.Encode(output);
                }
            }

            internal void Decode(BinaryReader input)
            {
                id = input.ReadInt32();
                itemStopOrigin = input.ReadString();
                itemTransported = input.ReadString();

                var stopCount = input.ReadInt32();
                for (int i = 0; i < stopCount; i++) 
                {
                    var stop = new StopSnapshot();
                    stop.Decode(input);
                    stops.Add(stop);
                }
                var nodesCount = input.ReadInt32();
                for (int i = 0; i < nodesCount; i++) {
                    var node = new NodeSnapshot();
                    node.Decode(input);
                    nodes.Add(node);
                }
                var vehiclesCount = input.ReadInt32();
                for (int i = 0; i < vehiclesCount; i++)
                {
                    var vehicle = new VehicleSnapshot();
                    vehicle.Decode(input);
                    vehicles.Add(vehicle);
                }
            }
        }        
        
        internal class StopSnapshot
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

            internal void Encode(BinaryWriter output)
            {
                output.Write(coords.x);
                output.Write(coords.y);
                output.Write(t);
            }

            internal void Decode(BinaryReader input)
            {
                coords = new int2(input.ReadInt32(), input.ReadInt32());
                t = input.ReadInt32();
            }
        }

        internal class NodeSnapshot
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

            internal void Encode(BinaryWriter output)
            {
                output.Write(coords.x);
                output.Write(coords.y);
                output.Write(tExit);
                output.Write(posMiddle);
                output.Write(posExit);
            }

            internal void Decode(BinaryReader input)
            {
                coords = new int2(input.ReadInt32(), input.ReadInt32());
                tExit = input.ReadSingle();
                posMiddle = input.ReadVector3();
                posExit = input.ReadVector3();
            }
        }

        internal class VehicleSnapshot
        {
            internal int id;
            internal int pathI;
            internal float t;
            internal float speed;
            internal int stopObjective;
            internal float loadWait;
            internal readonly List<StackSnapshot> stacks = new();

            internal void Encode(BinaryWriter output)
            {
                output.Write(id);
                output.Write(pathI);
                output.Write(t);
                output.Write(speed);
                output.Write(stopObjective);
                output.Write(loadWait);

                output.Write(stacks.Count);
                foreach (var s in stacks)
                {
                    s.Encode(output);
                }
            }

            internal void Decode(BinaryReader input)
            {
                id = input.ReadInt32();
                pathI = input.ReadInt32();
                t = input.ReadSingle();
                speed = input.ReadSingle();
                stopObjective = input.ReadInt32();
                loadWait = input.ReadSingle();

                int c = input.ReadInt32();
                for (int i = 0; i < c; i++)
                {
                    var ssnp = new StackSnapshot();
                    ssnp.Decode(input);
                    stacks.Add(ssnp);
                }
            }

            internal void GetSnapshot(CVehicle vehicle)
            {
                id = vehicle.id;
                pathI = vehicle.pathI;
                t = vehicle.t;
                speed = vehicle.speed;
                stopObjective = Haxx.cVehicleStopObjective(vehicle);
                loadWait = Haxx.cVehicleLoadWait(vehicle);

                foreach (var s in vehicle.stacks.stacks)
                {
                    var ssn = new StackSnapshot();
                    ssn.GetSnapshot(in s);
                    stacks.Add(ssn);
                }
            }

            internal CVehicle Create(CLine parent, Dictionary<string, CItem> itemDictionary)
            {
                var result = new CVehicle(parent);
                result.id = id;
                result.pathI= pathI;
                result.t = t;
                result.speed = speed;
                Haxx.cVehicleStopObjective(result) = stopObjective;
                Haxx.cVehicleLoadWait(result) = loadWait;

                for (int i = 0; i < stacks.Count; i++)
                {
                    StackSnapshot s = stacks[i];

                    s.ApplySnapshot(ref result.stacks.stacks[i], itemDictionary);
                }

                return result;
            }
        }
    }
}
