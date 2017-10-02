using RtmpSharp.IO;
using System;

namespace RtmpSharp.Messaging.Messages
{
    [Serializable]
    [SerializedName("flex.messaging.messages.AcknowledgeMessage")]
    public class AcknowledgeMessage
    {

        [SerializedName("correlationId")]
        public string CorrelationId { get; set; }

        [SerializedName("clientId")]
        public string ClientId { get; set; }

        [SerializedName("destination")]
        public string Destination { get; set; }

        [SerializedName("messageId")]
        public string MessageId { get; set; }

        [SerializedName("timestamp")]
        public DateTime Timestamp { get; set; }

        // TTL (in milliseconds) that message is valid for after `Timestamp`
        [SerializedName("timeToLive")]
        public long TimeToLive { get; set; }

        [SerializedName("body")]
        public object Body { get; set; }

        [SerializedName("headers")]
        public AsObject Headers { get; set; }

        public AcknowledgeMessage()
        {
            Timestamp = DateTime.Now;
        }
    }
}
