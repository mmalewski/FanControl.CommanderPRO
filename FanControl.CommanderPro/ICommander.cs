using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public interface ICommander
    {
        #region Properties

        DeviceType Type { get; }

        #endregion

        #region Public methods

        void Connect();

        void Disconnect();

        String GetFirmwareVersion();

        List<Int32> GetFanChannels();

        Int32 GetFanSpeed(Int32 channel);

        Int32 GetFanPower(Int32 channel);

        List<Int32> GetTemperatureChannels();

        Single GetTemperature(Int32 channel);

        void SetFanPower(Int32 channel, Int32 power);

        #endregion
    }
}