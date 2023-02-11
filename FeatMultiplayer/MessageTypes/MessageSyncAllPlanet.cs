// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllPlanet : MessageSync
    {
        const string messageCode = "SyncAllPlanet";
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
            var msg = new MessageSyncAllPlanet();

            msg.Decode(input);

            message = msg;
            return true;
        }

        internal override void GetSnapshot()
        {
            snapshot.GetSnapshot();
        }

        internal override void ApplySnapshot()
        {
            snapshot.ApplySnapshot();
        }

        public override void Encode(BinaryWriter output)
        {
            snapshot.Encode(output);
        }
    }
}
