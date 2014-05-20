using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Net.Sockets;
using System.Threading;

namespace IGS_Integration_Test_Project.DevicesTests
{
    [TestFixture]
    public class HTTPTest
    {
        Http http;
        private String ip = "";
        private int port;


        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void HTTP_Init()
        {
            port = 8080;

            //hier die IP Adresse des TCP Dummy abgeben.
            ip = "192.168.2.106";

            http = new Http(port, "http://" + ip);
        }

        /// <summary>
        /// Testet das Verhalten bei einem gültigen Command
        /// Vorbedingungen:
        /// 
        /// </summary>
        [Test]
        public void HTTP_Send_valid_command()
        {
            Thread.Sleep(10);
            Assert.AreNotEqual("HTTP Verbindung fehlgeschlagen .\n Befehl konnte nicht ausgeführt werden.", http.Send("http://" + ip + ":" + port));            
        }

        /// <summary>
        /// Testet das Verhalten bei einem ungültigen Command
        /// Vorbedingungen:
        /// 
        /// </summary>
        [Test]
        public void HTTP_Send_invalid_command()
        {
            Assert.AreEqual("HTTP Verbindung konnte nicht hergestellt werden.", http.Send("Test"));
            Thread.Sleep(10);
        }
    }
}
