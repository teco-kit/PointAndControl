using System;
using System.Threading;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;
using System.Collections.Generic;


namespace IGS_Integration_Test_Project.DevicesTests
{
    [TestFixture]
    public class BoxeeTest
    {
        Boxee box;
        private String ip = "";
        private String port = "";

        
        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Boxee_Init()
        {
            port = "8080";

            //hier die IP Adresse des TCP Dummy abgeben.
            ip = "192.168.2.106";

            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            box = new Boxee("Boxee1", "Boxee1", new List<Ball> { ball1 }, ip, port);
        }
        
        /// <summary>
        /// Testet gültige Eingaben
        /// Vorbedingungen:
        /// Ein Dummy eines Boxee Interface ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void Boxee_Transmit_valid_cmdId()
        {
            Assert.AreEqual("True", box.Transmit("volup", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("voldown", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("mute", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("left", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("right", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("down", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("up", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("select", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("select", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("select", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("select", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("pause", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("play", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("back", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("select", ""));
            Thread.Sleep(10);
            Assert.AreEqual("True", box.Transmit("s", ""));
            Thread.Sleep(10);
        }
    }
}
