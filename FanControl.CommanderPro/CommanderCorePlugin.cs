using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class CommanderCorePlugin : IPlugin
    {
        private Core.CommanderCore CommanderCore;

        public String Name => "Corsair Commander CORE";

        public void Close()
        {
            CommanderCore.Disconnect();
        }

        public void Initialize()
        {
            CommanderCore = new Core.CommanderCore();

            CommanderCore.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            List<FanSensor> _fanSensors = new List<FanSensor>();
            List<ControlSensor> _controlSensors = new List<ControlSensor>();

            CommanderCore.GetFirmwareVersion();

            foreach (Int32 channel in CommanderCore.GetFanChannels())
            {
                _fanSensors.Add(new FanSensor { CommanderInstance = CommanderCore, Channel = channel });
            }

            //foreach (Int32 channel in CommanderCore.GetFanChannels())
            //{
            //    _controlSensors.Add(new ControlSensor { CommanderInstance = CommanderCore, Channel = channel });
            //}

            _container.FanSensors.AddRange(_fanSensors);
            //_container.ControlSensors.AddRange(_controlSensors);
        }
    }
}