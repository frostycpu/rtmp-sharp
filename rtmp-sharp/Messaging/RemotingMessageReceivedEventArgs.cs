using RtmpSharp.IO;
using RtmpSharp.Messaging.Messages;
using System;

namespace RtmpSharp.Messaging
{
    public class RemotingMessageReceivedEventArgs : EventArgs
    {
        public readonly string Operation;
        public readonly string Destination;
        public readonly string Endpoint;
        public readonly string MessageId;
        public readonly int InvokeId;
        public readonly RemotingMessage Message;
        public AcknowledgeMessage Result;
        public ErrorMessage Error;

        internal RemotingMessageReceivedEventArgs(RemotingMessage message , string endpoint, string clientId, int invokeId)
        {
            Message = message;
            Operation = message.Operation;
            Destination = message.Destination;
            Endpoint = endpoint;
            MessageId = clientId;
            InvokeId = invokeId;
        }
    }
}
