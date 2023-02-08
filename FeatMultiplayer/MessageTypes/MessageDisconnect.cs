using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    /// <summary>
    /// Message indicating the sender loop should disconnect.
    /// </summary>
    internal class MessageDisconnect : MessageBase
    {
        /// <summary>
        /// Singleton instance as this is a marker message.
        /// </summary>
        internal static readonly MessageDisconnect Instance = new MessageDisconnect();

        const string messageCode = "Disconnect";
        static readonly byte[] bytes = Encoding.UTF8.GetBytes(messageCode);

        public override void Encode(BinaryWriter output)
        {
            throw new InvalidOperationException();
        }

        public override string MessageCode()
        {
            return messageCode;
        }

        public override byte[] MessageCodeBytes()
        {
            return bytes;
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            throw new InvalidOperationException();
        }
    }
}
