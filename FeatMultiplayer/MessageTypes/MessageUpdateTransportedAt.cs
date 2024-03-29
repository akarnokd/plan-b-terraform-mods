﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageUpdateTransportedAt : MessageBase
    {
        const string messageCode = "UpdateTransportedAt";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int2 coords;
        internal string codeName;

        public void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            var content = Plugin.ContentAt(coords);
            if (content is CItem_WayStop)
            {
                var line = SSingleton<SWays>.Inst.GetLine(coords);

                codeName = (line?.itemTransported?.codeName) ?? "";
            }
        }

        public void CreateRequest(int2 coords, string codeName)
        {
            this.coords = coords;
            this.codeName = codeName;
        }

        public void ApplySnapshot()
        {
            var lookup = Plugin.GetItemsDictionary();
            if (lookup.TryGetValue(codeName, out var item))
            {
                SSingleton<SWays>.Inst.GetLine(coords)?.SetItemTransported(item);
            }
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(codeName);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateTransportedAt();
            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            codeName = input.ReadString();
        }
    }
}
