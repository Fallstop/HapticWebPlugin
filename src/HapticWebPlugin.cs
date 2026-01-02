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

    using Loupedeck.HapticWebPlugin.Helpers;

    public class HapticWebPlugin : Plugin
    {
        private const Int32 HttpPort = 41080;
        private const Int32 HttpsPort = 41443;

        private CertificateManager _certificateManager;
        private HttpsServer _httpsServer;

        private static readonly List<String> HapticWaveforms = new List<String>
        {
            "sharp_collision",
            "sharp_state_change",
            "knock",
            "damp_collision",
            "mad",
            "ringing",
            "subtle_collision",
            "completed",
            "jingle",
            "damp_state_change",
            "firework",
            "happy_alert",
            "wave",
            "angry_alert",
            "square"
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
            _ = this.InitializeServerAsync();
        }

        public override void Unload()
        {
            this._httpsServer?.Dispose();
            this._certificateManager?.Dispose();
        }

        private async Task InitializeServerAsync()
        {
            try
            {
                var pluginDataDir = this.GetPluginDataDirectory();
                var certCacheDir = Path.Combine(pluginDataDir, "certificates");

                this._certificateManager = new CertificateManager(certCacheDir);
                await this._certificateManager.InitializeAsync();

                this.UpdatePluginStatus();

                this._httpsServer = new HttpsServer(
                    HttpPort,
                    HttpsPort,
                    this._certificateManager.Certificate,
                    this.HandleHttpRequest,
                    this.RunHapticByIndex,
                    HapticWaveforms);

                this._httpsServer.Start();

                if (!String.IsNullOrEmpty(this._httpsServer.BindError))
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, this._httpsServer.BindError);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to initialize server");
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Server failed to start. Try restarting the plugin.");
            }
        }

        private void UpdatePluginStatus()
        {
            switch (this._certificateManager.Status)
            {
                case CertificateStatus.Error:
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, this._certificateManager.StatusMessage);
                    break;

                case CertificateStatus.Expired:
                    this.OnPluginStatusChanged(
                        Loupedeck.PluginStatus.Warning,
                        this._certificateManager.StatusMessage,
                        "https://github.com/fallstop/HapticWebPlugin/issues",
                        "Report Issue");
                    break;

                case CertificateStatus.ExpiringSoon:
                    this.OnPluginStatusChanged(
                        Loupedeck.PluginStatus.Warning,
                        this._certificateManager.StatusMessage,
                        null,
                        null);
                    break;

                case CertificateStatus.Valid:
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, null);
                    break;

                default:
                    break;
            }
        }

        private Task<Object> HandleHttpRequest(String method, String path)
        {
            Object result = null;

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

            return Task.FromResult(result);
        }

        private Object HandleHealthCheck()
        {
            return new
            {
                success = true,
                service = "HapticWebPlugin",
                version = "1.0.0",
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
            return new
            {
                success = true,
                count = HapticWaveforms.Count,
                waveforms = HapticWaveforms
            };
        }

        private Object HandleTriggerHaptic(String waveform)
        {
            if (String.IsNullOrWhiteSpace(waveform))
            {
                return new { success = false, error = "Waveform name is required" };
            }

            if (!HapticWaveforms.Contains(waveform))
            {
                return new
                {
                    success = false,
                    error = $"Unknown waveform: {waveform}",
                    availableWaveforms = HapticWaveforms
                };
            }

            PluginLog.Verbose($"Queueing haptic: {waveform}");
            Task.Run(() =>
            {
                PluginLog.Verbose($"Starting haptic task: {waveform}");
                try
                {
                    this.PluginEvents.RaiseEvent(waveform);
                    PluginLog.Info($"Triggered haptic: {waveform}");
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, $"Failed haptic task: {waveform}");
                }
            });

            PluginLog.Verbose($"Returning response for: {waveform}");
            return new
            {
                success = true,
                waveform = waveform
            };
        }

        private void RegisterHapticEvents()
        {
            foreach (var waveform in HapticWaveforms)
            {
                this.PluginEvents.AddEvent(waveform, waveform, null);
            }
            PluginLog.Info($"Registered {HapticWaveforms.Count} haptic events");
        }

        private void RunHapticByIndex(Int32 index)
        {
            if (index < 0 || index >= HapticWaveforms.Count)
            {
                PluginLog.Warning($"Invalid haptic index: {index}");
                return;
            }

            var waveform = HapticWaveforms[index];
            try
            {
                this.PluginEvents.RaiseEvent(waveform);
                PluginLog.Verbose($"Triggered haptic by index: {index} -> {waveform}");
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, $"Failed to trigger haptic: {waveform}");
            }
        }
    }
}
