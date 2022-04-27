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
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                Console.WriteLine($"{DateTime.UtcNow} - Getting fan data:");

                List<Int32> channels = commander.GetFanChannels();

                foreach (var item in channels)
                {
                    Console.WriteLine($"Fan connected on channel: {item}");
                }

                sw.Stop();

                Console.WriteLine($"Fan data took: {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}