using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageSyncAllPlanet : MessageSync
    {
        const string messageCode = "SyncAllPlanet";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal string name;
        internal int sf6ContainerCount;
        internal int nf3ContainerCount;
        internal readonly List<float> dailySF6 = new();
        internal readonly List<float> dailyNF3 = new();
        internal readonly List<float> dailyTemperature = new();

        internal override void GetSnapshot()
        {
            name = GPlanet.name;
            sf6ContainerCount = GPlanet.sf6_NbContainersInAtm;
            nf3ContainerCount = GPlanet.nf3_NbContainersInAtm;
            dailySF6.AddRange(GPlanet.dailySF6);
            dailyNF3.AddRange(GPlanet.dailyNF3);
            dailyTemperature.AddRange(GPlanet.dailyTemperature);
        }

        internal override void ApplySnapshot()
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

        public override void Encode(BinaryWriter output)
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

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllPlanet();

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

            message = msg;
            return true;
        }
    }
}
