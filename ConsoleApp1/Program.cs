using FanControl.CommanderPro;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            CommanderCore commander = new CommanderCore();

            commander.Connect();

            String firmwareVersion = commander.GetFirmwareVersion();

            //List<Int32> channels = commander.GetFanChannels();

            Console.WriteLine($"Firmware version: {firmwareVersion}");

            //foreach (var item in channels)
            //{
            //    Console.WriteLine($"Fan connected on channel: {item}");
            //}

            Console.ReadLine();
        }
    }
}