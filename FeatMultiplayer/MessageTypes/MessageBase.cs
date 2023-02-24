// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using UnityEngine;

namespace FeatMultiplayer
{
    /// <summary>
    /// Base class for encoding and decoding message types.
    /// </summary>
    public abstract class MessageBase
    {
        /// <summary>
        /// Set for messages received from clients.
        /// </summary>
        public ClientSession sender;

        /// <summary>
        /// Set in the message dispatching init to call a method when this message was received
        /// </summary>
        public Action<MessageBase> onReceive;

        /// <summary>
        /// Specify this code to be used when decoding and dispatching binary messages.
        /// </summary>
        /// <returns></returns>
        public abstract string MessageCode();

        /// <summary>
        /// The UTF8 encoded MessageCode string (cache it in the implementor class).
        /// </summary>
        /// <returns></returns>
        public abstract byte[] MessageCodeBytes();

        /// <summary>
        /// Implement this to try and decode an input stream into the specified message type; called on the network thread.
        /// </summary>
        /// <param name="input">Contains only the message-specific bytes (excludes headers)</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool TryDecode(BinaryReader input, out MessageBase message);

        /// <summary>
        /// Implement this to encode this message into a stream; called on the network thread.
        /// </summary>
        /// <param name="output">To contain only the message-specific bytes (don't write any headers for the message type itself!)</param>
        public abstract void Encode(BinaryWriter output);

        /// <summary>
        /// Convenience to log an error report.
        /// </summary>
        /// <param name="message"></param>
        protected void LogError(object message)
        {
            Plugin.LogError(message);
        }
        /// <summary>
        /// Convenience to log an info report.
        /// </summary>
        /// <param name="message"></param>
        protected void LogInfo(object message)
        {
            Plugin.LogInfo(message);
        }
    }

    /// <summary>
    /// Extension methods for reading and writing game/engine types.
    /// </summary>
    public static class BinaryReadWriteEx 
    {
        public static void Write(this BinaryWriter writer, in Vector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );
        }

        public static void Write(this BinaryWriter writer, in int2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }

        public static void WriteShort(this BinaryWriter writer, in int2 vector)
        {
            writer.Write((ushort)vector.x);
            writer.Write((ushort)vector.y);
        }

        public static int2 ReadInt2(this BinaryReader reader)
        {
            return new int2(
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }
        public static int2 ReadInt2Short(this BinaryReader reader)
        {
            return new int2(
                reader.ReadUInt16(),
                reader.ReadUInt16()
            );
        }

        public static void Write(this BinaryWriter writer, in Quaternion q)
        {
            writer.Write(q.x);
            writer.Write(q.y);
            writer.Write(q.z);
            writer.Write(q.w);
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );
        }

        public static void Write(this BinaryWriter writer, in CTransform q)
        {
            writer.Write(q.pos);
            writer.Write(q.rot);
        }

        public static CTransform ReadCTransform(this BinaryReader reader)
        {
            return new CTransform(
                ReadVector3(reader),
                ReadQuaternion(reader)
            );
        }
    }
}
