using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class Plugin : IPlugin
    {
        private CommanderPro CommanderPro;

        public string Name => "Corsair Commander PRO";

        public void Close()
        {
            CommanderPro.Disconnect();
        }

        public void Initialize()
        {
            CommanderPro = new CommanderPro();

            CommanderPro.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            List<FanSensor> _fanSensors = new List<FanSensor>();
            List<ControlSensor> _controlSensors = new List<ControlSensor>();

            foreach (Int32 channel in CommanderPro.GetFanChannels())
            {
                _fanSensors.Add(new FanSensor { CommanderProInstance = CommanderPro, Channel = channel });
            }

            foreach (Int32 channel in CommanderPro.GetFanChannels())
            {
                _controlSensors.Add(new ControlSensor { CommanderProInstance = CommanderPro, Channel = channel });
            }

            _container.FanSensors.AddRange(_fanSensors);
            _container.ControlSensors.AddRange(_controlSensors);
        }
    }
}