using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            FanControl.CommanderPro.Core.CommanderCore commander = new FanControl.CommanderPro.Core.CommanderCore();

            commander.Connect();

            Boolean exitRequested = false;

            String firmware;

            while (!exitRequested)
            {
                firmware = commander.GetFirmwareVersion();

                if (String.Equals(firmware, "0.0.0"))
                {
                    Console.WriteLine("Bad firmware data!");
                }
                else
                {
                    Console.WriteLine($"Firmware v{firmware}");

                    List<Int32> channels = commander.GetFanChannels();

                    foreach (Int32 channel in channels)
                    {
                        Int32 speed = commander.GetFanSpeed(channel);

                        Console.WriteLine($"\tFan on channel {channel} speed: {speed}");
                    }
                }

                if (Console.KeyAvailable)
                {
                    exitRequested = true;
                }

                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

                Task delay = Task.Delay(pause);
                delay.Wait();
            }

            commander.Disconnect();

            Console.ReadLine();
        }
    }
}