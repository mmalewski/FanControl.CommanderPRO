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

        private String FirmwareVersion;

        private List<Int32> Channels = new List<Int32>();

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
                            System.IO.File.AppendAllText(TraceLogFileName, "Sending WAKE command" + Environment.NewLine);
                        }

                        Byte[] response = SendCommand(Constants.COMMAND_SLEEP);

                        response = SendCommand(Constants.COMMAND_WAKE);

                        //SendCommand(Constants.COMMAND_RESET);
                        //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);

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

            SendCommand(Constants.COMMAND_SLEEP);

            stream.Dispose();
            stream = null;

            IsConnected = false;
        }

        public String GetFirmwareVersion()
        {
            String result = FirmwareVersion;

            if (IsConnected)
            {
                try
                {
                    //SendCommand(Constants.COMMAND_WAKE);
                    //SendCommand(Constants.COMMAND_RESET);
                    Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
                    {
                        result = $"{response[4]}.{response[5]}.{response[6]}";
                    }

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
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        public List<Int32> GetFanChannels()
        {
            if (!Channels.Any() && IsConnected)
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

                    //DATA_TYPE_HW_CONNECTED

                    if (ChecksumMatches(response, Constants.DATA_TYPE_HW_CONNECTED, 2))
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

                    for (Int32 i = 0; i < Constants.DATA_TYPE_SPEEDS.Length; ++i)
                    {
                        if (response[4 + i] != Constants.DATA_TYPE_SPEEDS[i])
                        {
                            //result = GetFanSpeed(channel, attempts + 1);
                        }
                    }

                    Int32 totalResults = response[6];

                    for (Int32 i = 0; i < totalResults * 2; i++)
                    {
                        if (i != channel) continue;

                        Int32 offset = 7 + i * 2;

                        result = BitConverter.ToUInt16(response, offset);
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

        //private void WorkerThread(CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        if (IsConnected)
        //        {
        //            Byte[] data = ReadData();

        //            Boolean dataMatched = false;

        //            if (!dataMatched && ChecksumMatches(data, CommanderCoreProtocolConstants.DATA_TYPE_FIRMWARE))
        //            {
        //                dataMatched = true;

        //                FirmwareVersion = $"{data[4]}.{data[5]}.{data[6]}";
        //            }

        //            if (!dataMatched && ChecksumMatches(data, CommanderCoreProtocolConstants.DATA_TYPE_CONNECTED))
        //            {
        //                dataMatched = true;

        //                if (FanDetails == null)
        //                {
        //                    FanDetails = new List<FanData>();
        //                }

        //                //Int32 totalDevices = data[6];

        //                //for (Int32 i = 0; i < totalDevices; i++)
        //                //{
        //                //    //0 = AIO Pump, not a fan so ignore

        //                //    if (i > 0 && data[i + 7] == 0x07)
        //                //    {
        //                //        FanChannels.Add(i);
        //                //    }
        //                //}
        //            }

        //            if (!dataMatched && ChecksumMatches(data, CommanderCoreProtocolConstants.DATA_TYPE_SPEEDS))
        //            {
        //                dataMatched = true;

        //                if (FanDetails == null)
        //                {
        //                    FanDetails = new List<FanData>();
        //                }

        //                Int32 totalResults = data[6];

        //                Dictionary<Int32, Int32> results = new Dictionary<Int32, Int32>();

        //                for (Int32 i = 0; i < totalResults; i++)
        //                {
        //                    Int32 dataOffset = 7 + i * 2;
        //                    Int32 typeOffset = 7 + i * 2 + 1;

        //                    Int32 speed = BitConverter.ToUInt16(data, dataOffset);
        //                    Int32 type = data[typeOffset];

        //                    if (FanDetails.Exists(x => x.Channel == i))
        //                    {
        //                        FanDetails.First(x => x.Channel == i).Speed = speed;
        //                    }
        //                    else
        //                    {
        //                        FanDetails.Add(new FanData
        //                        {
        //                            Channel = i,
        //                            Type = type,
        //                            Speed = speed
        //                        });
        //                    }
        //                }
        //            }
        //        }

        //        try
        //        {
        //            if (!token.IsCancellationRequested)
        //            {
        //                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 100);

        //                Task delay = Task.Delay(pause, token);
        //                delay.Wait(token);
        //            }
        //        }
        //        catch (OperationCanceledException) { }
        //    }
        //}

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

                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                {
                    System.IO.File.AppendAllText(TraceLogFileName, $"Sending command data: {BitConverter.ToString(request)}" + Environment.NewLine);
                }

                stream.Write(request);
                stream.Read(result);

                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                {
                    System.IO.File.AppendAllText(TraceLogFileName, $"Received data: {BitConverter.ToString(result)}" + Environment.NewLine);
                }
            }
            catch (System.IO.IOException exception)
            {
                if (String.Equals("Can't write to this device.", exception.Message, StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
                else
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
            }

            return result;
        }

        //private Byte[] ReadData()
        //{
        //    Byte[] result = new Byte[CommanderCoreProtocolConstants.RESPONSE_SIZE];

        //    if (stream.CanRead)
        //    {
        //        try
        //        {
        //            stream.ReadTimeout = 5000;
        //            stream.Read(result);
        //        }
        //        catch (Exception exception)
        //        {
        //            SendCommand(CommanderCoreProtocolConstants.COMMAND_WAKE);
        //            SendCommand(CommanderCoreProtocolConstants.COMMAND_RESET);
        //        }
        //    }

        //    return result;
        //}

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

        #endregion
    }
}