using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotVehicle
    {
        internal int id;
        internal int pathI;
        internal float t;
        internal float speed;
        internal int stopObjective;
        internal float loadWait;
        internal readonly List<SnapshotStack> stacks = new();

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
                var ssnp = new SnapshotStack();
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
                var ssn = new SnapshotStack();
                ssn.GetSnapshot(in s);
                stacks.Add(ssn);
            }
        }

        internal CVehicle Create(CLine parent, Dictionary<string, CItem> itemDictionary)
        {
            var result = new CVehicle(parent);
            result.id = id;
            result.pathI = pathI;
            result.t = t;
            result.speed = speed;
            Haxx.cVehicleStopObjective(result) = stopObjective;
            Haxx.cVehicleLoadWait(result) = loadWait;

            for (int i = 0; i < stacks.Count; i++)
            {
                SnapshotStack s = stacks[i];

                s.ApplySnapshot(ref result.stacks.stacks[i], itemDictionary);
            }

            return result;
        }

        internal void ApplySnapshot(CVehicle vehicle, Dictionary<string, CItem> itemDictionary)
        {
            vehicle.id = id;
            vehicle.pathI = pathI;
            vehicle.t = t;
            vehicle.speed = speed;
            Haxx.cVehicleStopObjective(vehicle) = stopObjective;
            Haxx.cVehicleLoadWait(vehicle) = loadWait;
            for (int i = 0; i < stacks.Count; i++)
            {
                SnapshotStack s = stacks[i];

                s.ApplySnapshot(ref vehicle.stacks.stacks[i], itemDictionary);
            }
        }

    }
}
