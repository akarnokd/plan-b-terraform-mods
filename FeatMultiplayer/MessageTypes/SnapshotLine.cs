// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;

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
        internal bool updateNodes;

        internal void GetSnapshot(CLine line)
        {
            id = line.id;
            itemStopOrigin = line.ItemStop?.codeName ?? "";
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
            updateNodes = true;
        }

        internal bool HaveNodesChanged(List<SnapshotNode> nodes)
        {
            if (this.nodes.Count != nodes.Count)
            {
                return true;
            }
            for (int i = 0; i < this.nodes.Count; i++)
            {
                var n = this.nodes[i];
                var m = nodes[i];

                if (n.HasChanged(m))
                {
                    return true;
                }
            }
            return false;
        }
        internal CLine Create(Dictionary<string, CItem> itemDictionary)
        {
            var result = new CLine(int2.negative, null);
            result.stops.Clear();

            result.id = id;
            if (itemDictionary.TryGetValue(itemStopOrigin, out var item))
            {
                Haxx.cLineItemStop(result) = item as CItem_WayStop;
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

        internal void ApplySnapshot(CLine line, Dictionary<string, CItem> itemDictionary, bool updateBlocks)
        {
            line.id = id;
            if (itemDictionary.TryGetValue(itemStopOrigin, out var item))
            {
                Haxx.cLineItemStop(line) = item as CItem_WayStop;
            }
            else
            {
                Haxx.cLineItemStop(line) = null;
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

            if (updateNodes)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (line.nodes.Count == i)
                    {
                        line.nodes.Add(new CLine.Node(int2.negative));
                    }
                    nodes[i].ApplySnapshot(line.nodes[i]);
                }
                line.nodes.RemoveRange(nodes.Count, line.nodes.Count - nodes.Count);
            }

            for (int i = 0; i < vehicles.Count; i++)
            {
                if (line.vehicles.Count == i)
                {
                    line.vehicles.Add(new CVehicle(line));
                }
                vehicles[i].ApplySnapshot(line.vehicles[i], itemDictionary);
            }
            line.vehicles.RemoveRange(vehicles.Count, line.vehicles.Count - vehicles.Count);

            line.UpdateStopDataOrginEnd(updateBlocks, false); // FIXME not sure about erase?!
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
            output.Write(updateNodes);
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
            updateNodes = input.ReadBoolean();
        }
    }
}
