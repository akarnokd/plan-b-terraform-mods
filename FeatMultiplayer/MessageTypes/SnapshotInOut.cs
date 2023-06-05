// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.IO;

namespace FeatMultiplayer
{
    internal class SnapshotInOut
    {
        internal int needed;
        internal int done;
        internal int framesSinceProcess;
        internal float score;
        internal readonly Queue<bool>[] results = new Queue<bool>[4];

        internal bool HaveChanged(SnapshotInOut other)
        {
            return this.needed != other.needed
                || this.done != other.done
                || this.framesSinceProcess != other.framesSinceProcess
                || this.score != other.score
                || HaveQueuesChanged(other.results)
                ;
        }

        internal bool HaveQueuesChanged(Queue<bool>[] other)
        {
            if (this.results.Length != other.Length)
            {
                return true;
            }

            for (int i = 0; i < this.results.Length; i++)
            {
                var q1 = this.results[i];
                var q2 = other[i];

                if (HaveQueuesChanged(q1, q2))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool HaveQueuesChanged(Queue<bool> q1, Queue<bool> q2)
        {
            if (q1.Count != q2.Count)
            {
                return true;
            }

            using var en1 = q1.GetEnumerator();
            using var en2 = q2.GetEnumerator();

            while (en1.MoveNext() && en2.MoveNext())
            {
                if (en1.Current != en2.Current)
                {
                    return true;
                }
            }

            return false;
        }

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
