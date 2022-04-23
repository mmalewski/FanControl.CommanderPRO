using FanControl.Plugins;
using System;

namespace FanControl.CommanderPro
{
    public class FanSensor : IPluginSensor
    {
        public CommanderPro CommanderProInstance { get; set; }

        public Int32 Channel { get; set; }

        public String Id => Channel.ToString();

        public String Name => $"Commander PRO Channel {Channel}";

        public Single? Value { get; set; }

        public void Update()
        {
            CommanderProInstance.Connect();

            Value = CommanderProInstance.GetFanSpeed(Channel);
        }
    }
}