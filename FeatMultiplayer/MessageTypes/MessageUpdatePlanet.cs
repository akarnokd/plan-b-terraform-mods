// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdatePlanet : MessageBase
    {
        const string messageCode = "UpdatePlanet";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal readonly SnapshotPlanet snapshot = new SnapshotPlanet();

        void Decode(BinaryReader input)
        {
            snapshot.Decode(input);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdatePlanet();

            msg.Decode(input);

            message = msg;
            return true;
        }

        internal void GetSnapshot(int fromIndex)
        {
            snapshot.name = GPlanet.name;
            snapshot.sf6ContainerCount = GPlanet.sf6_NbContainersInAtm;
            snapshot.nf3ContainerCount = GPlanet.nf3_NbContainersInAtm;

            for (int i = fromIndex; i < GPlanet.dailySF6.Count; i++)
            {
                snapshot.dailySF6.Add(GPlanet.dailySF6[i]);
            }
            for (int i = fromIndex; i < GPlanet.dailyNF3.Count; i++)
            {
                snapshot.dailyNF3.Add(GPlanet.dailyNF3[i]);
            }
            for (int i = fromIndex; i < GPlanet.dailyTemperature.Count; i++)
            {
                snapshot.dailyTemperature.Add(GPlanet.dailyTemperature[i]);
            }
        }

        internal void ApplySnapshot()
        {
            GPlanet.name = snapshot.name;
            GPlanet.sf6_NbContainersInAtm = snapshot.sf6ContainerCount;
            GPlanet.nf3_NbContainersInAtm = snapshot.nf3ContainerCount;

            GPlanet.dailySF6.AddRange(snapshot.dailySF6);
            GPlanet.dailyNF3.AddRange(snapshot.dailyNF3);
            GPlanet.dailyTemperature.AddRange(snapshot.dailyTemperature);
        }

        public override void Encode(BinaryWriter output)
        {
            snapshot.Encode(output);
        }
    }
}
