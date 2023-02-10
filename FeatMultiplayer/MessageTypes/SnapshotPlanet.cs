using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    public struct SnapshotPlanet
    {
        internal string name;
        internal int sf6ContainerCount;
        internal int nf3ContainerCount;
        internal readonly List<float> dailySF6 = new();
        internal readonly List<float> dailyNF3 = new();
        internal readonly List<float> dailyTemperature = new();

        public SnapshotPlanet() { }

        internal void GetSnapshot()
        {
            name = GPlanet.name;
            sf6ContainerCount = GPlanet.sf6_NbContainersInAtm;
            nf3ContainerCount = GPlanet.nf3_NbContainersInAtm;
            dailySF6.AddRange(GPlanet.dailySF6);
            dailyNF3.AddRange(GPlanet.dailyNF3);
            dailyTemperature.AddRange(GPlanet.dailyTemperature);
        }

        internal void ApplySnapshot()
        {
            GPlanet.name = name;
            GPlanet.sf6_NbContainersInAtm = sf6ContainerCount;
            GPlanet.nf3_NbContainersInAtm = nf3ContainerCount;

            GPlanet.dailySF6.Clear();
            GPlanet.dailySF6.AddRange(dailySF6);
            GPlanet.dailyNF3.Clear();
            GPlanet.dailyNF3.AddRange(dailyNF3);
            GPlanet.dailyTemperature.Clear();
            GPlanet.dailyTemperature.AddRange(dailyTemperature);
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(name);
            output.Write(sf6ContainerCount);
            output.Write(nf3ContainerCount);

            output.Write(dailySF6.Count);
            foreach (var v in dailySF6)
            {
                output.Write(v);
            }

            output.Write(dailyNF3.Count);
            foreach (var v in dailyNF3)
            {
                output.Write(v);
            }
            output.Write(dailyTemperature.Count);
            foreach (var v in dailyTemperature)
            {
                output.Write(v);
            }
        }

        internal void Decode(BinaryReader input)
        {
            name = input.ReadString();
            sf6ContainerCount = input.ReadInt32();
            nf3ContainerCount = input.ReadInt32();

            var c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                dailySF6.Add(input.ReadSingle());
            }
            c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                dailyNF3.Add(input.ReadSingle());
            }
            c = input.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                dailyTemperature.Add(input.ReadSingle());
            }
        }
    }

}
