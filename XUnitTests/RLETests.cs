using FeatMultiplayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace XUnitTests
{
    [TestClass]
    public class RLETests
    {
        [TestMethod]
        public void BasicFloat()
        {
            float[] input = { 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 1, 0, 2 };

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(3, br.ReadByte());
            Assert.AreEqual(0f, br.ReadSingle());

            Assert.AreEqual(-5, br.ReadSByte());
            Assert.AreEqual(1f, br.ReadSingle());
            Assert.AreEqual(2f, br.ReadSingle());
            Assert.AreEqual(3f, br.ReadSingle());
            Assert.AreEqual(4f, br.ReadSingle());
            Assert.AreEqual(5f, br.ReadSingle());

            Assert.AreEqual(5, br.ReadByte());
            Assert.AreEqual(0f, br.ReadSingle());

            Assert.AreEqual(-3, br.ReadSByte());
            Assert.AreEqual(1f, br.ReadSingle());
            Assert.AreEqual(0f, br.ReadSingle());
            Assert.AreEqual(2f, br.ReadSingle());

            float[] output = new float[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void Shuffled()
        {
            float[] input = { 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 1, 0, 2 };

            for (int i = 0; i < 10000; i++)
            {

                Shuffle(input);

                var mw = new MemoryStream();
                var bw = new BinaryWriter(mw);

                RLE.Encode(input, bw);

                mw.Position = 0;

                var br = new BinaryReader(mw);

                float[] output = new float[input.Length];

                mw.Position = 0;

                RLE.Decode(br, ref output);

                CollectionAssert.AreEqual(input, output);
            }
        }

        [TestMethod]
        public void BasicInt()
        {
            int[] input = { 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 1, 0, 2 };

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(3, br.ReadByte());
            Assert.AreEqual(0f, br.ReadInt32());

            Assert.AreEqual(-5, br.ReadSByte());
            Assert.AreEqual(1, br.ReadInt32());
            Assert.AreEqual(2, br.ReadInt32());
            Assert.AreEqual(3, br.ReadInt32());
            Assert.AreEqual(4, br.ReadInt32());
            Assert.AreEqual(5, br.ReadInt32());

            Assert.AreEqual(5, br.ReadByte());
            Assert.AreEqual(0, br.ReadInt32());

            Assert.AreEqual(-3, br.ReadSByte());
            Assert.AreEqual(1, br.ReadInt32());
            Assert.AreEqual(0, br.ReadInt32());
            Assert.AreEqual(2, br.ReadInt32());

            int[] output = new int[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void Basic2()
        {
            float[] input = new float[256];

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(127, br.ReadByte());
            Assert.AreEqual(0f, br.ReadSingle());

            Assert.AreEqual(127, br.ReadByte());
            Assert.AreEqual(0f, br.ReadSingle());

            Assert.AreEqual(2, br.ReadByte());
            Assert.AreEqual(0f, br.ReadSingle());

            float[] output = new float[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void BasicInt2()
        {
            int[] input = new int[256];

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(127, br.ReadByte());
            Assert.AreEqual(0, br.ReadInt32());

            Assert.AreEqual(127, br.ReadByte());
            Assert.AreEqual(0, br.ReadInt32());

            Assert.AreEqual(2, br.ReadByte());
            Assert.AreEqual(0, br.ReadInt32());

            int[] output = new int[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void BasicInt3()
        {
            int[] input = new int[256];
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(-127, br.ReadSByte());
            for (int j = 0; j < 127; j++)
            {
                Assert.AreEqual(j, br.ReadInt32());
            }

            Assert.AreEqual(-127, br.ReadSByte());
            for (int j = 127; j < 254; j++)
            {
                Assert.AreEqual(j, br.ReadInt32());
            }

            Assert.AreEqual(-2, br.ReadSByte());
            Assert.AreEqual(254, br.ReadInt32());
            Assert.AreEqual(255, br.ReadInt32());

            int[] output = new int[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void Basic3()
        {
            float[] input = new float[256];
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = i;
            }

            var mw = new MemoryStream();
            var bw = new BinaryWriter(mw);

            RLE.Encode(input, bw);

            mw.Position = 0;
            var br = new BinaryReader(mw);

            Assert.AreEqual(-127, br.ReadSByte());
            for (int j = 0; j < 127; j++)
            {
                Assert.AreEqual(j, br.ReadSingle());
            }

            Assert.AreEqual(-127, br.ReadSByte());
            for (int j = 127; j < 254; j++)
            {
                Assert.AreEqual(j, br.ReadSingle());
            }

            Assert.AreEqual(-2, br.ReadSByte());
            Assert.AreEqual(254, br.ReadSingle());
            Assert.AreEqual(255, br.ReadSingle());

            float[] output = new float[input.Length];

            mw.Position = 0;


            RLE.Decode(br, ref output);

            CollectionAssert.AreEqual(input, output);
        }

        [TestMethod]
        public void ShuffledInt()
        {
            int[] input = { 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 1, 0, 2 };

            for (int i = 0; i < 10000; i++)
            {

                Shuffle(input);

                var mw = new MemoryStream();
                var bw = new BinaryWriter(mw);

                RLE.Encode(input, bw);

                mw.Position = 0;

                var br = new BinaryReader(mw);

                int[] output = new int[input.Length];

                mw.Position = 0;

                RLE.Decode(br, ref output);

                CollectionAssert.AreEqual(input, output);
            }
        }

        public static void Shuffle<T>(T[] list)
        {
            var rng = new Random();
            for (int i = 0; i < list.Length; i++)
            {
                T value = list[i];
                int index = i + rng.Next(list.Length - i);
                list[i] = list[index];
                list[index] = value;
            }
        }
    }
}
