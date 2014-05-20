using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace IGS_Integration_Test_Project.DevicesTests
{
    [TestFixture]
    public class PlugwiseTest
    {
        Plugwise plug;
        private String ip = "";
        private String port = "";


        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Plugwise_Init()
        {
            port = "8080";

            //hier die IP Adresse des TCP Dummy abgeben.
            ip = "192.168.2.106";

            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            plug = new Plugwise("Plug1", "Plug1", new List<Ball> { ball1 }, "http://" + ip + ":" + port + "_", port);
        }

        /// <summary>
        /// Testet das Verhalten On
        /// Vorbedingungen:
        /// Ein Dummy eines Plugwise ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void Plugwise_On()
        {
            Assert.IsTrue(plug.Transmit("on", "").StartsWith("<html>\n<li>OK</html>\n"));
        }

        /// <summary>
        /// Testet das Verhalten Off
        /// Vorbedingungen:
        /// Ein Dummy eines Plugwise ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void Plugwise_Off()
        {
            Assert.IsTrue(plug.Transmit("off", "").StartsWith("<html>\n<li>OK</html>\n"));
        }
    }
}
