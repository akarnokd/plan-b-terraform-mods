using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FeatMultiplayer.MessageSyncAllItems;

namespace FeatMultiplayer
{
    internal class MessageSyncAllWays : MessageSync
    {
        const string messageCode = "SyncAllWays";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal int lineIdMax;
        internal int vehicleIdMax;
        internal readonly List<SnapshotLine> lines = new();

        internal override void GetSnapshot()
        {
            lineIdMax = CLine.idMax;
            vehicleIdMax = CVehicle.idMax;

            for (int i = 1; i < GWays.lines.Count; i++) 
            { 
                var ls = new SnapshotLine();
                ls.GetSnapshot(GWays.lines[i]);
                lines.Add(ls);
            }
        }

        internal override void ApplySnapshot()
        {
            GWays.lines.Clear();
            GWays.lines.Add(null);
            SMisc.DestroyChildren(CVehicle.vehiclesParentTransform, null);

            var itemDictionary = Plugin.GetItemsDictionary();

            foreach (var line in lines)
            {
                GWays.lines.Add(line.Create(itemDictionary));
            }

            // Update it here because the constructors increment
            CLine.idMax = lineIdMax;
            CVehicle.idMax = vehicleIdMax;
        }

        public override void Encode(BinaryWriter output)
        {
            output.Write(lineIdMax);
            output.Write(vehicleIdMax);
            output.Write(lines.Count);

            foreach (var line in lines)
            {
                line.Encode(output);
            }
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageSyncAllWays();

            msg.Decode(input);

            message = msg;
            return true;
        }

        void Decode(BinaryReader input)
        {
            lineIdMax = input.ReadInt32();
            vehicleIdMax = input.ReadInt32();
            var linesCount = input.ReadInt32();

            for (int i = 0; i < linesCount; i++)
            {
                var ln = new SnapshotLine();
                ln.Decode(input);
                lines.Add(ln);
            }
        }
    }
}
