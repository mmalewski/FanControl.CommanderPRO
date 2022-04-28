using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro.Core
{
    public class CommanderCore : ICommander
    {
        #region Private objects

        private const String ErrorLogFileName = "CommanderCORE.err.log";

#if DEBUG
        private const String TraceLogFileName = "CommanderCORE.trc.log";
#else
        private const String TraceLogFileName = "";
#endif

        private HidSharp.HidDevice device;

        private HidSharp.HidStream stream;

        private Boolean IsConnected = false;

        private String FirmwareVersion = "0.0.0";

        private List<Int32> Channels = new List<Int32>();

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core;

        #endregion

        #region Public methods

        public void Connect()
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Attempting to connect to Commander CORE" + Environment.NewLine);
            }

            if (IsConnected) return;

            IsConnected = false;

            try
            {
                FirmwareVersion = "0.0.0";
                Channels.Clear();

                HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c1c);

                if (device != null)
                {
                    HidSharp.OpenConfiguration openConfiguration = new HidSharp.OpenConfiguration();

                    openConfiguration.SetOption(HidSharp.OpenOption.Exclusive, true);
                    openConfiguration.SetOption(HidSharp.OpenOption.Interruptible, true);

                    var options = openConfiguration.GetOptionsList();

                    if (device.TryOpen(openConfiguration, out stream))
                    {
                        stream.InterruptRequested += Stream_InterruptRequested;

                        IsConnected = true;

                        SendCommand(Constants.COMMAND_WAKE);
                        //SendCommand(Constants.COMMAND_RESET);
                        //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);

                        if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                        {
                            System.IO.File.AppendAllText(TraceLogFileName, $"Connected to device: {device.GetProductName()}" + Environment.NewLine);
                        }
                    }
                    else
                    {
                        IsConnected = false;
                    }
                }
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                IsConnected = false;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Disconnecting from Commander CORE" + Environment.NewLine);
            }

            SendCommand(Constants.COMMAND_SLEEP);

            stream.Dispose();
            stream = null;

            IsConnected = false;
        }

        public String GetFirmwareVersion()
        {
            if (!IsConnected)
            {
                Connect();
            }

            if (IsConnected && String.Equals(FirmwareVersion, "0.0.0", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                {
                    System.IO.File.AppendAllText(TraceLogFileName, "Attempting to get Commander CORE firmware version" + Environment.NewLine);
                }

                try
                {
                    //SendCommand(Constants.COMMAND_WAKE);
                    //SendCommand(Constants.COMMAND_RESET);
                    Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
                    {
                        FirmwareVersion = $"{response[4]}.{response[5]}.{response[6]}";

                        if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                        {
                            System.IO.File.AppendAllText(TraceLogFileName, $"Commander CORE Firmware v{FirmwareVersion}" + Environment.NewLine);
                        }
                    }
                    else
                    {
                        Disconnect();
                    }
                    
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return FirmwareVersion;
        }

        public List<Int32> GetFanChannels()
        {
            if (!IsConnected)
            {
                Connect();
            }

            if (IsConnected && !Channels.Any())
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, "Getting fan channels" + Environment.NewLine);
                    }

                    //SendCommand(Constants.COMMAND_WAKE);
                    //SendCommand(Constants.COMMAND_RESET);
                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_SW_CONNECTED))
                    {
                        Int32 totalDevices = response[6];

                        for (Int32 i = 0; i < totalDevices; i++)
                        {
                            //0 = AIO Pump, not a fan so ignore

                            if (i > 0 && response[i + 7] == 0x07)
                            {
                                Channels.Add(i);
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        foreach (Int32 channel in Channels)
                        {
                            System.IO.File.AppendAllText(TraceLogFileName, $"\tFound fan on channel {channel}" + Environment.NewLine);
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return Channels;
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            Int32 result = 0;

            if (!IsConnected)
            {
                Connect();
            }

            if (IsConnected)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Getting fan channel {channel} speed" + Environment.NewLine);
                    }

                    //SendCommand(Constants.COMMAND_WAKE);
                    SendCommand(Constants.COMMAND_RESET);
                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_SPEEDS);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_SPEEDS))
                    {
                        Int32 totalResults = response[6];

                        for (Int32 i = 0; i < totalResults * 2; i++)
                        {
                            if (i != channel) continue;

                            Int32 offset = 7 + i * 2;

                            result = BitConverter.ToUInt16(response, offset);

                            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                            {
                                System.IO.File.AppendAllText(TraceLogFileName, $"\tFan channel {channel} speed: {result}" + Environment.NewLine);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        public Int32 GetFanPower(Int32 channel)
        {
            Int32 result = 0;

            return result;
        }

        public void SetFanPower(Int32 channel, Int32 power)
        {

        }

        public Int32 GetTemperature(Int32 channel)
        {
            Int32 result = 0;

            return result;
        }

        #endregion

        #region Private methods

        private Byte[] SendCommand(Byte[] command, Byte[] data = null)
        {
            Byte[] result = new Byte[Constants.RESPONSE_SIZE];

            try
            {
                Byte[] request = new Byte[Constants.COMMAND_SIZE];

                request[1] = 0x08;

                Int32 outputIndex = 2;

                foreach (Byte commandByte in command)
                {
                    request[outputIndex] = commandByte;

                    outputIndex++;
                }

                if (data != null)
                {
                    foreach (Byte dataByte in data)
                    {
                        request[outputIndex] = dataByte;

                        outputIndex++;
                    }
                }

                stream.Write(request);
                stream.Read(result);
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
            }

            return result;
        }

        private Boolean ChecksumMatches(Byte[] data, Byte[] checksum, Int32 offset = 4)
        {
            Boolean result = true;

            for (Int32 i = 0; i < checksum.Length; ++i)
            {
                if (data[offset + i] != checksum[i])
                {
                    result = false;
                }
            }

            return result;
        }

        private void Stream_InterruptRequested(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Other process wants to access device" + Environment.NewLine);
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}