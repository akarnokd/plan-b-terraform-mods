// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;

namespace FeatMultiplayer
{
    /// <summary>
    /// Run length encoding of data
    /// </summary>
    public class RLE
    {
        static readonly float epsilon = 0.01f;

        public static void Encode(float[] data, BinaryWriter writer)
        {
            writer.Write(data.Length);
            for (int i = 0; i < data.Length; i++) 
            {
                float pivot = data[i];

                if (i < data.Length - 1)
                {

                    float next = data[i + 1];

                    if (Math.Abs(pivot - next) < epsilon)
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            float nextNext = data[i + j + 2];

                            if (Math.Abs(pivot - nextNext) < epsilon)
                            {
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        writer.Write((sbyte)count);
                        writer.Write(pivot);

                        i += count - 1;
                    }
                    else
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            float nextNext = data[i + j + 2];

                            if (Math.Abs(next - nextNext) >= epsilon)
                            {
                                count++;
                                next = nextNext;
                            }
                            else
                            {
                                count--;
                                break;
                            }
                        }
                        writer.Write((sbyte)(-count));
                        for (int j = i; j < i + count; j++)
                        {
                            writer.Write(data[j]);
                        }
                        i += count - 1;
                    }
                } else
                {
                    writer.Write((byte)1);
                    writer.Write(pivot);
                }
            }
        }

        public static void Encode(int[] data, BinaryWriter writer)
        {
            writer.Write(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                int pivot = data[i];

                if (i < data.Length - 1)
                {

                    int next = data[i + 1];

                    if (pivot == next)
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            int nextNext = data[i + j + 2];

                            if (pivot == nextNext)
                            {
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        writer.Write((sbyte)count);
                        writer.Write(pivot);

                        i += count - 1;
                    }
                    else
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            int nextNext = data[i + j + 2];

                            if (next != nextNext)
                            {
                                count++;
                                next = nextNext;
                            }
                            else
                            {
                                count--;
                                break;
                            }
                        }
                        writer.Write((sbyte)(-count));
                        for (int j = i; j < i + count; j++)
                        {
                            writer.Write(data[j]);
                        }
                        i += count - 1;
                    }
                }
                else
                {
                    writer.Write((byte)1);
                    writer.Write(pivot);
                }
            }
        }

        public static void Encode(byte[] data, BinaryWriter writer)
        {
            writer.Write(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                byte pivot = data[i];

                if (i < data.Length - 1)
                {

                    byte next = data[i + 1];

                    if (pivot == next)
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            byte nextNext = data[i + j + 2];

                            if (pivot == nextNext)
                            {
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        writer.Write((sbyte)count);
                        writer.Write(pivot);

                        i += count - 1;
                    }
                    else
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            byte nextNext = data[i + j + 2];

                            if (next != nextNext)
                            {
                                count++;
                                next = nextNext;
                            }
                            else
                            {
                                count--;
                                break;
                            }
                        }
                        writer.Write((sbyte)(-count));
                        for (int j = i; j < i + count; j++)
                        {
                            writer.Write(data[j]);
                        }
                        i += count - 1;
                    }
                }
                else
                {
                    writer.Write((byte)1);
                    writer.Write(pivot);
                }
            }
        }

        public static void Encode(ushort[] data, BinaryWriter writer)
        {
            writer.Write(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                ushort pivot = data[i];

                if (i < data.Length - 1)
                {

                    ushort next = data[i + 1];

                    if (pivot == next)
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            ushort nextNext = data[i + j + 2];

                            if (pivot == nextNext)
                            {
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        writer.Write((sbyte)count);
                        writer.Write(pivot);

                        i += count - 1;
                    }
                    else
                    {
                        int count = 2;
                        for (int j = 0; j < 125 && i + j + 2 < data.Length; j++)
                        {
                            ushort nextNext = data[i + j + 2];

                            if (next != nextNext)
                            {
                                count++;
                                next = nextNext;
                            }
                            else
                            {
                                count--;
                                break;
                            }
                        }
                        writer.Write((sbyte)(-count));
                        for (int j = i; j < i + count; j++)
                        {
                            writer.Write(data[j]);
                        }
                        i += count - 1;
                    }
                }
                else
                {
                    writer.Write((byte)1);
                    writer.Write(pivot);
                }
            }
        }

        public static void Decode(BinaryReader reader, ref float[] data)
        {
            data = new float[reader.ReadInt32()];

            int offset = 0;
            while (offset < data.Length)
            {
                int code = reader.ReadSByte();
                // Plugin.LogDebug("Code: " + code + ", Offset = " + offset + ", Len = " + data.Length);
                if (code < 0)
                {
                    for (int i = code; i < 0; i++)
                    {
                        data[offset++] = reader.ReadSingle();
                    }
                }
                else
                {
                    var val = reader.ReadSingle();
                    for (int i = 0; i < code; i++)
                    {
                        data[offset++] = val;
                    }
                }
            }
        }

        public static void Decode(BinaryReader reader, ref int[] data)
        {
            data = new int[reader.ReadInt32()];
            int offset = 0;
            while (offset < data.Length)
            {
                int code = reader.ReadSByte();
                if (code < 0)
                {
                    for (int i = code; i < 0; i++)
                    {
                        data[offset++] = reader.ReadInt32();
                    }
                }
                else
                {
                    var val = reader.ReadInt32();
                    for (int i = 0; i < code; i++)
                    {
                        data[offset++] = val;
                    }
                }
            }
        }

        public static void Decode(BinaryReader reader, ref byte[] data)
        {
            data = new byte[reader.ReadInt32()];
            int offset = 0;
            while (offset < data.Length)
            {
                int code = reader.ReadSByte();
                if (code < 0)
                {
                    for (int i = code; i < 0; i++)
                    {
                        data[offset++] = reader.ReadByte();
                    }
                }
                else
                {
                    var val = reader.ReadByte();
                    for (int i = 0; i < code; i++)
                    {
                        data[offset++] = val;
                    }
                }
            }
        }

        public static void Decode(BinaryReader reader, ref ushort[] data)
        {
            data = new ushort[reader.ReadInt32()];
            int offset = 0;
            while (offset < data.Length)
            {
                int code = reader.ReadSByte();
                if (code < 0)
                {
                    for (int i = code; i < 0; i++)
                    {
                        data[offset++] = reader.ReadUInt16();
                    }
                }
                else
                {
                    var val = reader.ReadUInt16();
                    for (int i = 0; i < code; i++)
                    {
                        data[offset++] = val;
                    }
                }
            }
        }
    }
}
