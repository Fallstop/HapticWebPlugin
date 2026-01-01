namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using HidSharp;

    /// <summary>
    /// HID++ 2.0/4.0 protocol implementation for communicating with Logitech devices.
    /// Used to query the Bolt receiver for connected devices.
    /// </summary>
    public class HidPlusPlusProtocol
    {
        // HID++ Report IDs
        private const Byte ShortReportId = 0x10;   // 7 bytes payload
        private const Byte LongReportId = 0x11;    // 20 bytes payload
        private const Byte VeryLongReportId = 0x12; // 64 bytes payload

        // Device indices
        private const Byte ReceiverIndex = 0xFF;
        private const Byte DeviceIndex1 = 0x01;
        private const Byte DeviceIndex2 = 0x02;
        private const Byte DeviceIndex3 = 0x03;
        private const Byte DeviceIndex4 = 0x04;
        private const Byte DeviceIndex5 = 0x05;
        private const Byte DeviceIndex6 = 0x06;

        // HID++ 1.0 Registers (for receiver queries)
        private const Byte SubIdGetRegister = 0x81;
        private const Byte SubIdSetRegister = 0x80;

        // Receiver registers
        private const Byte RegisterEnableNotifications = 0x00;
        private const Byte RegisterConnectionState = 0x02;
        private const Byte RegisterDevicePairing = 0xB5;

        // HID++ 2.0 Feature indices (common)
        private const UInt16 FeatureRoot = 0x0000;
        private const UInt16 FeatureDeviceInfo = 0x0003;
        private const UInt16 FeatureDeviceName = 0x0005;

        // Error codes
        private const Byte ErrorInvalidSubId = 0x01;
        private const Byte ErrorInvalidAddress = 0x02;
        private const Byte ErrorInvalidValue = 0x03;
        private const Byte ErrorConnectFail = 0x04;
        private const Byte ErrorTooManyDevices = 0x05;
        private const Byte ErrorAlreadyExists = 0x06;
        private const Byte ErrorBusy = 0x07;
        private const Byte ErrorUnknownDevice = 0x08;
        private const Byte ErrorResourceError = 0x09;
        private const Byte ErrorRequestUnavailable = 0x0A;
        private const Byte ErrorInvalidParamValue = 0x0B;
        private const Byte ErrorWrongPinCode = 0x0C;

        private const Int32 LogitechVendorId = 0x046D;

        public class ConnectedDevice
        {
            public Byte DeviceIndex { get; set; }
            public String DeviceName { get; set; }
            public UInt16 DeviceType { get; set; }
            public Boolean IsConnected { get; set; }
            public String WirelessPid { get; set; }
        }

        public class BoltQueryResult
        {
            public Boolean Success { get; set; }
            public String Error { get; set; }
            public List<ConnectedDevice> ConnectedDevices { get; set; } = new List<ConnectedDevice>();
            public String RawResponse { get; set; }
        }

        /// <summary>
        /// Query the Bolt receiver for connected devices using HID++ protocol.
        /// </summary>
        public BoltQueryResult QueryBoltReceiverDevices()
        {
            var result = new BoltQueryResult();

            try
            {
                var devices = DeviceList.Local.GetHidDevices()
                    .Where(d => d.VendorID == LogitechVendorId)
                    .Where(d => d.ProductID == 0xC548 || d.ProductID == 0xC53A) // Bolt receivers
                    .ToList();

                if (!devices.Any())
                {
                    result.Success = false;
                    result.Error = "No Bolt receiver found";
                    return result;
                }

                // Find the HID interface that supports HID++ (usually the one with the right report descriptor)
                HidDevice targetDevice = null;
                foreach (var device in devices)
                {
                    try
                    {
                        var reportDescriptor = device.GetReportDescriptor();
                        // Look for device that can handle our reports
                        // HID++ devices typically have input/output reports of 7, 20, or 64 bytes
                        var inputReports = reportDescriptor.InputReports.ToList();
                        if (inputReports.Any(r => r.Length >= 7))
                        {
                            targetDevice = device;
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (targetDevice == null)
                {
                    // Just use the first one
                    targetDevice = devices.First();
                }

                // Try to open the device and query it
                using (var stream = targetDevice.Open())
                {
                    stream.ReadTimeout = 500;
                    stream.WriteTimeout = 500;

                    // Query each possible device slot (1-6)
                    for (Byte deviceIndex = 1; deviceIndex <= 6; deviceIndex++)
                    {
                        var connectedDevice = this.QueryDeviceSlot(stream, targetDevice, deviceIndex);
                        if (connectedDevice != null && connectedDevice.IsConnected)
                        {
                            result.ConnectedDevices.Add(connectedDevice);
                        }
                    }

                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                PluginLog.Warning($"HID++ query failed: {ex.Message}");
            }

            return result;
        }

        private ConnectedDevice QueryDeviceSlot(HidStream stream, HidDevice device, Byte deviceIndex)
        {
            try
            {
                // Send HID++ 1.0 request to get device pairing info
                // Register 0xB5 with param 0x20+n gives pairing info for device n
                var request = new Byte[7];
                request[0] = ShortReportId;      // Report ID
                request[1] = ReceiverIndex;       // Query the receiver
                request[2] = SubIdGetRegister;   // Get register
                request[3] = RegisterDevicePairing; // Pairing info register (0xB5)
                request[4] = (Byte)(0x20 + deviceIndex - 1); // Pairing info for this device slot
                request[5] = 0x00;
                request[6] = 0x00;

                stream.Write(request);

                // Read response with short timeout
                var response = new Byte[20];
                Int32 bytesRead;
                try
                {
                    bytesRead = stream.Read(response);
                }
                catch (TimeoutException)
                {
                    return null;
                }

                if (bytesRead >= 7)
                {
                    var rawHex = BitConverter.ToString(response.Take(bytesRead).ToArray());

                    // Check if this is an error response (0x8F = error)
                    if (response[0] == 0x8F)
                    {
                        // Error response - device slot empty or invalid
                        return null;
                    }

                    // Check for successful response from receiver
                    if (response[0] == ShortReportId && 
                        response[1] == ReceiverIndex && 
                        response[2] == SubIdGetRegister &&
                        response[3] == RegisterDevicePairing)
                    {
                        // Response format for pairing info:
                        // [4] = destination ID
                        // [5] = report interval
                        // [6] = wireless PID (low)
                        // [7] = wireless PID (high) - only in long reports

                        var wirelessPidLow = response[4];
                        var reportInterval = response[5];
                        var deviceTypeByte = response[6];

                        // If all zeros or specific error pattern, device not present
                        if (wirelessPidLow == 0x00 && reportInterval == 0x00 && deviceTypeByte == 0x00)
                        {
                            return null;
                        }

                        return new ConnectedDevice
                        {
                            DeviceIndex = deviceIndex,
                            IsConnected = true,
                            DeviceType = deviceTypeByte,
                            WirelessPid = $"0x{wirelessPidLow:X2}"
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Error querying device slot {deviceIndex}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get device name using HID++ 2.0 protocol.
        /// Queries the device directly for its name using feature 0x0005.
        /// </summary>
        private String GetDeviceNameViaHidPP1(HidStream stream, Byte deviceIndex)
        {
            try
            {
                // HID++ 2.0 approach: Query the device directly (not the receiver)
                // First, get the feature index for DeviceName (0x0005) via IRoot (0x0000)

                // Step 1: Query IRoot.GetFeatureIndex(0x0005) to find the DeviceName feature
                var request = new Byte[7];
                request[0] = ShortReportId;
                request[1] = deviceIndex;  // Query the device, not receiver
                request[2] = 0x00;         // Feature index 0 = IRoot
                request[3] = 0x00;         // Function 0 = GetFeatureIndex
                request[4] = 0x00;         // Feature ID high byte (0x0005 >> 8)
                request[5] = 0x05;         // Feature ID low byte (0x0005 & 0xFF)
                request[6] = 0x00;

                stream.Write(request);

                var response = new Byte[20];
                Int32 bytesRead;
                try
                {
                    bytesRead = stream.Read(response);
                }
                catch (TimeoutException)
                {
                    return null;
                }

                // Check for error response
                if (bytesRead < 7 || response[0] == 0x8F)
                {
                    return null;
                }

                // Response should contain the feature index at byte 4
                Byte deviceNameFeatureIndex = response[4];

                if (deviceNameFeatureIndex == 0)
                {
                    // Feature not supported
                    return null;
                }

                // Step 2: Query DeviceName.GetCount() to get name length
                request = new Byte[7];
                request[0] = ShortReportId;
                request[1] = deviceIndex;
                request[2] = deviceNameFeatureIndex;  // DeviceName feature index
                request[3] = 0x00;                     // Function 0 = GetCount
                request[4] = 0x00;
                request[5] = 0x00;
                request[6] = 0x00;

                stream.Write(request);

                try
                {
                    bytesRead = stream.Read(response);
                }
                catch (TimeoutException)
                {
                    return null;
                }

                if (bytesRead < 5 || response[0] == 0x8F)
                {
                    return null;
                }

                Byte nameLength = response[4];
                if (nameLength == 0 || nameLength > 32)
                {
                    return null;
                }

                // Step 3: Query DeviceName.GetName() to get the actual name
                // Use long report to get more characters
                var longRequest = new Byte[20];
                longRequest[0] = LongReportId;
                longRequest[1] = deviceIndex;
                longRequest[2] = deviceNameFeatureIndex;
                longRequest[3] = 0x10;  // Function 1 = GetName (0x10 in HID++ 2.0)
                longRequest[4] = 0x00;  // Starting character index
                // Rest are zeros

                stream.Write(longRequest);

                var longResponse = new Byte[20];
                try
                {
                    bytesRead = stream.Read(longResponse);
                }
                catch (TimeoutException)
                {
                    return null;
                }

                if (bytesRead < 5 || longResponse[0] == 0x8F)
                {
                    return null;
                }

                // Extract name from response (bytes 4+)
                var nameBuilder = new StringBuilder();
                for (Int32 i = 4; i < bytesRead && i < 4 + nameLength; i++)
                {
                    if (longResponse[i] == 0x00)
                    {
                        break;
                    }
                    if (longResponse[i] >= 0x20 && longResponse[i] <= 0x7E)
                    {
                        nameBuilder.Append((Char)longResponse[i]);
                    }
                }

                var name = nameBuilder.ToString().Trim();
                return String.IsNullOrEmpty(name) ? null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Query receiver for basic connection state using HID++ 1.0 protocol.
        /// </summary>
        public BoltQueryResult QueryReceiverConnectionState()
        {
            var result = new BoltQueryResult();

            try
            {
                var devices = DeviceList.Local.GetHidDevices()
                    .Where(d => d.VendorID == LogitechVendorId)
                    .Where(d => d.ProductID == 0xC548 || d.ProductID == 0xC53A)
                    .ToList();

                if (!devices.Any())
                {
                    result.Success = false;
                    result.Error = "No Bolt receiver found";
                    return result;
                }

                // Try each HID interface
                foreach (var device in devices)
                {
                    try
                    {
                        using (var stream = device.Open())
                        {
                            stream.ReadTimeout = 200;
                            stream.WriteTimeout = 200;

                            // Query connection state register
                            var request = new Byte[7];
                            request[0] = ShortReportId;
                            request[1] = ReceiverIndex;
                            request[2] = SubIdGetRegister;
                            request[3] = RegisterConnectionState;
                            request[4] = 0x00;
                            request[5] = 0x00;
                            request[6] = 0x00;

                            stream.Write(request);

                            // Read response
                            var response = new Byte[20];
                            var bytesRead = stream.Read(response);

                            if (bytesRead > 0)
                            {
                                result.Success = true;
                                result.RawResponse = BitConverter.ToString(response.Take(bytesRead).ToArray());

                                // Parse connection state
                                // Response format: [ReportID][DeviceIdx][SubId][Register][P0][P1][P2]
                                // For register 0x02, P1 contains connection flags for each device slot
                                if (response[0] == ShortReportId && response[1] == ReceiverIndex && response[2] == SubIdGetRegister)
                                {
                                    // P1 (byte 5) contains the connection bitmap
                                    var connectionFlags = response[5];
                                    for (Byte i = 0; i < 6; i++)
                                    {
                                        if ((connectionFlags & (1 << i)) != 0)
                                        {
                                            var deviceIdx = (Byte)(i + 1);
                                            var deviceName = this.GetDeviceNameViaHidPP1(stream, deviceIdx);

                                            result.ConnectedDevices.Add(new ConnectedDevice
                                            {
                                                DeviceIndex = deviceIdx,
                                                IsConnected = true,
                                                DeviceName = deviceName ?? $"Device {deviceIdx}"
                                            });
                                        }
                                    }
                                }

                                return result;
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Warning($"HID++ connection state query failed on interface: {ex.Message}");
                        continue;
                    }
                }

                result.Success = false;
                result.Error = "Could not communicate with receiver";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Simple check if any device is connected to the Bolt receiver.
        /// Uses a less intrusive method - just tries to read any HID++ traffic.
        /// </summary>
        public Boolean IsAnyDeviceActive()
        {
            try
            {
                var devices = DeviceList.Local.GetHidDevices()
                    .Where(d => d.VendorID == LogitechVendorId)
                    .Where(d => d.ProductID == 0xC548 || d.ProductID == 0xC53A)
                    .ToList();

                // If Bolt receiver is present, we can assume a device might be connected
                // A more thorough check would query the actual connection state
                return devices.Any();
            }
            catch
            {
                return false;
            }
        }
    }
}
