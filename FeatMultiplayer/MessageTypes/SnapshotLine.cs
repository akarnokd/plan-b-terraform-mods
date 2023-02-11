using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FeatMultiplayer
{
    internal class SnapshotLine
    {
        internal int id;
        internal string itemStopOrigin;
        internal string itemTransported;
        internal readonly List<SnapshotStop> stops = new();
        internal readonly List<SnapshotNode> nodes = new();
        internal readonly List<SnapshotVehicle> vehicles = new();

        internal void GetSnapshot(CLine line)
        {
            id = line.id;
            itemStopOrigin = line.itemStopOrigin.codeName;
            itemTransported = line.itemTransported?.codeName ?? "";

            foreach (var stop in line.stops)
            {
                var sstop = new SnapshotStop();
                stops.Add(sstop);
                sstop.GetSnapshot(stop);
            }

            foreach (var node in line.nodes)
            {
                var snode = new SnapshotNode();
                nodes.Add(snode);
                snode.GetSnapshot(node);
            }

            foreach (var vehicle in line.vehicles)
            {
                var svehicle = new SnapshotVehicle();
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

        internal void ApplySnapshot(CLine line, Dictionary<string, CItem> itemDictionary)
        {
            line.id = id;
            if (itemDictionary.TryGetValue(itemStopOrigin, out var item))
            {
                line.itemStopOrigin = item as CItem_WayStop;
            }
            else
            {
                line.itemStopOrigin = null;
            }
            itemDictionary.TryGetValue(itemTransported, out line.itemTransported);

            for (int i = 0; i < stops.Count; i++)
            {
                if (line.stops.Count == i)
                {
                    line.stops.Add(new CLine.Stop(int2.negative));
                }
                stops[i].ApplySnapshot(line.stops[i]);
            }
            line.stops.RemoveRange(stops.Count, line.stops.Count - stops.Count);

            for (int i = 0; i < nodes.Count; i++) {
                if (line.nodes.Count == i)
                {
                    line.nodes.Add(new CLine.Node(int2.negative));
                }
                nodes[i].ApplySnapshot(line.nodes[i]);
            }
            line.nodes.RemoveRange(nodes.Count, line.nodes.Count - nodes.Count);

            for (int i = 0; i < vehicles.Count; i++)
            {
                if (line.vehicles.Count == i)
                {
                    line.vehicles.Add(new CVehicle(line));
                }
                vehicles[i].ApplySnapshot(line.vehicles[i], itemDictionary);
            }
            line.vehicles.RemoveRange(vehicles.Count, line.vehicles.Count - vehicles.Count);

            line.UpdateStopDataOrginEnd(true, false); // FIXME not sure about erase?!
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
                var stop = new SnapshotStop();
                stop.Decode(input);
                stops.Add(stop);
            }
            var nodesCount = input.ReadInt32();
            for (int i = 0; i < nodesCount; i++)
            {
                var node = new SnapshotNode();
                node.Decode(input);
                nodes.Add(node);
            }
            var vehiclesCount = input.ReadInt32();
            for (int i = 0; i < vehiclesCount; i++)
            {
                var vehicle = new SnapshotVehicle();
                vehicle.Decode(input);
                vehicles.Add(vehicle);
            }
        }
    }
}
