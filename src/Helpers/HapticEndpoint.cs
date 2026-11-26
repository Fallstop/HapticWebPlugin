namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    using NetCoreServer;

    /// <summary>
    /// Request handling shared by the HTTP (ws://) and HTTPS (wss://) listeners.
    /// </summary>
    internal sealed class HapticEndpoint
    {
        private readonly Func<String, String, System.Threading.Tasks.Task<Object>> _requestHandler;
        private readonly Action<Int32> _runHapticByIndex;
        private readonly IReadOnlyList<String> _waveforms;

        public HapticEndpoint(
            Func<String, String, System.Threading.Tasks.Task<Object>> requestHandler,
            Action<Int32> runHapticByIndex,
            IReadOnlyList<String> waveforms)
        {
            this._requestHandler = requestHandler;
            this._runHapticByIndex = runHapticByIndex;
            this._waveforms = waveforms;
        }

        // Only /ws upgrades; anything else is answered as a normal HTTP request.
        public static Boolean IsWebSocketPath(HttpRequest request) => request.Url == "/ws";

        public void HandleWebSocketMessage(Byte[] buffer, Int64 offset, Int64 size)
        {
            if (size < 1)
            {
                return;
            }

            var index = (Int32)buffer[offset];

            if (index < this._waveforms.Count)
            {
                this._runHapticByIndex(index);
                PluginLog.Verbose($"WebSocket haptic triggered: index={index}, waveform={this._waveforms[index]}");
            }
            else
            {
                PluginLog.Warning($"WebSocket received invalid haptic index: {index}");
            }
        }

        public void HandleHttpRequest(HttpRequest request, Func<HttpResponse, Boolean> send)
        {
            var method = request.Method;
            var path = request.Url;

            PluginLog.Verbose($"HTTP {method} {path}");

            if (method == "OPTIONS")
            {
                send(CorsPreflightResponse());
                return;
            }

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var result = await this._requestHandler(method, path);

                    send(result == null
                        ? JsonResponse(404, new { success = false, error = "Not found" })
                        : JsonResponse(200, result));
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Request handler error");
                    send(JsonResponse(500, new { success = false, error = "Internal error" }));
                }
            });
        }

        private static HttpResponse CorsPreflightResponse()
        {
            var response = new HttpResponse();
            response.SetBegin(200);
            response.SetHeader("Access-Control-Allow-Origin", "*");
            response.SetHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.SetHeader("Access-Control-Allow-Headers", "Content-Type");
            response.SetHeader("Access-Control-Allow-Private-Network", "true");
            response.SetHeader("Access-Control-Max-Age", "86400");
            response.SetBody();
            return response;
        }

        private static HttpResponse JsonResponse(Int32 statusCode, Object data)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

            var response = new HttpResponse();
            response.SetBegin(statusCode);
            response.SetHeader("Content-Type", "application/json; charset=utf-8");
            response.SetHeader("Access-Control-Allow-Origin", "*");
            response.SetHeader("Access-Control-Allow-Private-Network", "true");
            response.SetBody(body);
            return response;
        }
    }

    /// <summary>
    /// Rejects oversized input before NetCoreServer buffers it. NetCoreServer has no
    /// request or WebSocket message size limit of its own and buffers a whole frame
    /// (whatever length the client declares) before handing it to us.
    /// Every haptic command is a single byte, so the limits are tiny.
    ///
    /// In WebSocket mode it also holds back a frame header that is split across reads
    /// and forwards it whole with the next read: NetCoreServer 8.0.7 restarts its header
    /// loops from zero on a split header and would misparse the stream, which would also
    /// put it out of step with this guard.
    /// </summary>
    internal sealed class ReceiveGuard
    {
        // Largest payload that fits the 7-bit length field; frames needing an
        // extended length are never valid for this protocol.
        public const Int32 MaxFramePayload = 125;
        public const Int32 MaxMessageBytes = 1024;
        public const Int32 MaxHttpRequestBytes = 16 * 1024;

        private readonly Byte[] _header = new Byte[6];
        private Int32 _headerLength;
        private Int64 _payloadRemaining;
        private Int64 _messageLength;
        private Int64 _httpBytes;

        public Boolean IsWebSocket { get; private set; }

        public void EnterWebSocketMode()
        {
            this.IsWebSocket = true;
            this._httpBytes = 0;
        }

        public void HttpRequestCompleted() => this._httpBytes = 0;

        /// <summary>
        /// Checks received bytes and rewrites the range to forward to NetCoreServer
        /// (which may be empty, or include header bytes held back from the previous read).
        /// </summary>
        /// <returns>false if the connection should be dropped.</returns>
        public Boolean Filter(ref Byte[] buffer, ref Int64 offset, ref Int64 size)
        {
            if (!this.IsWebSocket)
            {
                this._httpBytes += size;
                return this._httpBytes <= MaxHttpRequestBytes;
            }

            if (this._headerLength > 0)
            {
                var combined = new Byte[this._headerLength + size];
                Array.Copy(this._header, combined, this._headerLength);
                Array.Copy(buffer, offset, combined, this._headerLength, size);
                buffer = combined;
                offset = 0;
                size = combined.Length;
                this._headerLength = 0;
            }

            var end = offset + size;
            var i = offset;
            var frameStart = offset;
            while (i < end)
            {
                if (this._payloadRemaining > 0)
                {
                    var skip = Math.Min(this._payloadRemaining, end - i);
                    i += skip;
                    this._payloadRemaining -= skip;
                    continue;
                }

                if (this._headerLength == 0)
                {
                    frameStart = i;
                }

                this._header[this._headerLength++] = buffer[i++];
                if (this._headerLength < 2)
                {
                    continue;
                }

                var payloadLength = this._header[1] & 0x7F;
                if (payloadLength > MaxFramePayload)
                {
                    return false;
                }

                var masked = (this._header[1] & 0x80) != 0;
                if (this._headerLength < (masked ? 6 : 2))
                {
                    continue;
                }

                // NetCoreServer appends every frame's payload (control frames included)
                // to one message buffer until a frame with FIN set, so count them all.
                this._messageLength += payloadLength;
                if (this._messageLength > MaxMessageBytes)
                {
                    return false;
                }

                if ((this._header[0] & 0x80) != 0)
                {
                    this._messageLength = 0;
                }

                this._payloadRemaining = payloadLength;
                this._headerLength = 0;
            }

            if (this._headerLength > 0)
            {
                // Hold the partial header back; it is prepended to the next read.
                size = frameStart - offset;
            }

            return true;
        }
    }
}
