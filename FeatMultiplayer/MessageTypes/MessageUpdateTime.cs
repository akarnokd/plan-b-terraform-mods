﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;
using UnityEngine;

namespace FeatMultiplayer
{
    internal class MessageUpdateTime : MessageSync
    {
        const string messageCode = "UpdateTime";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal double simuPlanetTime;
        internal double simuUnitsTime;
        internal float timePlayed;
        internal float timeScale;

        internal override void GetSnapshot()
        {
            simuPlanetTime = GMain.simuPlanetTime;
            simuUnitsTime = GMain.simuUnitsTime;
            timePlayed = GMain.timePlayed;
            timeScale = Time.timeScale;
        }

        internal override void ApplySnapshot()
        {
            GMain.simuPlanetTime = simuPlanetTime;
            GMain.simuPlanetTime_LastFrame = simuPlanetTime;

            GMain.simuUnitsTime = simuUnitsTime;
            GMain.simuUnitsTime_LastFrame = simuUnitsTime;

            GMain.timePlayed = timePlayed;

            Time.timeScale = timeScale;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(simuPlanetTime);
            output.Write(simuUnitsTime);
            output.Write(timePlayed);
            output.Write(timeScale);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateTime();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            simuPlanetTime = input.ReadDouble();
            simuUnitsTime = input.ReadDouble();
            timePlayed = input.ReadSingle();
            timeScale = input.ReadSingle();
        }
    }
}