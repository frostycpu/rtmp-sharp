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
        public readonly object Body;
        public readonly int InvokeId;
        public readonly object Response;
        public object Result;

        internal RemotingMessageReceivedEventArgs(string operation, string endpoint, string destination, string messageId, object body, int invokeId)
        {
            Operation = operation;
            Destination = destination;
            Endpoint = endpoint;
            MessageId = messageId;
            Body = body;
            InvokeId = invokeId;
            Result = body;
        }
        internal RemotingMessageReceivedEventArgs(string operation, string endpoint, string destination, string messageId, object body, int invokeId, object response):this(operation,endpoint,destination,messageId,body,invokeId)
        {
            Response = response;
            Result = response;
        }
    }
}
