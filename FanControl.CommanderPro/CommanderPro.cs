using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class CommanderPro
    {
        #region Private objects

        //private static HidSharp.HidDeviceLoader Commander_Loader = new HidSharp.HidDeviceLoader();

        private HidSharp.HidStream stream;

        private HidSharp.HidDevice device;

        private Byte[] outbuf = new Byte[64];

        private Byte[] inbuf = new Byte[16];

        private Boolean IsConnected = false;

        #endregion

        #region Public methods

        public void Connect()
        {
            if (IsConnected) return;

            if (HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c10))
            {
                if (String.Equals(device.GetProductName(), "Commander PRO", StringComparison.InvariantCultureIgnoreCase))
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
                else
                {
                    IsConnected = false;
                }
            }
            else
            {
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

                outbuf[1] = CorsairLightingProtocolConstants.READ_FIRMWARE_VERSION;

                try
                {
                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    for (Int32 i = 2; i < 5; ++i)
                    {
                        if (i > 2) { result = result + "." + inbuf[i]; }
                        else { result = result + inbuf[i]; }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

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
                String fanMask = ReadFanMask();

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
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Int32 GetFanSpeed(Int32 fan)
        {
            Int32 result = 0;

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = CorsairLightingProtocolConstants.READ_FAN_SPEED;
                    outbuf[2] = (Byte)fan;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    result = 256 * inbuf[2] + inbuf[3];
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public Int32 GetFanPower(Int32 fan)
        {
            Int32 result = 0;

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = CorsairLightingProtocolConstants.READ_FAN_POWER;
                    outbuf[2] = (Byte)fan;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    if (inbuf[2] <= 100)
                    {
                        result = inbuf[2];
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        public void SetFanSpeed(Int32 fan, Int32 speed)
        {
            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = CorsairLightingProtocolConstants.WRITE_FAN_SPEED;
                    outbuf[2] = (Byte)fan;
                    outbuf[3] = (Byte)(speed >> 8);
                    outbuf[4] = (Byte)(speed & 0xff);

                    stream.Write(outbuf);
                    stream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
        }

        public void SetFanPower(Int32 fan, Int32 power)
        {
            if (IsConnected && power >= 0 && power <= 100)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = CorsairLightingProtocolConstants.WRITE_FAN_POWER;
                    outbuf[2] = (Byte)fan;
                    outbuf[3] = (Byte)(power);

                    stream.Write(outbuf);
                    stream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
        }

        public Dictionary<Int32, Int32> GetFanSpeeds()
        {
            Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();

            if (IsConnected)
            {
                String fanMask = ReadFanMask();

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

                                outbuf[1] = CorsairLightingProtocolConstants.READ_FAN_SPEED;
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
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

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

        private String ReadFanMask()
        {
            String result = "";

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = CorsairLightingProtocolConstants.READ_FAN_MASK;

                    stream.Write(outbuf);
                    stream.Read(inbuf);

                    for (Int32 i = 2; i < 8; ++i)
                    {
                        result = result + inbuf[i].ToString();
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText("err.log", exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            if (result.Length != 6)
            {
                result = "000000";
            }

            return result;
        }

        #endregion
    }
}