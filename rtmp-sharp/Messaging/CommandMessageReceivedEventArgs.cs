using RtmpSharp.Messaging.Messages;
using System;

namespace RtmpSharp.Messaging
{
    public class CommandMessageReceivedEventArgs : EventArgs
    {
        public readonly CommandMessage Message;
        public readonly string DSId;
        public readonly CommandOperation Operation;
        public readonly string Endpoint;
        public readonly int InvokeId;
        public AcknowledgeMessage Result;

        internal CommandMessageReceivedEventArgs(CommandMessage message, string endpoint, string dsId, int invokeId)
        {
            DSId = dsId;
            Operation = message?.Operation??CommandOperation.ClientPing;
            Endpoint = endpoint;
            Message = message;
            InvokeId = invokeId;
        }
    }
}
