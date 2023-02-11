// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllAltitude : MessageSyncFloatArray
    {
        const string messageCode = "SyncAllAltitude";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override float[,] GetData()
        {
            return GHexes.altitude;
        }

        public override string MessageCode()
        {
            return messageCode;
        }

        public override byte[] MessageCodeBytes()
        {
            return messageCodeBytes;
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllAltitude();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
