using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal class MessageLogin : MessageBase
    {
        const string messageCode = "Login";
        static readonly byte[] messageCodeBytes = Encoding.UTF8.GetBytes(messageCode);

        internal string userName;

        internal string password;

        public override void Encode(BinaryWriter output)
        {
            output.Write(userName);
            output.Write(password);
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
            var msg = new MessageLogin();
            msg.userName = input.ReadString();
            msg.password = input.ReadString();
            message = msg;
            return true;
        }
    }
}
