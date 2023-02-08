using System.IO;

namespace FeatMultiplayer
{
    /// <summary>
    /// Base class for encoding and decoding message types.
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// Set for messages received from clients.
        /// </summary>
        public ClientSession sender;

        /// <summary>
        /// Specify this code to be used when decoding and dispatching binary messages.
        /// </summary>
        /// <returns></returns>
        public abstract string MessageCode();

        /// <summary>
        /// The UTF8 encoded MessageCode string (cache it in the implementor class).
        /// </summary>
        /// <returns></returns>
        public abstract byte[] MessageCodeBytes();

        /// <summary>
        /// Implement this to try and decode an input stream into the specified message type; called on the network thread.
        /// </summary>
        /// <param name="input">Contains only the message-specific bytes (excludes headers)</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool TryDecode(BinaryReader input, out BaseMessage message);

        /// <summary>
        /// Implement this to encode this message into a stream; called on the network thread.
        /// </summary>
        /// <param name="output">To contain only the message-specific bytes (don't write any headers for the message type itself!)</param>
        public abstract void Encode(BinaryWriter output);

        /// <summary>
        /// Convenience to log an error report.
        /// </summary>
        /// <param name="message"></param>
        protected void LogError(object message)
        {
            Plugin.LogError(message);
        }
        /// <summary>
        /// Convenience to log an info report.
        /// </summary>
        /// <param name="message"></param>
        protected void LogInfo(object message)
        {
            Plugin.LogInfo(message);
        }
    }
}
