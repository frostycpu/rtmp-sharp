using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Messaging.Messages;

namespace RtmpSharp.Messaging
{
    public class ConnectMessageEventArgs : CommandMessageReceivedEventArgs
    {
        public readonly string ClientId;
        public readonly string AuthToken;
        public readonly AsObject ConnectionParameters;
        internal ConnectMessageEventArgs(string clientId, string authToken, CommandMessage message, string endpoint, string dsId, int invokeId, AsObject cParameters) : base(message, endpoint, dsId, invokeId)
        {
            ClientId = clientId;
            AuthToken = authToken;
            ConnectionParameters = cParameters;
        }
    }
}
