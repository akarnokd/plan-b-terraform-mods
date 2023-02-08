using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageLoginResponse : MessageBase
    {
        const string messageCode = "LoginResponse";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal string reason;

        public override void Encode(BinaryWriter output)
        {
            output.Write(reason);
        }

        public override string MessageCode()
        {
            return messageCode;
        }

        public override byte[] MessageCodeBytes()
        {
            return messageCodeBytes;
        }

        public override bool TryDecode(BinaryReader input, out MessageBase message)
        {
            var msg = new MessageLoginResponse();
            msg.reason = input.ReadString();
            message = msg;
            return true;
        }
    }
}
