using FanControl.Plugins;
using System;

namespace FanControl.CommanderPro
{
    public class ControlSensor : IPluginControlSensor
    {
        public CommanderPro CommanderProInstance { get; set; }

        public Int32 Channel { get; set; }

        public String Id => Channel.ToString();

        public String Name => $"Commander PRO Channel {Channel}";

        public Single? Value { get; set; }

        public void Reset()
        {
            CommanderProInstance.Connect();

            CommanderProInstance.SetFanPower(Channel, 50);
        }

        public void Set(Single val)
        {
            CommanderProInstance.Connect();

            CommanderProInstance.SetFanPower(Channel, Convert.ToInt32(val));
        }

        public void Update()
        {
            CommanderProInstance.Connect();

            Value = CommanderProInstance.GetFanPower(Channel);
        }
    }
}