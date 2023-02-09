using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatMultiplayer
{
    /// <summary>
    /// Base class for messages having a single coordinate-dependent state.
    /// </summary>
    internal abstract class MessageUpdate : MessageBase
    {
        /// <summary>
        /// The coordinates this message refers to.
        /// </summary>
        internal int2 coords;

        /// <summary>
        /// Take a snapshot at the given coordinates (and save the coordinates too).
        /// </summary>
        /// <param name="coords"></param>
        public abstract void GetSnapshot(int2 coords);

        /// <summary>
        /// Apply the current snapshot state to the saved coordinates.
        /// </summary>
        public abstract void ApplySnapshot();
    }
}
