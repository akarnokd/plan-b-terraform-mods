using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    internal abstract class MessageSync : MessageBase
    {
        internal abstract void GetSnapshot();

        internal abstract void ApplySnapshot();
    }
}
