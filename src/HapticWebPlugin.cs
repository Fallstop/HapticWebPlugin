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
        private DeviceDetector _deviceDetector;

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
            this._deviceDetector = new DeviceDetector();
            this._deviceDetector.DeviceStatusChanged += this.OnDeviceStatusChanged;
            this._deviceDetector.StartPolling(TimeSpan.FromSeconds(5));

            this.RegisterHapticEvents();
            _ = this.InitializeServerAsync();
        }

        private void OnDeviceStatusChanged(Object sender, DeviceStatus status)
        {
            if (status.IsConnected)
            {
                PluginLog.Info($"MX Master 4 connected: {status.DeviceName} via {status.ConnectionType}");
            }
            else
            {
                PluginLog.Info("MX Master 4 disconnected");
            }
        }

        public override void Unload()
        {
            this._deviceDetector?.Dispose();
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
                var certLoaded = await this._certificateManager.InitializeAsync();

                this.UpdatePluginStatus();

                this._httpsServer = new HttpsServer(
                    HttpPort,
                    HttpsPort,
                    this._certificateManager.Certificate,
                    this.HandleHttpRequest);

                this._httpsServer.Start();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to initialize server");
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, $"Server initialization failed: {ex.Message}");
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
                        "https://github.com/fallstop/HapticWebPlugin/actions",
                        "Check GitHub Actions");
                    break;

                case CertificateStatus.ExpiringSoon:
                    this.OnPluginStatusChanged(
                        Loupedeck.PluginStatus.Warning,
                        this._certificateManager.StatusMessage,
                        "https://github.com/fallstop/HapticWebPlugin/actions",
                        "Check GitHub Actions");
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
            else if (path == "/devices" && method == "GET")
            {
                result = this.HandleListDevices();
            }
            else if (path == "/devices/hidpp" && method == "GET")
            {
                result = this.HandleHidPlusPlusQuery();
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
            var deviceStatus = this._deviceDetector?.GetCachedStatus();

            return new
            {
                success = true,
                service = "HapticWebPlugin",
                version = "1.0.0",
                https = new
                {
                    enabled = this._certificateManager?.Certificate != null,
                    status = this._certificateManager?.Status.ToString() ?? "Unknown",
                    expires = this._certificateManager?.CertificateExpiry?.ToString("yyyy-MM-dd") ?? "N/A"
                },
                device = new
                {
                    connected = deviceStatus?.IsConnected ?? false,
                    connectionType = deviceStatus?.ConnectionType.ToString() ?? "Unknown",
                    name = deviceStatus?.DeviceName,
                    productId = deviceStatus?.ProductId,
                    vendorId = deviceStatus?.VendorId
                },
                endpoints = new
                {
                    health = "GET /",
                    listWaveforms = "GET /waveforms",
                    triggerHaptic = "POST /haptic/{waveform}",
                    listDevices = "GET /devices"
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

            var deviceStatus = this._deviceDetector?.GetCachedStatus();

            return new
            {
                success = true,
                deviceConnected = deviceStatus?.IsConnected ?? false,
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

            var deviceStatus = this._deviceDetector?.GetCachedStatus();

            try
            {
                this.PluginEvents.RaiseEvent(waveform);
                PluginLog.Info($"Triggered haptic: {waveform}");

                return new
                {
                    success = true,
                    waveform = waveform,
                    deviceConnected = deviceStatus?.IsConnected ?? false,
                    connectionType = deviceStatus?.ConnectionType.ToString() ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, $"Failed to trigger haptic: {waveform}");
                return new { success = false, error = $"Failed to trigger haptic: {ex.Message}" };
            }
        }

        private Object HandleListDevices()
        {
            var deviceStatus = this._deviceDetector?.GetCachedStatus();
            var allDevices = this._deviceDetector?.ListAllLogitechDevices() ?? new List<Object>();

            return new
            {
                success = true,
                mxMaster4 = new
                {
                    connected = deviceStatus?.IsConnected ?? false,
                    connectionType = deviceStatus?.ConnectionType.ToString() ?? "None",
                    name = deviceStatus?.DeviceName,
                    productId = deviceStatus?.ProductId,
                    vendorId = deviceStatus?.VendorId
                },
                logitechDevices = allDevices
            };
        }

        private Object HandleHidPlusPlusQuery()
        {
            try
            {
                var hidpp = new HidPlusPlusProtocol();

                // Try querying connected devices
                var devicesResult = hidpp.QueryBoltReceiverDevices();

                // Also try connection state query
                var stateResult = hidpp.QueryReceiverConnectionState();

                return new
                {
                    success = true,
                    deviceQuery = new
                    {
                        success = devicesResult.Success,
                        error = devicesResult.Error,
                        connectedDevices = devicesResult.ConnectedDevices.Select(d => new
                        {
                            index = d.DeviceIndex,
                            name = d.DeviceName,
                            deviceType = d.DeviceType,
                            wirelessPid = d.WirelessPid,
                            isConnected = d.IsConnected
                        }).ToList()
                    },
                    connectionState = new
                    {
                        success = stateResult.Success,
                        error = stateResult.Error,
                        rawResponse = stateResult.RawResponse,
                        connectedDevices = stateResult.ConnectedDevices.Select(d => new
                        {
                            index = d.DeviceIndex,
                            name = d.DeviceName,
                            isConnected = d.IsConnected
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "HID++ query failed");
                return new { success = false, error = ex.Message };
            }
        }

        private void RegisterHapticEvents()
        {
            foreach (var waveform in HapticWaveforms)
            {
                this.PluginEvents.AddEvent(waveform.Key, waveform.Key, waveform.Value);
            }
            PluginLog.Info($"Registered {HapticWaveforms.Count} haptic events");
        }
    }
}
