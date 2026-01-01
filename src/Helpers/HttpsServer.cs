namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpsServer : IDisposable
    {
        private readonly Int32 _httpPort;
        private readonly Int32 _httpsPort;
        private readonly X509Certificate2 _certificate;
        private readonly Func<String, String, Task<Object>> _requestHandler;

        private TcpListener _httpListener;
        private TcpListener _httpsListener;
        private CancellationTokenSource _cts;
        private Task _httpTask;
        private Task _httpsTask;
        private Boolean _isRunning;

        public Boolean IsRunning => this._isRunning;

        public HttpsServer(Int32 httpPort, Int32 httpsPort, X509Certificate2 certificate, Func<String, String, Task<Object>> requestHandler)
        {
            this._httpPort = httpPort;
            this._httpsPort = httpsPort;
            this._certificate = certificate;
            this._requestHandler = requestHandler;
        }

        public void Start()
        {
            if (this._isRunning)
            {
                return;
            }

            this._cts = new CancellationTokenSource();

            this._httpListener = new TcpListener(IPAddress.Loopback, this._httpPort);
            this._httpListener.Start();
            this._httpTask = Task.Run(() => this.AcceptConnectionsAsync(this._httpListener, false, this._cts.Token));
            PluginLog.Info($"HTTP server started on http://127.0.0.1:{this._httpPort}/");

            if (this._certificate != null)
            {
                this._httpsListener = new TcpListener(IPAddress.Loopback, this._httpsPort);
                this._httpsListener.Start();
                this._httpsTask = Task.Run(() => this.AcceptConnectionsAsync(this._httpsListener, true, this._cts.Token));
                PluginLog.Info($"HTTPS server started on https://local.jmw.nz:{this._httpsPort}/");
            }
            else
            {
                PluginLog.Warning("No SSL certificate available, HTTPS server not started");
            }

            this._isRunning = true;
        }

        public void Stop()
        {
            if (!this._isRunning)
            {
                return;
            }

            this._cts?.Cancel();
            this._httpListener?.Stop();
            this._httpsListener?.Stop();

            try
            {
                Task.WaitAll(new[] { this._httpTask, this._httpsTask }.Where(t => t != null).ToArray(), TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
            }

            this._isRunning = false;
            PluginLog.Info("HTTP/HTTPS servers stopped");
        }

        private async Task AcceptConnectionsAsync(TcpListener listener, Boolean useSsl, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(() => this.HandleClientAsync(client, useSsl, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Error accepting connection");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, Boolean useSsl, CancellationToken cancellationToken)
        {
            Stream stream = null;

            try
            {
                client.ReceiveTimeout = 30000;
                client.SendTimeout = 30000;

                stream = client.GetStream();

                if (useSsl)
                {
                    var sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsServerAsync(
                        this._certificate,
                        clientCertificateRequired: false,
                        enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13,
                        checkCertificateRevocation: false);
                    stream = sslStream;
                }

                await this.ProcessHttpRequestAsync(stream, cancellationToken);
            }
            catch (AuthenticationException ex)
            {
                PluginLog.Warning(ex, "SSL authentication failed");
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
            {
                // Client disconnected
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Error handling client");
            }
            finally
            {
                stream?.Dispose();
                client?.Dispose();
            }
        }

        private async Task ProcessHttpRequestAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new Byte[8192];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            if (bytesRead == 0)
            {
                return;
            }

            var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var lines = requestText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (lines.Length == 0)
            {
                return;
            }

            var requestLine = lines[0].Split(' ');
            if (requestLine.Length < 2)
            {
                return;
            }

            var method = requestLine[0].ToUpperInvariant();
            var path = requestLine[1].ToLowerInvariant();

            PluginLog.Verbose($"HTTP {method} {path}");

            Object responseData;
            var statusCode = 200;
            var statusText = "OK";

            if (method == "OPTIONS")
            {
                await this.SendCorsPreflightResponse(stream);
                return;
            }

            try
            {
                responseData = await this._requestHandler(method, path);

                if (responseData == null)
                {
                    statusCode = 404;
                    statusText = "Not Found";
                    responseData = new { success = false, error = "Not found" };
                }
            }
            catch (Exception ex)
            {
                statusCode = 500;
                statusText = "Internal Server Error";
                responseData = new { success = false, error = ex.Message };
            }

            await this.SendJsonResponse(stream, statusCode, statusText, responseData);
        }

        private async Task SendCorsPreflightResponse(Stream stream)
        {
            var response = new StringBuilder();
            response.AppendLine("HTTP/1.1 200 OK");
            response.AppendLine("Access-Control-Allow-Origin: *");
            response.AppendLine("Access-Control-Allow-Methods: GET, POST, OPTIONS");
            response.AppendLine("Access-Control-Allow-Headers: Content-Type");
            response.AppendLine("Access-Control-Allow-Private-Network: true");
            response.AppendLine("Content-Length: 0");
            response.AppendLine("Connection: close");
            response.AppendLine();

            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        private async Task SendJsonResponse(Stream stream, Int32 statusCode, String statusText, Object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var bodyBytes = Encoding.UTF8.GetBytes(json);

            var response = new StringBuilder();
            response.AppendLine($"HTTP/1.1 {statusCode} {statusText}");
            response.AppendLine("Content-Type: application/json; charset=utf-8");
            response.AppendLine("Access-Control-Allow-Origin: *");
            response.AppendLine("Access-Control-Allow-Private-Network: true");
            response.AppendLine($"Content-Length: {bodyBytes.Length}");
            response.AppendLine("Connection: close");
            response.AppendLine();

            var headerBytes = Encoding.UTF8.GetBytes(response.ToString());
            await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
            await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
