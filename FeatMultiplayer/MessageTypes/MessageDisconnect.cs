// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    /// <summary>
    /// Message indicating the sender loop should disconnect.
    /// </summary>
    internal class MessageDisconnect : MessageBase
    {
        /// <summary>
        /// Singleton instance as this is a marker message.
        /// </summary>
        internal static readonly MessageDisconnect Instance = new MessageDisconnect();

        const string messageCode = "Disconnect";
        static readonly byte[] bytes = Encoding.UTF8.GetBytes(messageCode);

        public override void Encode(BinaryWriter output)
        {
            throw new InvalidOperationException();
        }

        public override string MessageCode()
        {
            return messageCode;
        }

        public override byte[] MessageCodeBytes()
        {
            return bytes;
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            throw new InvalidOperationException();
        }
    }
}
