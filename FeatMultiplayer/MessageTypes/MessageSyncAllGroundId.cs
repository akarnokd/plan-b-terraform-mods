// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllGroundId : MessageSyncByteArray
    {
        const string messageCode = "SyncAllGroundId";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override byte[,] GetData()
        {
            return GHexes.groundId;
        }

        public override string MessageCode() => messageCode;

        public override byte[] MessageCodeBytes() => messageCodeBytes;

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllGroundId();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
