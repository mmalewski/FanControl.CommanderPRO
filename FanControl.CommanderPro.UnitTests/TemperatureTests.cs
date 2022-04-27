using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro.UnitTests
{
    [TestClass]
    public class TemperatureTests
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
        public void GetTemperatures()
        {
            Dictionary<Int32, Int32> temperatures = commanderPro.GetTemperatures();

            Assert.IsNotNull(temperatures);
        }
    }
}