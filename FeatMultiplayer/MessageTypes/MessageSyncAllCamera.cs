﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllCamera : MessageSync
    {
        const string messageCode = "SyncAllCamera";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal angles focusPosition;
        internal int zoomLevelTarget;
        internal float zoomLog;

        internal override void GetSnapshot()
        {
            focusPosition = GCamera.focusPosition;
            zoomLevelTarget = GCamera.zoomLevelTarget;
            zoomLog = GCamera.zoomLog;
        }

        internal override void ApplySnapshot()
        {
            GCamera.focusPosition = focusPosition;
            GCamera.zoomLevelTarget = zoomLevelTarget;
            GCamera.zoomLog = zoomLog;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(focusPosition.alpha);
            output.Write(focusPosition.beta);
            output.Write(focusPosition.radius);
            output.Write(zoomLevelTarget);
            output.Write(zoomLog);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllCamera();

            msg.focusPosition = new angles(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
            msg.zoomLevelTarget = input.ReadInt32();
            msg.zoomLog = input.ReadSingle();

            message = msg;
            return true;
        }
    }
}
