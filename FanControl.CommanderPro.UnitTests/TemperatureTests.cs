using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro.UnitTests
{
    [TestClass]
    public class TemperatureTests
    {
        CommanderPro commanderPro;

        [TestInitialize]
        public void Init()
        {
            commanderPro = new CommanderPro();
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