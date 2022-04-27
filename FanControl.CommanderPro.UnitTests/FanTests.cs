using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro.UnitTests
{
    [TestClass]
    public class FanTests
    {
        Pro.CommanderPro commanderPro;

        [TestInitialize]
        public void Init()
        {
            commanderPro = new Pro.CommanderPro();
            commanderPro.Connect();
        }

        [TestCleanup]
        public void CleanUp()
        {
            commanderPro.Disconnect();
        }

        [TestMethod]
        public void GetFirmwareVersion()
        {
            String firmwareVersion = commanderPro.GetFirmwareVersion();

            Assert.AreNotEqual(firmwareVersion, String.Empty);
        }

        [TestMethod]
        public void GetFanChannels()
        {
            List<Int32> fanChannels = commanderPro.GetFanChannels();

            Assert.IsNotNull(fanChannels);
        }

        [TestMethod]
        public void GetFanSpeeds()
        {
            Dictionary<Int32, Int32> fanSpeeds = commanderPro.GetFanSpeeds();

            Assert.IsNotNull(fanSpeeds);
        }

        [TestMethod]
        public void GetFanSpeed()
        {
            Int32 fanSpeed = commanderPro.GetFanSpeed(2);

            Assert.IsNotNull(fanSpeed);
        }

        [TestMethod]
        public void GetFanPower()
        {
            Int32 result = commanderPro.GetFanPower(0);

            Assert.AreEqual(50, result);
        }

        [TestMethod]
        public void SetFanSpeed()
        {
            commanderPro.SetFanSpeed(0, 1000);

            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void SetFanPower()
        {
            commanderPro.SetFanPower(0, 50);
            commanderPro.SetFanPower(1, 50);
            commanderPro.SetFanPower(2, 75);
            commanderPro.SetFanPower(3, 75);
            commanderPro.SetFanPower(4, 50);

            Assert.AreEqual(1, 1);
        }
    }
}