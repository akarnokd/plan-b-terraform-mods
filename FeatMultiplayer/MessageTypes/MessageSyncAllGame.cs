// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using MonoMod.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FeatMultiplayer
{
    internal class MessageSyncAllGame : MessageSync
    {
        const string messageCode = "SyncAllGame";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int level;
        internal long population;
        internal List<SnapshotCity> cities = new();
        internal HashSet<string> completedDialogs = new();
        internal Dictionary<int2, string> landmarks = new();
        internal bool tutorialSkipped;

        internal override void GetSnapshot()
        {
            level = GGame.level.levelNumber;
            population = GGame.population;

            foreach (var city in GGame.cities)
            {
                var cs = new SnapshotCity();
                cs.GetSnapshot(city);

                cities.Add(cs);
            }
            foreach (var dialog in GGame.dialogs)
            {
                if (dialog.state == CDialog.State.Complete)
                {
                    completedDialogs.Add(dialog.codeName);
                }
            }

            landmarks.AddRange(GGame.dicoLandmarks);
            tutorialSkipped = GGame.isTutoSkipped;
        }

        internal override void ApplySnapshot()
        {
            GGame.level = GGame.levels.Find(v => v.levelNumber == level);
            GGame.population = population;

            GGame.cities.Clear();
            foreach (var cs in cities)
            {
                GGame.cities.Add(cs.Create());
            }

            foreach (var dialog in GGame.dialogs)
            {
                if (completedDialogs.Contains(dialog.codeName))
                {
                    dialog.state = CDialog.State.Complete;
                }
            }

            GGame.dicoLandmarks.Clear();
            GGame.dicoLandmarks.AddRange(landmarks);
            GGame.isTutoSkipped = tutorialSkipped;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(level);
            output.Write(population);
            output.Write(cities.Count);
            foreach (var c in cities)
            {
                c.Write(output);
            }
            output.Write(completedDialogs.Count);
            foreach (var dialog in completedDialogs)
            {
                output.Write(dialog);
            }
            output.Write(landmarks.Count);
            foreach (var landmark in landmarks)
            {
                var xy = landmark.Key;
                output.Write(xy.x);
                output.Write(xy.y);
                output.Write(landmark.Value);
            }

            output.Write(tutorialSkipped);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllGame();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            level = input.ReadInt32();
            population = input.ReadInt64();
            int cc = input.ReadInt32();
            for (int i = 0; i < cc; i++)
            {
                var cs = new SnapshotCity();
                cs.Read(input);
                cities.Add(cs);
            }

            cc = input.ReadInt32();
            for (int i = 0; i < cc; i++)
            {
                completedDialogs.Add(input.ReadString());
            }

            cc = input.ReadInt32();
            for (int i = 0; i < cc; i++)
            {
                int lx = input.ReadInt32();
                int ly = input.ReadInt32();

                landmarks[new int2(lx, ly)] = input.ReadString();
            }

            tutorialSkipped = input.ReadBoolean();
        }
       
    }
}
