using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class Plugin : IPlugin
    {
        private CommanderPro CommanderPro;

        public string Name => "Corsair Commander Pro";

        public void Close()
        {
            System.IO.File.AppendAllText("trace.log", "Plugin.Close()" + Environment.NewLine);

            CommanderPro.Disconnect();
        }

        public void Initialize()
        {
            System.IO.File.AppendAllText("trace.log", "Plugin.Initialize()" + Environment.NewLine);

            CommanderPro = new CommanderPro();

            CommanderPro.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            System.IO.File.AppendAllText("trace.log", "Plugin.Load()" + Environment.NewLine);

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