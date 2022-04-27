using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro.Pro
{
    public class CommanderPro : ICommander
    {
        #region Private objects

        private const String ErrorLogFileName = "CommanderPRO.err.log";

#if DEBUG
        private const String TraceLogFileName = "CommanderPRO.trc.log";
#else
        private const String TraceLogFileName = ""; //"CommanderPRO.trc.log";
#endif

        private HidSharp.HidDevice device;

        private HidSharp.HidStream stream;

        private Byte[] outbuf = new Byte[Constants.COMMAND_SIZE];

        private Byte[] inbuf = new Byte[Constants.RESPONSE_SIZE];

        private Boolean IsConnected = false;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Pro;

        #endregion

        #region Public methods

        public void Connect()
        {
            if (IsConnected) return;

            IsConnected = false;

            try
            {
                HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c10);

                if (device != null)
                {
                    if (device.TryOpen(out stream))
                    {
                        IsConnected = true;
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
                ClearOutputBuffer();

                outbuf[1] = Constants.READ_FIRMWARE_VERSION;

                try
                {
                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    for (Int32 i = 2; i < 5; ++i)
                    {
                        if (i > 2) { result = result + "." + inbuf[i]; }
                        else { result = result + inbuf[i]; }
                    }

                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Commander PRO Firmware v{result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public List<Int32> GetFanChannels()
        {
            List<Int32> result = new List<Int32>();

            if (IsConnected)
            {
                String fanMask = GetFanMask();

                try
                {
                    for (Int32 j = 0; j < fanMask.Length; j++)
                    {
                        Char y = fanMask[j];

                        switch (y)
                        {
                            case '1':
                            case '2':
                                result.Add(j);

                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            Int32 result = 0;

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_FAN_SPEED;
                    outbuf[2] = (Byte)channel;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    result = 256 * inbuf[2] + inbuf[3];
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Int32 GetFanPower(Int32 channel)
        {
            Int32 result = 0;

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_FAN_POWER;
                    outbuf[2] = (Byte)channel;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    if (inbuf[2] <= 100)
                    {
                        result = inbuf[2];
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public void SetFanSpeed(Int32 channel, Int32 speed)
        {
            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.WRITE_FAN_SPEED;
                    outbuf[2] = (Byte)channel;
                    outbuf[3] = (Byte)(speed >> 8);
                    outbuf[4] = (Byte)(speed & 0xff);

                    stream.Write(outbuf);
                    stream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
        }

        public void SetFanPower(Int32 channel, Int32 power)
        {
            if (IsConnected && power >= 0 && power <= 100)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.WRITE_FAN_POWER;
                    outbuf[2] = (Byte)channel;
                    outbuf[3] = (Byte)(power);

                    stream.Write(outbuf);
                    stream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
        }

        public List<Int32> GetTemperatureChannels()
        {
            List<Int32> result = new List<Int32>();

            if (IsConnected)
            {
                String temperatureMask = GetTemperatureMask();

                try
                {
                    for (Int32 j = 0; j < temperatureMask.Length; j++)
                    {
                        Char y = temperatureMask[j];

                        switch (y)
                        {
                            case '1':
                                result.Add(j);

                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Int32 GetTemperature(Int32 channel)
        {
            Int32 result = 0;

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    ClearOutputBuffer();

                    outbuf[1] = Constants.READ_TEMPERATURE_VALUE;
                    outbuf[2] = (Byte)channel;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    result = BitConverter.ToUInt16(inbuf, 1) / 100;
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Dictionary<Int32, Int32> GetFanSpeeds()
        {
            Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();

            if (IsConnected)
            {
                String fanMask = GetFanMask();

                try
                {
                    for (Int32 j = 0; j < fanMask.Length; j++)
                    {
                        Char y = fanMask[j];

                        switch (y)
                        {
                            case '1':
                            case '2':
                                ClearOutputBuffer();

                                outbuf[1] = Constants.READ_FAN_SPEED;
                                outbuf[2] = (Byte)j;

                                stream.Write(outbuf);
                                stream.Read(inbuf);

                                result.Add(j + 1, 256 * inbuf[2] + inbuf[3]);

                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Dictionary<Int32, Int32> GetTemperatures()
        {
            Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();

            if (IsConnected)
            {
                String temperatureMask = GetTemperatureMask();

                try
                {
                    for (Int32 j = 0; j < temperatureMask.Length; j++)
                    {
                        Char y = temperatureMask[j];

                        switch (y)
                        {
                            case '1':
                                ClearOutputBuffer();

                                outbuf[1] = Constants.READ_TEMPERATURE_VALUE;
                                outbuf[2] = (Byte)j;

                                stream.Write(outbuf);
                                stream.Read(inbuf);

                                Int32 i = BitConverter.ToUInt16(inbuf, 1) / 100;

                                result.Add(j + 1, i);

                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        #endregion

        #region Private methods

        private void ClearOutputBuffer()
        {
            for (Int32 i = 0; i < 64; ++i)
            {
                outbuf[i] = 0x00;
            }
        }

        private String GetFanMask()
        {
            String result = "";

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_FAN_MASK;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    for (Int32 i = 2; i < 8; ++i)
                    {
                        result = result + inbuf[i].ToString();
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            if (result.Length != 6)
            {
                result = "000000";
            }

            return result;
        }

        private String GetTemperatureMask()
        {
            String result = "";

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_TEMPERATURE_MASK;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    for (Int32 i = 2; i < 6; ++i)
                    {
                        result = result + inbuf[i].ToString();
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            if (result.Length != 4)
            {
                result = "0000";
            }

            return result;
        }

        #endregion
    }
}