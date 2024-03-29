﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllMain : MessageSync
    {
        const string messageCode = "SyncAllMain";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal double simuPlanetTime;
        internal double simuUnitsTime;
        internal float timePlayed;

        internal override void GetSnapshot()
        {
            simuPlanetTime = GMain.simuPlanetTime;
            simuUnitsTime = GMain.simuUnitsTime;
            timePlayed = GMain.timePlayed;
        }

        internal override void ApplySnapshot()
        {
            GMain.simuPlanetTime = simuPlanetTime;
            GMain.simuPlanetTime_LastFrame = simuPlanetTime;

            GMain.simuUnitsTime = simuUnitsTime;
            GMain.simuUnitsTime_LastFrame = simuUnitsTime;

            GMain.timePlayed = timePlayed;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(simuPlanetTime);
            output.Write(simuUnitsTime);
            output.Write(timePlayed);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllMain();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            simuPlanetTime = input.ReadDouble();
            simuUnitsTime = input.ReadDouble();
            timePlayed = input.ReadSingle();
        }
    }
}
