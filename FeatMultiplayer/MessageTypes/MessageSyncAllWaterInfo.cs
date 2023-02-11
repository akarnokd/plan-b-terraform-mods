// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllWaterInfo : MessageSync
    {
        const string messageCode = "SyncAllWaterInfo";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal double waterInGroundEvaporated;

        internal float[] supergridWater;

        internal override void GetSnapshot()
        {
            waterInGroundEvaporated = GWater.waterInGroundEvaporated;
            var s = GWater.supergridSize;
            supergridWater = new float[s.x * s.y];
            Buffer.BlockCopy(GWater.supergridWater, 0, supergridWater, 0, s.x * s.y * 4);
        }

        internal override void ApplySnapshot()
        {
            GWater.waterInGroundEvaporated = waterInGroundEvaporated;
            var s = GWater.supergridSize;
            Buffer.BlockCopy(supergridWater, 0, GWater.supergridWater, 0, s.x * s.y * 4);
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(waterInGroundEvaporated);
            RLE.Encode(supergridWater, output);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllWaterInfo();

            msg.waterInGroundEvaporated = input.ReadDouble();

            RLE.Decode(input, ref msg.supergridWater);

            message = msg;
            return true;
        }

    }
}
