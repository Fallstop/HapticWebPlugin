namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using HidSharp;

    public enum DeviceConnectionType
    {
        None,
        Bluetooth,
        LogiBolt,
        Unknown
    }

    public class DeviceStatus
    {
        public Boolean IsConnected { get; set; }
        public DeviceConnectionType ConnectionType { get; set; }
        public String DeviceName { get; set; }
        public String ProductId { get; set; }
        public String VendorId { get; set; }
    }

    public class DeviceDetector : IDisposable
    {
        private const Int32 LogitechVendorId = 0x046D;

        private static readonly HashSet<Int32> MxMaster4ProductIds = new HashSet<Int32>
        {
            0xB042, // MX Master 4 Bluetooth
            0xB043, // MX Master 4 Bluetooth (alternate)
            0x4108, // MX Master 4 via Bolt receiver (paired device)
        };

        private static readonly HashSet<Int32> LogiBoltReceiverProductIds = new HashSet<Int32>
        {
            0xC548, // Logi Bolt Receiver
            0xC53A, // Logi Bolt Receiver (alternate)
        };

        private Timer _pollingTimer;
        private DeviceStatus _cachedStatus;
        private readonly Object _lock = new Object();

        public event EventHandler<DeviceStatus> DeviceStatusChanged;

        public DeviceDetector()
        {
            this._cachedStatus = new DeviceStatus { IsConnected = false, ConnectionType = DeviceConnectionType.None };
        }

        public void StartPolling(TimeSpan interval)
        {
            this._pollingTimer = new Timer(
                _ => this.PollDeviceStatus(),
                null,
                TimeSpan.Zero,
                interval);
        }

        public void StopPolling()
        {
            this._pollingTimer?.Dispose();
            this._pollingTimer = null;
        }

        private void PollDeviceStatus()
        {
            try
            {
                var newStatus = this.DetectMxMaster4();
                lock (this._lock)
                {
                    if (newStatus.IsConnected != this._cachedStatus.IsConnected ||
                        newStatus.ConnectionType != this._cachedStatus.ConnectionType)
                    {
                        this._cachedStatus = newStatus;
                        this.DeviceStatusChanged?.Invoke(this, newStatus);
                    }
                    else
                    {
                        this._cachedStatus = newStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Device polling error: {ex.Message}");
            }
        }

        public DeviceStatus GetCachedStatus()
        {
            lock (this._lock)
            {
                return this._cachedStatus;
            }
        }

        public DeviceStatus DetectMxMaster4()
        {
            try
            {
                var devices = DeviceList.Local.GetHidDevices();
                var logitechDevices = devices.Where(d => d.VendorID == LogitechVendorId).ToList();

                // Check for direct MX Master 4 connection via USB HID
                foreach (var device in logitechDevices)
                {
                    if (MxMaster4ProductIds.Contains(device.ProductID))
                    {
                        var connectionType = this.InferConnectionType(device);
                        return new DeviceStatus
                        {
                            IsConnected = true,
                            ConnectionType = connectionType,
                            DeviceName = this.GetSafeDeviceName(device),
                            ProductId = $"0x{device.ProductID:X4}",
                            VendorId = $"0x{device.VendorID:X4}"
                        };
                    }
                }

                // Check for Logi Bolt receiver - use HID++ to verify a device is actually connected
                foreach (var device in logitechDevices)
                {
                    if (LogiBoltReceiverProductIds.Contains(device.ProductID))
                    {
                        // Use HID++ to check if any device is connected to the receiver
                        var hidpp = new HidPlusPlusProtocol();
                        var connectionState = hidpp.QueryReceiverConnectionState();

                        if (connectionState.Success && connectionState.ConnectedDevices.Any())
                        {
                            var connectedDevice = connectionState.ConnectedDevices.First();
                            return new DeviceStatus
                            {
                                IsConnected = true,
                                ConnectionType = DeviceConnectionType.LogiBolt,
                                DeviceName = connectedDevice.DeviceName ?? "MX Master 4 (via Logi Bolt)",
                                ProductId = $"0x{device.ProductID:X4}",
                                VendorId = $"0x{device.VendorID:X4}"
                            };
                        }
                        else
                        {
                            // Bolt receiver present but no device connected
                            return new DeviceStatus
                            {
                                IsConnected = false,
                                ConnectionType = DeviceConnectionType.None,
                                DeviceName = "Logi Bolt (no device)"
                            };
                        }
                    }
                }

                // Fallback: Check for Bluetooth connection on macOS
                var bluetoothStatus = this.DetectBluetoothMxMaster4();
                if (bluetoothStatus.IsConnected)
                {
                    return bluetoothStatus;
                }

                return new DeviceStatus
                {
                    IsConnected = false,
                    ConnectionType = DeviceConnectionType.None
                };
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to detect MX Master 4");
                return new DeviceStatus
                {
                    IsConnected = false,
                    ConnectionType = DeviceConnectionType.None
                };
            }
        }

        private DeviceStatus DetectBluetoothMxMaster4()
        {
            // On macOS, use system_profiler to detect Bluetooth devices
            // On Windows, Bluetooth HID devices should appear in HidSharp
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new DeviceStatus { IsConnected = false, ConnectionType = DeviceConnectionType.None };
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"system_profiler SPBluetoothDataType 2>/dev/null\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                // Parse the output to find MX Master 4 in connected devices
                // Look for pattern: "MX Master 4:" followed by connection info (not in "Not Connected" section)
                var lines = output.Split('\n');
                var inConnectedSection = false;
                var inNotConnectedSection = false;
                var foundMxMaster4 = false;
                String productId = null;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Track which section we're in
                    if (trimmedLine.StartsWith("Connected:") || trimmedLine.StartsWith("Devices (Paired, Configured, & Connected):"))
                    {
                        inConnectedSection = true;
                        inNotConnectedSection = false;
                    }
                    else if (trimmedLine.StartsWith("Not Connected:") || trimmedLine.StartsWith("Devices (Paired, Not Connected):"))
                    {
                        inConnectedSection = false;
                        inNotConnectedSection = true;
                    }

                    // Look for MX Master 4 in connected section
                    if (inConnectedSection && !inNotConnectedSection)
                    {
                        if (trimmedLine.StartsWith("MX Master 4"))
                        {
                            foundMxMaster4 = true;
                        }
                        else if (foundMxMaster4 && trimmedLine.StartsWith("Product ID:"))
                        {
                            var match = Regex.Match(trimmedLine, @"0x([0-9A-Fa-f]+)");
                            if (match.Success)
                            {
                                productId = $"0x{match.Groups[1].Value.ToUpper()}";
                            }

                            return new DeviceStatus
                            {
                                IsConnected = true,
                                ConnectionType = DeviceConnectionType.Bluetooth,
                                DeviceName = "MX Master 4",
                                ProductId = productId ?? "0xB042",
                                VendorId = "0x046D"
                            };
                        }
                    }
                }

                return new DeviceStatus { IsConnected = false, ConnectionType = DeviceConnectionType.None };
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Bluetooth detection failed: {ex.Message}");
                return new DeviceStatus { IsConnected = false, ConnectionType = DeviceConnectionType.None };
            }
        }

        public List<Object> ListAllLogitechDevices()
        {
            var result = new List<Object>();

            try
            {
                var devices = DeviceList.Local.GetHidDevices();
                var logitechDevices = devices.Where(d => d.VendorID == LogitechVendorId);

                foreach (var device in logitechDevices)
                {
                    result.Add(new
                    {
                        name = this.GetSafeDeviceName(device),
                        productId = $"0x{device.ProductID:X4}",
                        vendorId = $"0x{device.VendorID:X4}",
                        isMxMaster4 = MxMaster4ProductIds.Contains(device.ProductID),
                        isBoltReceiver = LogiBoltReceiverProductIds.Contains(device.ProductID)
                    });
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to list Logitech devices");
            }

            return result;
        }

        private DeviceConnectionType InferConnectionType(HidDevice device)
        {
            var productId = device.ProductID;

            // Bluetooth product IDs typically start with 0xB0xx
            if ((productId & 0xFF00) == 0xB000)
            {
                return DeviceConnectionType.Bluetooth;
            }

            // Bolt-connected devices typically have different product IDs
            if ((productId & 0xFF00) == 0x4100)
            {
                return DeviceConnectionType.LogiBolt;
            }

            return DeviceConnectionType.Unknown;
        }

        private String GetSafeDeviceName(HidDevice device)
        {
            try
            {
                return device.GetProductName() ?? $"Logitech Device (0x{device.ProductID:X4})";
            }
            catch
            {
                return $"Logitech Device (0x{device.ProductID:X4})";
            }
        }

        public void Dispose()
        {
            this.StopPolling();
        }
    }
}
