
namespace RtmpSharp.Messaging.Events
{
    class PeerBandwidth : RtmpEvent
    {
        public enum BandwidthLimitType : byte
        {
            Hard = 0,
            Soft = 1,
            Dynamic = 2
        }

        public int AcknowledgementWindowSize { get; private set; }
        public BandwidthLimitType LimitType { get; private set; }

        private PeerBandwidth() : base(Net.MessageType.SetPeerBandwidth) { }

        public PeerBandwidth(int acknowledgementWindowSize, BandwidthLimitType limitType) : this()
        {
            AcknowledgementWindowSize = acknowledgementWindowSize;
            LimitType = limitType;
        }

        public PeerBandwidth(int acknowledgementWindowSize, byte limitType) : this()
        {
            AcknowledgementWindowSize = acknowledgementWindowSize;
            LimitType = (BandwidthLimitType)limitType;
        }
    }
}
