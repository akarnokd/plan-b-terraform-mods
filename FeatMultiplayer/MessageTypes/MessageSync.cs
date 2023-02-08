using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    /// <summary>
    /// Base class for messages that sync with the game state.
    /// </summary>
    internal abstract class MessageSync : MessageBase
    {
        /// <summary>
        /// Called when the game state should be saved into a message.
        /// </summary>
        internal abstract void GetSnapshot();

        /// <summary>
        /// Called when the game state should be restored from a message.
        /// </summary>
        internal abstract void ApplySnapshot();
    }
}
