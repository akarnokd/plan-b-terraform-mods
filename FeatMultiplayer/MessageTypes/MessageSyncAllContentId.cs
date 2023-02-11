// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllContentId : MessageSyncByteArray
    {
        const string messageCode = "SyncAllContentId";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal override byte[,] GetData()
        {
            return GHexes.contentId;
        }

        public override string MessageCode() => messageCode;

        public override byte[] MessageCodeBytes() => messageCodeBytes;

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllContentId();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
