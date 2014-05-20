using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class PlugwiseTest
    {
        Plugwise plug;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Plugwise_Init()
        {
            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            plug = new Plugwise("Plug1", "Plug1", new List<Ball> { ball1 }, "http://1.1.1.1:8080_", "8080");
        }

        /// <summary>
        /// Testet die Methode Transmit.
        /// </summary>
        [Test]
        public void Plugwise_Transmit_invalid_Command()
        {
            Assert.AreEqual("ungueltiger Befehl", plug.Transmit("test", ""));
        }

        /// <summary>
        /// Testet die Methode Transmit.
        /// </summary>
        [Test]
        public void Plugwise_Transmit_valid_Command()
        {
            Assert.AreEqual("HTTP Verbindung fehlgeschlagen .\n Befehl konnte nicht ausgeführt werden.", plug.Transmit("on", ""));
        }
    }
}
