// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageRenameCity : MessageBase
    {
        const string messageCode = "RenameCity";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int id;
        internal string name;

        public void GetSnapshot(CCity city)
        {
            id = city.cityId;
            name = city.name;
        }

        public void ApplySnapshot(CCity city)
        {
            city.name = name;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(id);
            output.Write(name);
        }

        void Decode(BinaryReader input)
        {
            id = input.ReadInt32();
            name = input.ReadString();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageRenameCity();
            msg.Decode(input);
            message = msg;
            return true;
        }
    }
}
