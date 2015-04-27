using RtmpSharp.IO;
using RtmpSharp.Messaging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using RtmpSharp.Messaging.Messages;

namespace RtmpSharp.Net
{
    public class RtmpServer
    {
        public delegate void ClientMessageHandler(object sender, RemotingMessageReceivedEventArgs message);
        public delegate void ClientCommandHandler(object sender, CommandMessageReceivedEventArgs message);

        public event EventHandler<EventArgs> ClientDisconnected;
        public event EventHandler<EventArgs> ClientConnected;
        public event EventHandler<RemotingMessageReceivedEventArgs> ClientMessageReceived;
        public event EventHandler<CommandMessageReceivedEventArgs> ClientCommandReceived;

        private TcpListener _listener;
        private IPEndPoint _serverEndPoint;
        private Uri _serverUri;
        private SerializationContext _context;

        private List<RtmpClient> _clients;

        private readonly RemoteCertificateValidationCallback certificateValidator = (sender, certificate, chain, errors) => true;
        private X509Certificate2 Certificate;
        private bool SSL = false;

        private bool stopped = true;

        public int MaxConnections { get; set; }

        public RtmpServer(IPEndPoint ServerEndPoint, SerializationContext context)
        {
            _serverEndPoint = ServerEndPoint;
            _context = context;

            if (_context == null)
                _context = new SerializationContext();

            _clients = new List<RtmpClient>();

            string ServerUri = string.Format("rtmp://{0}:{1}", _serverEndPoint.Address, _serverEndPoint.Port);
            _serverUri = new Uri(ServerUri);
            MaxConnections = -1;
        }

        public RtmpServer(IPEndPoint ServerEndPoint, SerializationContext context, X509Certificate2 certificate)
            : this(ServerEndPoint, context)
        {
            SSL = true;
            Certificate = certificate;

            string ServerUri = string.Format("rtmps://{0}:{1}", _serverEndPoint.Address, _serverEndPoint.Port);
            _serverUri = new Uri(ServerUri);
        }

        async void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null||stopped)
                return;

            try
            {
                if (ClientConnected != null)
                    ClientConnected(this, new EventArgs());
                TcpClient client = listener.EndAcceptTcpClient(ar);
                if (MaxConnections >= 0 && _clients.Count >= MaxConnections)
                    client.Client.Disconnect(false);
                var stream = GetRtmpStream(client);
                // read c0+c1
                var c01 = await RtmpHandshake.ReadAsync(stream, true);

                var random = new Random();
                var randomBytes = new byte[1528];
                random.NextBytes(randomBytes);

                // write s0+s1+s2
                var s01 = new RtmpHandshake()
                {
                    Version = 3,
                    Time = (uint)Environment.TickCount,
                    Time2 = 0,
                    Random = randomBytes
                };
                var s02 = s01.Clone();
                s02.Time2 = (uint)Environment.TickCount;
                await RtmpHandshake.WriteAsync(stream, s01, s02, true);

                // read c02
                var c02 = await RtmpHandshake.ReadAsync(stream, false);

                RtmpClient rtmpClient = new RtmpClient(_serverUri, _context, stream);
                rtmpClient.ServerMessageReceived += ServerMessageReceived;
                rtmpClient.ServerCommandReceived += ServerCommandReceived;
                rtmpClient.Disconnected += OnClientDisconnected;
                _clients.Add(rtmpClient);
            }
            finally
            {
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }
        }

        void OnClientDisconnected(object sender, EventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(sender, e);
            }

            _clients.Remove(sender as RtmpClient);
        }

        void ServerCommandReceived(object sender, CommandMessageReceivedEventArgs e)
        {
            RtmpClient client = (RtmpClient)sender;
            if (ClientCommandReceived == null)
            {
                switch (e.Message.Operation)
                {
                    case CommandOperation.Login:
                        AcknowledgeMessageExt login = new AcknowledgeMessageExt
                        {
                            Body = "success",
                            CorrelationId = e.Message.MessageId,
                            MessageId = Uuid.NewUuid(),
                            ClientId = Uuid.NewUuid(),
                            Headers = new AsObject
                            {
                                { "DSMessagingVersion", 1.0 },
                                { FlexMessageHeaders.FlexClientId, e.DSId }
                            }
                        };

                        client.InvokeResult(e.InvokeId, login);
                        break;
                    case CommandOperation.Subscribe:
                        AcknowledgeMessageExt subscribe = new AcknowledgeMessageExt
                        {
                            Body = null,
                            CorrelationId = e.Message.MessageId,
                            MessageId = Uuid.NewUuid(),
                            ClientId = e.Message.ClientId
                        };

                        client.InvokeResult(e.InvokeId, subscribe);
                        break;
                    case CommandOperation.Unsubscribe:
                        break;
                    case CommandOperation.ClientPing:
                        break;
                    case CommandOperation.Logout:
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                ClientCommandReceived(this, e);
                client.InvokeResult(e.InvokeId, e.Result);
            }
        }
   


        void ServerMessageReceived(object sender, RemotingMessageReceivedEventArgs e)
        {
            if (ClientMessageReceived!=null)
            {
                RtmpClient client = (RtmpClient)sender;
                ClientMessageReceived(sender, e);

                if ( e.Result is InvocationException)
                {
                    InvocationException ex = e.Result as InvocationException;
                    client.InvokeError(e.InvokeId, e.MessageId, ex.RootCause, ex.FaultDetail, ex.FaultString, ex.FaultCode);
                }
                else
                    client.InvokeResult(e.InvokeId, e.MessageId, e.Result);
            }
        }

        public void Listen()
        {
            if (stopped)
            {
                stopped = false;
                if (_listener == null)
                    _listener = new TcpListener(_serverEndPoint);
                _listener.Start();
                _listener.BeginAcceptTcpClient(OnClientAccepted, _listener);
            }
        }

        public void Close()
        {
            if (!stopped)
            {
                stopped = true;
                _listener.Stop();
                _listener.Server.Disconnect(true);
                foreach (var client in _clients)
                {
                    if (!client.IsDisconnected)
                        client.Close();
                }
                _clients.Clear();
            }
        }

        Stream GetRtmpStream(TcpClient client)
        {
            var stream = client.GetStream();
            if (SSL && Certificate != null)
            {
                var ssl = new SslStream(stream, false, certificateValidator);
                ssl.AuthenticateAsServer(Certificate);
                return ssl;
            }
            else
            {
                return stream;
            }
        }

        internal void InvokeReceive(string clientId, string subtopic, object body)
        {
            foreach(var client in _clients)
            {
                client.InvokeReceive(clientId, subtopic, body);
            }
        }
    }
}
