using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal List<CitySnapshot> cities = new();
        internal HashSet<string> completedDialogs = new();
        internal Dictionary<int2, string> landmarks = new();
        internal bool tutorialSkipped;

        internal override void GetSnapshot()
        {
            level = GGame.level.levelNumber;
            population = GGame.population;

            foreach (var city in GGame.cities)
            {
                var cs = new CitySnapshot();
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
                var cs = new CitySnapshot();
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

        internal class CitySnapshot
        {
            internal int id;
            internal int2 center;
            internal readonly List<int2> hexes = new();
            internal string name;
            internal double population;
            internal float score;
            internal readonly InOutSnapshot inputs = new();
            internal readonly InOutSnapshot outputs = new();

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

                city.population = population;
                city.hexes.AddRange(hexes);
                city.name = name;
                city.score = score;

                inputs.ApplySnapshot(city.GetInOutData(false));
                outputs.ApplySnapshot(city.GetInOutData(true));

                return city;
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

        internal class InOutSnapshot
        {
            internal int needed;
            internal int done;
            internal int framesSinceProcess;
            internal float score;
            internal readonly Queue<bool>[] results = new Queue<bool>[4]; 

            internal void GetSnapshot(CCityInOutData ind)
            {
                needed = ind.nbNeeded;
                done = ind.nbDone;
                framesSinceProcess = ind.nbFramesSinceProcess;
                score = ind.score;
                for (int i = 0; i < 4; i++)
                {
                    var q = new Queue<bool>();
                    results[i] = q;

                    foreach (var e in ind.elementsResults[i])
                    {
                        q.Enqueue(e);
                    }
                }
            }

            internal void ApplySnapshot(CCityInOutData ind)
            {
                ind.nbNeeded = needed;
                ind.nbDone = done;
                ind.nbFramesSinceProcess = framesSinceProcess;
                ind.score = score;


                for (int i = 0; i < 4; i++)
                {
                    var dest = ind.elementsResults[i];
                    dest.Clear();
                    foreach (var e in results[i])
                    {
                        dest.Enqueue(e);
                    }
                }
            }

            internal void Write(BinaryWriter output)
            {
                output.Write(needed);
                output.Write(done);
                output.Write(framesSinceProcess);
                output.Write(score);
                for (int i = 0; i < 4; i++)
                {
                    var q = results[i];
                    output.Write(q.Count);

                    foreach (var e in q)
                    {
                        output.Write(e);
                    }
                }
            }

            internal void Read(BinaryReader input)
            {
                needed = input.ReadInt32();
                done = input.ReadInt32();
                framesSinceProcess = input.ReadInt32();
                score = input.ReadSingle();

                for (int i = 0; i < 4; i++)
                {
                    var q = new Queue<bool>();
                    results[i] = q;

                    int c = input.ReadInt32();
                    for (int j = 0; j < c; j++)
                    {
                        q.Enqueue(input.ReadBoolean());
                    }
                }
            }
        }
        
    }
}
