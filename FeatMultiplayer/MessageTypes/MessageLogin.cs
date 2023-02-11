// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageLogin : MessageBase
    {
        const string messageCode = "Login";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal string userName;

        internal string password;

        public override void Encode(BinaryWriter output)
        {
            output.Write(userName);
            output.Write(password);
        }


        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageLogin();
            msg.userName = input.ReadString();
            msg.password = input.ReadString();
            message = msg;
            return true;
        }
    }
}
