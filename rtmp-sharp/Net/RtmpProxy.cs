using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using RtmpSharp.Messaging.Messages;

namespace RtmpSharp.Net
{
    public class RtmpProxy
    {
        /*
        public delegate object AsyncMessageHandler(object sender, MessageReceivedEventArgs args);
        public delegate object AcknowledgeMessageHandler(object sender, RemotingMessageReceivedEventArgs args, object response);
        public delegate InvocationException ErrorMessageHandler(object sender, RemotingMessageReceivedEventArgs args, InvocationException response);

        private RtmpServer.ClientMessageHandler _srcMessageHandler;
        private AcknowledgeMessageHandler _ackMessageHandler;
        private AsyncMessageHandler _asyncMessageHandler;
        private ErrorMessageHandler _errorMessageHandler;

        public RtmpServer.ClientMessageHandler SourceMessageHandler { set { _srcMessageHandler = value; } }
        public AsyncMessageHandler RemoteAsyncMessageHandler { set { _asyncMessageHandler = value; } }
        public AcknowledgeMessageHandler RemoteAcknowledgeMessageHandler { set { _ackMessageHandler = value; } }
        public ErrorMessageHandler RemoteErrorMessageHandler { set { _errorMessageHandler = value; } }
         * */
        public delegate void AsyncMessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public delegate void MessageReceivedHandler(object sender, RemotingMessageReceivedEventArgs args);
        public delegate void CommandMessageReceivedHandler(object sender, CommandMessageReceivedEventArgs args);


        public event AsyncMessageReceivedHandler AsyncMessageReceived;
        public event MessageReceivedHandler RemotingMessageReceived;
        public event MessageReceivedHandler AcknowledgeMessageReceived;
        public event MessageReceivedHandler ErrorMessageReceived;
        public event CommandMessageReceivedHandler ClientCommandReceived;

        public event EventHandler<DisconnectedSite> Disconnected;
        public event EventHandler Connected;

        public List<string> SubscribedChannels { set; private get; }

        private RtmpServer _source;
        private RtmpClient _remote;

        public RtmpProxy(IPEndPoint source, Uri remote, SerializationContext context) : this(source, remote, context, null) { }

        public RtmpProxy(IPEndPoint source, Uri remote, SerializationContext context, X509Certificate2 cert)
        {
            SubscribedChannels = new List<string>();
            _source = cert == null ? new RtmpServer(source, context) : new RtmpServer(source, context, cert);
            _source.MaxConnections = 1;
            _remote = new RtmpClient(remote, context, ObjectEncoding.Amf3);

            _source.ClientMessageReceived+=OnSourceMessageReceived;
            _source.ClientCommandReceived+=OnSourceCommandReceived;
            _source.ClientDisconnected += _source_ClientDisconnected;
            _source.ClientConnected += _source_ClientConnected;
            _remote.Disconnected += _remote_Disconnected;
            _remote.MessageReceived += OnRemoteMessageReceived;
        }

        void _source_ClientConnected(object sender, EventArgs e)
        {
            _remote.ConnectAsync().Wait();
            if (Connected != null) Connected(this, e);
        }

        void _source_ClientDisconnected(object sender, EventArgs e)
        {
            _remote.Close();
            _remote = null;
            if (Disconnected != null)
                Disconnected(this, DisconnectedSite.Client);
        }

        void _remote_Disconnected(object sender, EventArgs e)
        {
            _source.Close();
            if (Disconnected != null)
                Disconnected(this, DisconnectedSite.Server);
        }

        internal void OnRemoteMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (AsyncMessageReceived != null)
                AsyncMessageReceived(this, e);
                _source.InvokeReceive(e.ClientId, e.Subtopic, e.Result);
        }


        internal void OnSourceCommandReceived(object sender, CommandMessageReceivedEventArgs e)
        {
            if (ClientCommandReceived != null)
                ClientCommandReceived(this, e);
            if (!_remote.IsDisconnected)
            {
                switch (e.Message.Operation)
                {
                    case CommandOperation.Subscribe:
                        bool success = _remote.SubscribeAsync(e.Endpoint, e.Message.Destination, e.Message.ClientId, e.Message.ClientId).Result;
                        //if (success)
                        {
                            SubscribedChannels.Add(e.Message.ClientId);
                            e.Result= new AcknowledgeMessageExt
                            {
                                Body = null,
                                CorrelationId = e.Message.MessageId,
                                MessageId = Uuid.NewUuid(),
                                ClientId = e.Message.ClientId
                            };
                            return;
                        }
                    case CommandOperation.Login:
                        success = _remote.LoginAsync(e.Message.Body as string).Result;
                        //if (success)
                        {
                            e.Result = new AcknowledgeMessageExt
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
                            return;
                        }
                    default: return;
                }
            }
            else
                Debugger.Break();
        }

        internal void OnSourceMessageReceived(object sender, RemotingMessageReceivedEventArgs e)
        {
            if (RemotingMessageReceived == null)
                RemotingMessageReceived(this, e);
            object ans;
            RemotingMessageReceivedEventArgs args=null;
            try
            {
                ans = _remote.InvokeAsync<object>("my-rtmps", e.Destination, e.Operation, e.Result).Result;
            }
            catch(AggregateException ex)
            {
                if (!(ex.InnerException is InvocationException))
                {
                    //something is not quite right here...
                    Debugger.Break();
                    throw;
                }
                ans = ex.InnerException;

                args = new RemotingMessageReceivedEventArgs(e.Operation, e.Endpoint, e.Destination, e.MessageId, e.Body, e.InvokeId, ans);
                if (ErrorMessageReceived != null)
                    ErrorMessageReceived(this, args);
                e.Result = args.Result;
                return;
            }
            args = new RemotingMessageReceivedEventArgs(e.Operation, e.Endpoint, e.Destination, e.MessageId, e.Body, e.InvokeId, ans);
            if (AcknowledgeMessageReceived != null)
                AcknowledgeMessageReceived(this, args);
            e.Result = args.Result;
        }

        public void SendAsyncMessage(string clientId, string subtopic, object body)
        {
            _source.InvokeReceive(clientId, subtopic, body);
        }

        public void Listen()
        {
            _source.Listen();
        }

        public void Close()
        {
            _source.Close();
            _remote.Close();
        }

        public async Task<object> InvokeAsync(string destination, string operation, params object[] arguments)
        {
            return await _remote.InvokeAsync<object>("my-rtmps", destination, operation, arguments);
        }


    }

    public enum DisconnectedSite
    {
        Client,
        Server,
    }
}
