namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.Json;

    using NetCoreServer;

    public class HttpsServer : IDisposable
    {
        private readonly Int32 _httpsPort;
        private readonly X509Certificate2 _certificate;
        private readonly Func<String, String, System.Threading.Tasks.Task<Object>> _requestHandler;
        private readonly Action<Int32> _runHapticByIndex;
        private readonly IReadOnlyList<String> _waveforms;

        private HapticWssServer _server;
        private Boolean _isRunning;
        private String _bindError;

        public Boolean IsRunning => this._isRunning;
        public String BindError => this._bindError;

        public HttpsServer(
            Int32 httpPort, // Ignored - HTTPS only
            Int32 httpsPort,
            X509Certificate2 certificate,
            Func<String, String, System.Threading.Tasks.Task<Object>> requestHandler,
            Action<Int32> runHapticByIndex,
            IReadOnlyList<String> waveforms)
        {
            this._httpsPort = httpsPort;
            this._certificate = certificate;
            this._requestHandler = requestHandler;
            this._runHapticByIndex = runHapticByIndex;
            this._waveforms = waveforms;
        }

        public void Start()
        {
            if (this._isRunning)
            {
                return;
            }

            if (this._certificate == null)
            {
                this._bindError = "Server can't start without SSL certificate";
                PluginLog.Error(this._bindError);
                return;
            }

            try
            {
                var context = new SslContext(System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13, this._certificate);

                this._server = new HapticWssServer(
                    context,
                    IPAddress.Loopback,
                    this._httpsPort,
                    this._requestHandler,
                    this._runHapticByIndex,
                    this._waveforms);

                if (!this._server.Start())
                {
                    this._bindError = $"Failed to start server on port {this._httpsPort}";
                    PluginLog.Error(this._bindError);
                    return;
                }

                this._isRunning = true;
                PluginLog.Info($"HTTPS/WSS server started on https://local.jmw.nz:{this._httpsPort}/");
            }
            catch (SocketException ex) when (
                ex.SocketErrorCode == SocketError.AddressAlreadyInUse ||
                ex.SocketErrorCode == SocketError.AccessDenied)
            {
                this._bindError = $"Port {this._httpsPort} is already in use. Close any other apps using this port.";
                PluginLog.Error(ex, this._bindError);
            }
            catch (Exception ex)
            {
                this._bindError = $"Failed to start server: {ex.Message}";
                PluginLog.Error(ex, "Server startup error");
            }
        }

        public void Stop()
        {
            if (!this._isRunning)
            {
                return;
            }

            this._server?.Stop();
            this._isRunning = false;
            PluginLog.Info("HTTPS/WSS server stopped");
        }

        public void Dispose()
        {
            this.Stop();
            this._server?.Dispose();
        }
    }

    internal class HapticWssServer : WssServer
    {
        private readonly Func<String, String, System.Threading.Tasks.Task<Object>> _requestHandler;
        private readonly Action<Int32> _runHapticByIndex;
        private readonly IReadOnlyList<String> _waveforms;

        public HapticWssServer(
            SslContext context,
            IPAddress address,
            Int32 port,
            Func<String, String, System.Threading.Tasks.Task<Object>> requestHandler,
            Action<Int32> runHapticByIndex,
            IReadOnlyList<String> waveforms)
            : base(context, address, port)
        {
            this._requestHandler = requestHandler;
            this._runHapticByIndex = runHapticByIndex;
            this._waveforms = waveforms;

            // Disable WebSocket compression to avoid RSV1 bit issues
            this.OptionNoDelay = true;
        }

        protected override SslSession CreateSession()
        {
            return new HapticWssSession(this, this._requestHandler, this._runHapticByIndex, this._waveforms);
        }

        protected override void OnError(SocketError error)
        {
            PluginLog.Error($"WSS server error: {error}");
        }
    }

    internal class HapticWssSession : WssSession
    {
        private readonly Func<String, String, System.Threading.Tasks.Task<Object>> _requestHandler;
        private readonly Action<Int32> _runHapticByIndex;
        private readonly IReadOnlyList<String> _waveforms;

        public HapticWssSession(
            WssServer server,
            Func<String, String, System.Threading.Tasks.Task<Object>> requestHandler,
            Action<Int32> runHapticByIndex,
            IReadOnlyList<String> waveforms)
            : base(server)
        {
            this._requestHandler = requestHandler;
            this._runHapticByIndex = runHapticByIndex;
            this._waveforms = waveforms;
        }

        public override void OnWsConnected(HttpRequest request)
        {
            PluginLog.Info($"WebSocket connected: {Id}");
        }

        public override void OnWsDisconnected()
        {
            PluginLog.Verbose($"WebSocket disconnected: {Id}");
        }

        public override void OnWsReceived(Byte[] buffer, Int64 offset, Int64 size)
        {
            if (size >= 1)
            {
                var index = (Int32)buffer[offset];

                if (index >= 0 && index < this._waveforms.Count)
                {
                    this._runHapticByIndex(index);
                    PluginLog.Verbose($"WebSocket haptic triggered: index={index}, waveform={this._waveforms[index]}");
                }
                else
                {
                    PluginLog.Warning($"WebSocket received invalid haptic index: {index}");
                }
            }
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            var method = request.Method;
            var path = request.Url;

            // Let base class handle WebSocket upgrade requests
            // WebSocket upgrades come as GET with Upgrade: websocket header
            // After upgrade, OnReceivedRequest shouldn't be called for WS frames
            // but if it is, checking for path helps avoid breaking things
            if (method == "GET" && path == "/ws")
            {
                // This was a WebSocket upgrade - base class handles it via OnReceivedRequestHeader
                // Just return and let WssSession handle WebSocket frames
                return;
            }

            PluginLog.Verbose($"HTTPS {method} {path}");

            // Handle CORS preflight
            if (method == "OPTIONS")
            {
                this.SendCorsPreflightResponse();
                return;
            }

            // Handle HTTP requests (not WebSocket)
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var result = await this._requestHandler(method, path);

                    if (result == null)
                    {
                        this.SendJsonResponse(404, new { success = false, error = "Not found" });
                        return;
                    }

                    this.SendJsonResponse(200, result);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Request handler error");
                    this.SendJsonResponse(500, new { success = false, error = ex.Message });
                }
            });
        }

        private void SendCorsPreflightResponse()
        {
            var response = new HttpResponse();
            response.SetBegin(200);
            response.SetHeader("Access-Control-Allow-Origin", "*");
            response.SetHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.SetHeader("Access-Control-Allow-Headers", "Content-Type");
            response.SetHeader("Access-Control-Allow-Private-Network", "true");
            response.SetHeader("Access-Control-Max-Age", "86400");
            response.SetBody();
            this.SendResponseAsync(response);
        }

        private void SendJsonResponse(Int32 statusCode, Object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var body = Encoding.UTF8.GetBytes(json);

            var response = new HttpResponse();
            response.SetBegin(statusCode);
            response.SetHeader("Content-Type", "application/json; charset=utf-8");
            response.SetHeader("Access-Control-Allow-Origin", "*");
            response.SetHeader("Access-Control-Allow-Private-Network", "true");
            response.SetBody(body);
            this.SendResponseAsync(response);
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
