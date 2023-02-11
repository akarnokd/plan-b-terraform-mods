// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

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
