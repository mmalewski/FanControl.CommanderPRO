using System;

namespace FanControl.CommanderPro.Core
{
    public class CommanderCoreSWMode
    {
        #region Private objects

        private HidSharp.HidDevice device;

        private HidSharp.HidStream stream;

        private Boolean IsConnected = false;

        #endregion

        #region Public methods

        public void Connect()
        {
            IsConnected = false;

            HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c1c);

            if (device != null)
            {
                if (device.TryOpen(out stream))
                {
                    stream.Closed += Stream_Closed;

                    IsConnected = true;
                }
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            stream.Dispose();
            stream = null;
            device = null;

            IsConnected = false;
        }

        public String GetFirmwareVersion()
        {
            String result = "0.0.0";

            if (IsConnected)
            {
                //SendCommand(Constants.COMMAND_RESET);
                Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

                if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
                {
                    result = $"{response[4]}.{response[5]}.{response[6]}";
                }
            }

            return result;
        }

        public Byte[] ReadDevice()
        {
            return stream.Read();
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
            catch (System.IO.IOException exception)
            {
                Disconnect();

                System.Threading.Thread.Sleep(5000);

                Connect();
            }
            catch (Exception exception)
            {

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

        private void Stream_Closed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}