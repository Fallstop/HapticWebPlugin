namespace Loupedeck.HapticWebPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class HapticWebPlugin : Plugin
    {
        private const Int32 ServerPort = 8765;
        private const String ServerHost = "127.0.0.1";

        private HttpListener _httpListener;
        private CancellationTokenSource _cts;
        private Task _serverTask;

        private static readonly Dictionary<String, String> HapticWaveforms = new Dictionary<String, String>
        {
            { "sharp_state_change", "Short, high-intensity pulse for discrete state transitions" },
            { "damp_state_change", "Gradual intensity change for smooth state transitions" },
            { "sharp_collision", "High-intensity impact simulation for collision events" },
            { "damp_collision", "Medium-intensity impact with gradual decay" },
            { "subtle_collision", "Low-intensity feedback for light contact events" },
            { "happy_alert", "Positive feedback pattern for success states" },
            { "angry_alert", "Attention-grabbing pattern for error conditions" },
            { "completed", "Confirmation pattern for task completion" },
            { "square", "Sharp-edged waveform with defined start/stop points" },
            { "wave", "Smooth sinusoidal pattern with gradual transitions" },
            { "firework", "Multi-burst pattern with varying intensities" },
            { "mad", "High-frequency chaotic pattern" },
            { "knock", "Repetitive impact pattern" },
            { "jingle", "Musical-style pattern with multiple tones" },
            { "ringing", "Continuous oscillating pattern" },
            { "heartbeat", "Rhythmic double-pulse pattern" }
        };

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public HapticWebPlugin()
        {
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
        }

        public override void Load()
        {
            this.RegisterHapticEvents();
            this.StartHttpServer();
        }

        public override void Unload()
        {
            this.StopHttpServer();
        }

        private void RegisterHapticEvents()
        {
            foreach (var waveform in HapticWaveforms)
            {
                this.PluginEvents.AddEvent(waveform.Key, waveform.Key, waveform.Value);
            }
            PluginLog.Info($"Registered {HapticWaveforms.Count} haptic events");
        }

        private void StartHttpServer()
        {
            try
            {
                this._cts = new CancellationTokenSource();
                this._httpListener = new HttpListener();
                this._httpListener.Prefixes.Add($"http://{ServerHost}:{ServerPort}/");
                this._httpListener.Start();

                this._serverTask = Task.Run(() => this.HandleRequestsAsync(this._cts.Token));
                PluginLog.Info($"HTTP server started on http://{ServerHost}:{ServerPort}/");
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to start HTTP server");
            }
        }

        private void StopHttpServer()
        {
            try
            {
                this._cts?.Cancel();
                this._httpListener?.Stop();
                this._httpListener?.Close();
                this._serverTask?.Wait(TimeSpan.FromSeconds(5));
                PluginLog.Info("HTTP server stopped");
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, "Error stopping HTTP server");
            }
        }

        private async Task HandleRequestsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && this._httpListener.IsListening)
            {
                try
                {
                    var context = await this._httpListener.GetContextAsync().ConfigureAwait(false);
                    _ = Task.Run(() => this.ProcessRequest(context), cancellationToken);
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Error handling HTTP request");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                var path = request.Url?.AbsolutePath?.ToLowerInvariant() ?? "/";
                var method = request.HttpMethod.ToUpperInvariant();

                PluginLog.Verbose($"HTTP {method} {path}");

                if (method == "OPTIONS")
                {
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                Object result;

                if (path == "/" && method == "GET")
                {
                    result = this.HandleHealthCheck();
                }
                else if (path == "/waveforms" && method == "GET")
                {
                    result = this.HandleListWaveforms();
                }
                else if (path.StartsWith("/haptic/") && method == "POST")
                {
                    var waveform = path.Substring("/haptic/".Length).Trim('/');
                    result = this.HandleTriggerHaptic(waveform);
                }
                else
                {
                    response.StatusCode = 404;
                    result = new { success = false, error = "Not found" };
                }

                this.WriteJsonResponse(response, result);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Error processing request");
                response.StatusCode = 500;
                this.WriteJsonResponse(response, new { success = false, error = ex.Message });
            }
            finally
            {
                response.Close();
            }
        }

        private Object HandleHealthCheck()
        {
            return new
            {
                success = true,
                service = "HapticWebPlugin",
                version = "1.0.0",
                deviceStatus = "unknown",
                deviceNote = "MX Master 4 connection cannot be detected directly. Haptics will be sent if device is connected.",
                endpoints = new
                {
                    health = "GET /",
                    listWaveforms = "GET /waveforms",
                    triggerHaptic = "POST /haptic/{waveform}"
                }
            };
        }

        private Object HandleListWaveforms()
        {
            var waveformList = new List<Object>();
            foreach (var waveform in HapticWaveforms)
            {
                waveformList.Add(new { name = waveform.Key, description = waveform.Value });
            }

            return new
            {
                success = true,
                deviceStatus = "unknown",
                count = HapticWaveforms.Count,
                waveforms = waveformList
            };
        }

        private Object HandleTriggerHaptic(String waveform)
        {
            if (String.IsNullOrWhiteSpace(waveform))
            {
                return new { success = false, error = "Waveform name is required" };
            }

            if (!HapticWaveforms.ContainsKey(waveform))
            {
                return new
                {
                    success = false,
                    error = $"Unknown waveform: {waveform}",
                    availableWaveforms = new List<String>(HapticWaveforms.Keys)
                };
            }

            try
            {
                this.PluginEvents.RaiseEvent(waveform);
                PluginLog.Info($"Triggered haptic: {waveform}");

                return new
                {
                    success = true,
                    waveform = waveform,
                    deviceStatus = "unknown",
                    note = "Haptic event raised. Will trigger if MX Master 4 is connected."
                };
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, $"Failed to trigger haptic: {waveform}");
                return new { success = false, error = $"Failed to trigger haptic: {ex.Message}" };
            }
        }

        private void WriteJsonResponse(HttpListenerResponse response, Object data)
        {
            response.ContentType = "application/json";
            response.Headers.Add("Access-Control-Allow-Origin", "*");

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
