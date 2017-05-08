using RtmpSharp.IO;
using System;

namespace RtmpSharp.Messaging.Messages
{
    [Serializable]
    [SerializedName("DSA",Canonical =false)]
    [SerializedName("flex.messaging.messages.AsyncMessage")]
    public class AsyncMessage
    {

        [SerializedName("timestamp")]
        public long Timestamp { get; set; }

        [SerializedName("headers")]
        public AsObject Headers { get; set; }

        [SerializedName("body")]
        public object Body { get; set; }

        [SerializedName("correlationId")]
        public string CorrelationId { get; set; }

        [SerializedName("messageId")]
        public string MessageId { get; set; }

        // TTL (in milliseconds) that message is valid for after `Timestamp`
        [SerializedName("timeToLive")]
        public long TimeToLive { get; set; }

        [SerializedName("clientId")]
        public string ClientId { get; set; }

        [SerializedName("destination")]
        public string Destination { get; set; }
    }

    static class AsyncMessageHeaders
    {
        public const string Subtopic = "DSSubtopic";
        public const string Endpoint = "DSEndpoint";
        public const string ID = "DSId";
    }
}
