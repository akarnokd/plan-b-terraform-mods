// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class SnapshotTransportStack
    {
        internal int stackId;
        internal readonly SnapshotStack stack = new();
        internal int2 coords;
        internal int vehicleId;

        internal void GetSnapshot(in CDrone.TransportStep step)
        {
            coords = step.coords;
            stackId = step.stackId;
            stack.GetSnapshot(step.stacks.stacks[stackId]);
            vehicleId = step.vehicle?.id ?? -1;
        }

        internal void ApplySnapshot(Dictionary<int, CVehicle> vehicleLookup, Dictionary<string, CItem> itemLookup)
        {
            if (vehicleId >= 0)
            {
                if (vehicleLookup.TryGetValue(vehicleId, out var vehicle))
                {
                    stack.ApplySnapshot(ref vehicle.stacks.stacks[stackId], itemLookup);
                }
            }
            else
            {
                var gstacks = GHexes.stacks[coords.x, coords.y];
                if (gstacks != null)
                {
                    stack.ApplySnapshot(ref gstacks.stacks[stackId], itemLookup);
                }
            }
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(coords);
            output.Write(stackId);
            output.Write(vehicleId);
            stack.Encode(output);
        }
        internal void Decode(BinaryReader input) {
            coords = input.ReadInt2();
            stackId = input.ReadInt32();
            vehicleId = input.ReadInt32();
            stack.Decode(input);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb
                .Append(nameof(coords)).Append(" = ").Append(coords).Append(", ")
                .Append(nameof(stackId)).Append(" = ").Append(stackId).Append(", ")
                .Append(nameof(vehicleId)).Append(" = ").Append(vehicleId).Append(", ")
                .Append(nameof(stack)).Append(" = ").Append(stack)
                ;
            return sb.ToString();
        }
    }
}
