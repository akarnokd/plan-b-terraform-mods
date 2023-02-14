// Copyright (c) David Karnok, 2023
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
        internal double simuPlanetTime_LastFrame;
        internal double simuUnitsTime;
        internal double simuUnitsTime_LastFrame;
        internal float timePlayed;
        internal float timeScale;

        internal override void GetSnapshot()
        {
            simuPlanetTime = GMain.simuPlanetTime;
            simuPlanetTime_LastFrame = GMain.simuPlanetTime_LastFrame;
            simuUnitsTime = GMain.simuUnitsTime;
            simuUnitsTime_LastFrame = GMain.simuUnitsTime_LastFrame;
            timePlayed = GMain.timePlayed;
            timeScale = Time.timeScale;
        }

        internal override void ApplySnapshot()
        {
            GMain.simuPlanetTime = simuPlanetTime;
            GMain.simuPlanetTime_LastFrame = simuPlanetTime_LastFrame;
           
            GMain.simuUnitsTime = simuUnitsTime;
            GMain.simuUnitsTime_LastFrame = simuUnitsTime_LastFrame;
            //GMain.simuUnitsTime_LastFrame = simuUnitsTime;

            GMain.timePlayed = timePlayed;

            Time.timeScale = timeScale;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(simuPlanetTime);
            output.Write(simuPlanetTime_LastFrame);
            output.Write(simuUnitsTime);
            output.Write(simuUnitsTime_LastFrame);
            output.Write(timePlayed);
            output.Write(timeScale);
        }

        void Decode(BinaryReader input)
        {
            simuPlanetTime = input.ReadDouble();
            simuPlanetTime_LastFrame = input.ReadDouble();
            simuUnitsTime = input.ReadDouble();
            simuUnitsTime_LastFrame = input.ReadDouble();
            timePlayed = input.ReadSingle();
            timeScale = input.ReadSingle();
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdateTime();

            msg.Decode(input);

            message = msg;
            return true;
        }

        
    }
}
