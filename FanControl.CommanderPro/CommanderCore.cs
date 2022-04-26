using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro
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

        private String FirmwareVersion;

        private Byte[] outbuf = new Byte[CommanderCoreProtocolConstants.COMMAND_SIZE];

        private Byte[] inbuf = new Byte[CommanderCoreProtocolConstants.RESPONSE_SIZE];

        private Boolean IsConnected = false;

        static readonly Object UpdateLock = new Object();

        private List<Int32> Channels = new List<Int32>();

        private Dictionary<Int32, Int32> FanSpeeds = new Dictionary<Int32, Int32>();

        private Dictionary<Int32, Int32> Temperatures = new Dictionary<Int32, Int32>();

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core;

        #endregion

        #region Public methods

        public void Connect()
        {
            if (IsConnected) return;

            IsConnected = false;

            try
            {
                FirmwareVersion = String.Empty;

                HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c1c);

                if (device != null)
                {
                    if (device.TryOpen(out stream))
                    {
                        IsConnected = true;

                        if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                        {
                            System.IO.File.AppendAllText(TraceLogFileName, $"Found device: {device.GetProductName()}" + Environment.NewLine);
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

            stream.Dispose();
            stream = null;

            IsConnected = false;
        }

        public String GetFirmwareVersion()
        {
            String result = String.Empty;

            if (IsConnected)
            {
                try
                {
                    //SendCommand(CommanderCoreProtocolConstants.COMMAND_WAKE);
                    inbuf = SendCommand(CommanderCoreProtocolConstants.READ_FIRMWARE_VERSION);

                    result = $"{inbuf[4]}.{inbuf[5]}.{inbuf[6]}";

                    if (result != null && !String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Commander CORE Firmware v{result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
                finally
                {
                    //SendCommand(CommanderCoreProtocolConstants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        public List<Int32> GetFanChannels()
        {
            return GetFanChannels(0);
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            return GetFanSpeed(channel, 0);
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

        private void ClearOutputBuffer()
        {
            outbuf = new Byte[CommanderCoreProtocolConstants.COMMAND_SIZE];
        }

        private Byte[] SendCommand(Byte[] command, Byte[] data = null, Int32 attempts = 0)
        {
            Byte[] result = new Byte[CommanderCoreProtocolConstants.RESPONSE_SIZE];

            if (attempts <= 1)
            {
                try
                {
                    ClearOutputBuffer();

                    outbuf[1] = 0x08;

                    Int32 outputIndex = 2;

                    foreach (Byte commandByte in command)
                    {
                        outbuf[outputIndex] = commandByte;

                        outputIndex++;
                    }

                    if (data != null)
                    {
                        foreach (Byte dataByte in data)
                        {
                            outbuf[outputIndex] = dataByte;

                            outputIndex++;
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Sending command data: {BitConverter.ToString(outbuf)}" + Environment.NewLine);
                    }

                    //Int32 maxInputLength = device.GetMaxInputReportLength();
                    //Int32 maxOutputLength = device.GetMaxOutputReportLength();

                    stream.Write(outbuf);
                    stream.Read(result);

                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Received data: {BitConverter.ToString(result)}" + Environment.NewLine);
                    }
                }
                catch (System.IO.IOException exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    if (String.Equals("Can't write to this device.", exception.Message, StringComparison.InvariantCultureIgnoreCase))
                    {
                        SendCommand(CommanderCoreProtocolConstants.COMMAND_RESET, attempts: attempts + 1);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
            }

            return result;
        }

        private List<Int32> GetFanChannels(Int32 attempts = 0)
        {
            if (!Channels.Any() && IsConnected && attempts < 5)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Getting fan channels - attempt: {attempts}" + Environment.NewLine);
                    }

                    SendCommand(CommanderCoreProtocolConstants.COMMAND_WAKE);
                    //SendCommand(CommanderCoreProtocolConstants.COMMAND_RESET);
                    SendCommand(CommanderCoreProtocolConstants.COMMAND_SET_MODE, CommanderCoreProtocolConstants.MODE_CONNECTED);
                    inbuf = SendCommand(CommanderCoreProtocolConstants.COMMAND_READ);

                    for (Int32 i = 0; i < CommanderCoreProtocolConstants.DATA_TYPE_CONNECTED.Length; ++i)
                    {
                        if (inbuf[4 + i] != CommanderCoreProtocolConstants.DATA_TYPE_CONNECTED[i])
                        {
                            Channels = GetFanChannels(attempts + 1);
                        }
                    }

                    Int32 totalDevices = inbuf[6];

                    for (Int32 i = 0; i < totalDevices; i++)
                    {
                        //0 = AIO Pump, not a fan so ignore

                        if (i > 0 && inbuf[i + 7] == 0x07)
                        {
                            Channels.Add(i);
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    SendCommand(CommanderCoreProtocolConstants.COMMAND_SLEEP);
                }
            }

            return Channels;
        }

        private Int32 GetFanSpeed(Int32 channel, Int32 attempts = 0)
        {
            Int32 result = 0;

            if (IsConnected && attempts < 5)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Getting fan channel {channel} speed - attempt: {attempts}" + Environment.NewLine);
                    }

                    SendCommand(CommanderCoreProtocolConstants.COMMAND_WAKE);
                    //SendCommand(CommanderCoreProtocolConstants.COMMAND_RESET);
                    SendCommand(CommanderCoreProtocolConstants.COMMAND_SET_MODE, CommanderCoreProtocolConstants.MODE_GET_SPEEDS);
                    inbuf = SendCommand(CommanderCoreProtocolConstants.COMMAND_READ);

                    for (Int32 i = 0; i < CommanderCoreProtocolConstants.DATA_TYPE_SPEEDS.Length; ++i)
                    {
                        if (inbuf[4 + i] != CommanderCoreProtocolConstants.DATA_TYPE_SPEEDS[i])
                        {
                            result = GetFanSpeed(channel, attempts + 1);
                        }
                    }

                    Int32 totalResults = inbuf[6];

                    for (Int32 i = 0; i < totalResults * 2; i++)
                    {
                        if (i != channel) continue;

                        Int32 offset = 7 + i * 2;

                        result = BitConverter.ToUInt16(inbuf, offset);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    SendCommand(CommanderCoreProtocolConstants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        #endregion
    }
}