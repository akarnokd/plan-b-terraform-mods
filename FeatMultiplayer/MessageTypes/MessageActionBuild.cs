// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageActionBuild : MessageBase
    {
        const string messageCode = "ActionBuild";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal byte id;
        internal bool copyMode;
        internal int2 copyFrom = int2.negative;

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(id);
            output.Write(copyMode);
            output.Write(copyFrom);
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            id = input.ReadByte();
            copyMode = input.ReadBoolean();
            copyFrom = input.ReadInt2();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageActionBuild();
            msg.Decode(input);
            message = msg;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(nameof(coords)).Append(" = ").Append(coords).Append(", ")
                .Append(nameof(id)).Append(" = ").Append(id).Append(", ")
                .Append(nameof(copyMode)).Append(" = ").Append(copyMode).Append(", ")
                .Append(nameof(copyFrom)).Append(" = ").Append(copyFrom)
                ;

            return sb.ToString();
        }
    }
}
