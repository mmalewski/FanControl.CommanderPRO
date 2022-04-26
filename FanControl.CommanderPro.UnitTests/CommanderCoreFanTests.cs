using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro.UnitTests
{
    [TestClass]
    public class CommanderCoreFanTests
    {
        CommanderCore commander;

        [TestInitialize]
        public void Init()
        {
            commander = new CommanderCore();
            commander.Connect();
        }

        [TestCleanup]
        public void CleanUp()
        {
            commander.Disconnect();
        }

        [TestMethod]
        public void GetFirmwareVersion()
        {
            String result = commander.GetFirmwareVersion();

            Assert.AreNotEqual(result, String.Empty);
            Assert.AreEqual("2.10.219", result);
        }

        [TestMethod]
        public void GetFanChannels()
        {
            List<Int32> result = commander.GetFanChannels();

            Assert.IsTrue(result.All(x => x > 0));
        }

        [TestMethod]
        public void GetFanChannelsMultipleTimes()
        {
            List<Int32> result;

            for (Int32 i = 0; i < 50; i++)
            {
                result = commander.GetFanChannels();

                Assert.IsTrue(result.All(x => x > 0));
            }
        }

        [TestMethod]
        public void GetFanSpeed()
        {
            Int32 result = commander.GetFanSpeed(1);

            Assert.AreNotEqual(0, result);
        }
    }
}