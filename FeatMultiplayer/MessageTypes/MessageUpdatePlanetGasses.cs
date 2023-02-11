// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdatePlanetGasses : MessageBase
    {
        const string messageCode = "UpdatePlanetGasses";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int sf6ContainerCount;
        internal int nf3ContainerCount;

        public void GetSnapshot()
        {
            sf6ContainerCount = GPlanet.sf6_NbContainersInAtm;
            nf3ContainerCount = GPlanet.nf3_NbContainersInAtm;
        }

        public void ApplySnapshot()
        {
            GPlanet.sf6_NbContainersInAtm = sf6ContainerCount;
            GPlanet.nf3_NbContainersInAtm = nf3ContainerCount;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(sf6ContainerCount);
            output.Write(nf3ContainerCount);
        }

        void Decode(BinaryReader input)
        {
            sf6ContainerCount = input.ReadInt32();
            nf3ContainerCount = input.ReadInt32();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdatePlanetGasses();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
