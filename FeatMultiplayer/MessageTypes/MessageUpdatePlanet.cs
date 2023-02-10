using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageUpdatePlanet : MessageSync
    {
        const string messageCode = "UpdatePlanet";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);
        public override string MessageCode() => messageCode;
        public override byte[] MessageCodeBytes() => messageCodeBytes;

        internal SnapshotPlanet snapshot;

        void Decode(BinaryReader input)
        {
            snapshot.Decode(input);
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageUpdatePlanet();

            msg.Decode(input);

            message = msg;
            return true;
        }

        internal override void GetSnapshot()
        {
            snapshot.GetSnapshot();
        }

        internal override void ApplySnapshot()
        {
            snapshot.ApplySnapshot();
        }

        public override void Encode(BinaryWriter output)
        {
            snapshot.Encode(output);
        }
    }
}
