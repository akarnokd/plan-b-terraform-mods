// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotCity
    {
        internal int id;
        internal int2 center;
        internal readonly List<int2> hexes = new();
        internal string name;
        internal double population;
        internal float score;
        internal readonly SnapshotInOut inputs = new();
        internal readonly SnapshotInOut outputs = new();

        internal void GetSnapshot(CCity city)
        {
            id = city.cityId;
            center = city.center;
            population = city.population;
            hexes.AddRange(city.hexes);
            name = city.name;
            score = city.score;

            inputs.GetSnapshot(city.GetInOutData(false));
            outputs.GetSnapshot(city.GetInOutData(true));
        }

        internal CCity Create()
        {
            var city = new CCity(id, center);
            ApplySnapshot(city);

            return city;
        }

        internal void ApplySnapshot(CCity city)
        {
            city.center = center;

            city.population = population;
            city.name = name;
            city.score = score;

            city.hexes.Clear();
            city.hexes.AddRange(hexes);

            inputs.ApplySnapshot(city.GetInOutData(false));
            outputs.ApplySnapshot(city.GetInOutData(true));
        }

        internal void Write(BinaryWriter output)
        {
            output.Write(id);
            output.Write(center.x);
            output.Write(center.y);
            output.Write(population);
            output.Write(name);
            output.Write(score);

            output.Write(hexes.Count);
            foreach (var hx in hexes)
            {
                output.Write(hx.x);
                output.Write(hx.y);
            }
            inputs.Write(output);
            outputs.Write(output);
        }

        internal void Read(BinaryReader input)
        {
            id = input.ReadInt32();
            int cx = input.ReadInt32();
            int cy = input.ReadInt32();
            center = new int2(cx, cy);
            population = input.ReadDouble();
            name = input.ReadString();
            score = input.ReadSingle();

            int hc = input.ReadInt32();
            for (int i = 0; i < hc; i++)
            {
                int hx = input.ReadInt32();
                int hy = input.ReadInt32();
                hexes.Add(new int2(hx, hy));
            }
            inputs.Read(input);
            outputs.Read(input);
        }
    }
}
