// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllWaterFlow : MessageSyncFloatArray
    {
        const string messageCode = "SyncAllWaterFlow";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override float[,] GetData()
        {
            return GHexes.waterflow;
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
            var msg = new MessageSyncAllWaterFlow();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
