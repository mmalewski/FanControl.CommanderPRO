using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            FanControl.CommanderPro.Core.CommanderCore commander = new FanControl.CommanderPro.Core.CommanderCore();

            commander.Connect();

            while (!Console.KeyAvailable)
            {
                Console.WriteLine($"Firmware: {commander.GetFirmwareVersion()}");

                List<Int32> channels = commander.GetFanChannels();

                Console.WriteLine("Connected fans:");

                foreach (Int32 channel in channels)
                {
                    Int32 speed = commander.GetFanSpeed(channel);

                    Console.WriteLine($"\tChannel {channel}: Speed: {speed}");
                }

                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

                System.Threading.Tasks.Task delay = System.Threading.Tasks.Task.Delay(pause);
                delay.Wait();
            }

            commander.Disconnect();

            Console.ReadLine();
        }
    }
}