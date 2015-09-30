using System;
using RtmpSharp.Messaging.Messages;

namespace RtmpSharp.Messaging
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public readonly string ClientId;
        public readonly string Subtopic;
        public readonly AsyncMessageExt Message;

        internal MessageReceivedEventArgs(string clientId, string subtopic, AsyncMessageExt message)
        {
            ClientId = clientId;
            Subtopic = subtopic;
            Message = message;
        }
    }
}
