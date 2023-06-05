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
        internal int2 copyFrom = int2.negative;
        internal byte overrideId;
        internal bool allowRecipePick;
        internal bool firstBuild;

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords);
            output.Write(id);
            output.Write(copyFrom);
            output.Write(overrideId);
            output.Write(allowRecipePick);
            output.Write(firstBuild);
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            id = input.ReadByte();
            copyFrom = input.ReadInt2();
            overrideId = input.ReadByte();
            allowRecipePick = input.ReadBoolean();
            firstBuild = input.ReadBoolean();
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
                .Append(nameof(copyFrom)).Append(" = ").Append(copyFrom).Append(", ")
                .Append(nameof(overrideId)).Append(" = ").Append(overrideId).Append(", ")
                .Append(nameof(allowRecipePick)).Append(" = ").Append(allowRecipePick).Append(", ")
                .Append(nameof(firstBuild)).Append(" = ").Append(firstBuild)
                ;

            return sb.ToString();
        }
    }
}
