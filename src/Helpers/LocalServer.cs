namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;

    using NetCoreServer;

    /// <summary>
    /// Runs the loopback listeners:
    ///  - HTTP/WS on 127.0.0.1 (primary; loopback is a secure context, so HTTPS pages can use it)
    ///  - HTTPS/WSS on local.jmw.nz (deprecated fallback for Safari, which still blocks
    ///    ws:// to loopback from HTTPS pages)
    /// </summary>
    internal sealed class LocalServer : IDisposable
    {
        private readonly HapticEndpoint _endpoint;

        private HapticWsServer _httpServer;
        private HapticWssServer _httpsServer;

        public String HttpError { get; private set; }
        public String HttpsError { get; private set; }

        public LocalServer(HapticEndpoint endpoint)
        {
            this._endpoint = endpoint;
        }

        public void StartHttp(Int32 port)
        {
            var server = new HapticWsServer(IPAddress.Loopback, port, this._endpoint);
            this.HttpError = TryStart(server.Start, port, $"HTTP/WS server started on http://127.0.0.1:{port}/");
            if (this.HttpError == null)
            {
                this._httpServer = server;
            }
            else
            {
                server.Dispose();
            }
        }

        public void StartHttps(Int32 port, X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                this.HttpsError = "Server can't start without SSL certificate";
                PluginLog.Error(this.HttpsError);
                return;
            }

            var context = new SslContext(System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13, certificate);
            var server = new HapticWssServer(context, IPAddress.Loopback, port, this._endpoint);
            this.HttpsError = TryStart(server.Start, port, $"HTTPS/WSS server started on https://local.jmw.nz:{port}/");
            if (this.HttpsError == null)
            {
                this._httpsServer = server;
            }
            else
            {
                server.Dispose();
            }
        }

        private static String TryStart(Func<Boolean> start, Int32 port, String startedMessage)
        {
            try
            {
                if (!start())
                {
                    var error = $"Failed to start server on port {port}";
                    PluginLog.Error(error);
                    return error;
                }

                PluginLog.Info(startedMessage);
                return null;
            }
            catch (SocketException ex) when (
                ex.SocketErrorCode == SocketError.AddressAlreadyInUse ||
                ex.SocketErrorCode == SocketError.AccessDenied)
            {
                var error = $"Port {port} is already in use. Close any other apps using this port.";
                PluginLog.Error(ex, error);
                return error;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Server startup error");
                return $"Failed to start server: {ex.Message}";
            }
        }

        public void Dispose()
        {
            this._httpServer?.Stop();
            this._httpServer?.Dispose();
            this._httpServer = null;

            this._httpsServer?.Stop();
            this._httpsServer?.Dispose();
            this._httpsServer = null;
        }
    }

    internal sealed class HapticWsServer : WsServer
    {
        private readonly HapticEndpoint _endpoint;

        public HapticWsServer(IPAddress address, Int32 port, HapticEndpoint endpoint)
            : base(address, port)
        {
            this._endpoint = endpoint;
            this.OptionNoDelay = true;
        }

        protected override TcpSession CreateSession() => new HapticWsSession(this, this._endpoint);

        protected override void OnError(SocketError error) => PluginLog.Error($"WS server error: {error}");
    }

    internal sealed class HapticWssServer : WssServer
    {
        private readonly HapticEndpoint _endpoint;

        public HapticWssServer(SslContext context, IPAddress address, Int32 port, HapticEndpoint endpoint)
            : base(context, address, port)
        {
            this._endpoint = endpoint;
            this.OptionNoDelay = true;
        }

        protected override SslSession CreateSession() => new HapticWssSession(this, this._endpoint);

        protected override void OnError(SocketError error) => PluginLog.Error($"WSS server error: {error}");
    }

    // The two session classes are identical apart from their base class (NetCoreServer
    // has separate plain and TLS hierarchies); all logic lives in HapticEndpoint/ReceiveGuard.

    internal sealed class HapticWsSession : WsSession
    {
        private readonly HapticEndpoint _endpoint;
        private readonly ReceiveGuard _guard = new ReceiveGuard();

        public HapticWsSession(WsServer server, HapticEndpoint endpoint)
            : base(server)
        {
            this._endpoint = endpoint;
        }

        protected override void OnReceived(Byte[] buffer, Int64 offset, Int64 size)
        {
            if (!this._guard.Filter(ref buffer, ref offset, ref size))
            {
                PluginLog.Warning($"Dropping connection {this.Id}: request or message too large");
                this.Disconnect();
                return;
            }

            if (size > 0)
            {
                base.OnReceived(buffer, offset, size);
            }
        }

        public override Boolean OnWsConnecting(HttpRequest request, HttpResponse response) => HapticEndpoint.IsWebSocketPath(request);

        public override void OnWsConnected(HttpRequest request)
        {
            this._guard.EnterWebSocketMode();
            PluginLog.Info($"WebSocket connected: {this.Id}");
        }

        public override void OnWsDisconnected() => PluginLog.Verbose($"WebSocket disconnected: {this.Id}");

        public override void OnWsReceived(Byte[] buffer, Int64 offset, Int64 size) => this._endpoint.HandleWebSocketMessage(buffer, offset, size);

        protected override void OnReceivedRequest(HttpRequest request)
        {
            if (this._guard.IsWebSocket)
            {
                // The upgrade request itself. The base class would feed any request body to the
                // frame parser without passing through ReceiveGuard; browsers never send one.
                return;
            }

            this._guard.HttpRequestCompleted();
            this._endpoint.HandleHttpRequest(request, this.SendResponseAsync);
        }

        protected override void OnError(SocketError error)
        {
            if (error != SocketError.ConnectionReset)
            {
                PluginLog.Warning($"WS session error: {error}");
            }
        }
    }

    internal sealed class HapticWssSession : WssSession
    {
        private readonly HapticEndpoint _endpoint;
        private readonly ReceiveGuard _guard = new ReceiveGuard();

        public HapticWssSession(WssServer server, HapticEndpoint endpoint)
            : base(server)
        {
            this._endpoint = endpoint;
        }

        protected override void OnReceived(Byte[] buffer, Int64 offset, Int64 size)
        {
            if (!this._guard.Filter(ref buffer, ref offset, ref size))
            {
                PluginLog.Warning($"Dropping connection {this.Id}: request or message too large");
                this.Disconnect();
                return;
            }

            if (size > 0)
            {
                base.OnReceived(buffer, offset, size);
            }
        }

        public override Boolean OnWsConnecting(HttpRequest request, HttpResponse response) => HapticEndpoint.IsWebSocketPath(request);

        public override void OnWsConnected(HttpRequest request)
        {
            this._guard.EnterWebSocketMode();
            PluginLog.Info($"WebSocket connected: {this.Id}");
        }

        public override void OnWsDisconnected() => PluginLog.Verbose($"WebSocket disconnected: {this.Id}");

        public override void OnWsReceived(Byte[] buffer, Int64 offset, Int64 size) => this._endpoint.HandleWebSocketMessage(buffer, offset, size);

        protected override void OnReceivedRequest(HttpRequest request)
        {
            if (this._guard.IsWebSocket)
            {
                // The upgrade request itself. The base class would feed any request body to the
                // frame parser without passing through ReceiveGuard; browsers never send one.
                return;
            }

            this._guard.HttpRequestCompleted();
            this._endpoint.HandleHttpRequest(request, this.SendResponseAsync);
        }

        protected override void OnError(SocketError error)
        {
            if (error != SocketError.ConnectionReset)
            {
                PluginLog.Warning($"WSS session error: {error}");
            }
        }
    }
}
