using System;
using NUnit.Framework;
using IGS.Server.Devices;

namespace IGS_Integration_Test_Project.DevicesTests
{
    [TestFixture]
    public class TCPTest
    {
        Tcp tcp;
        private String ip = "";
        private int port;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// Bitte IP Adresse des TCP Dummy angeben.
        /// </summary>
        [SetUp]
        public void Init()
        {
            port = 15555;

            //hier die IP Adresse des TCP Dummy abgeben.
            ip = "192.168.2.106";

            tcp = new Tcp(port, ip);
        }

        /// <summary>
        /// Testet das Verhalten bei einem gültigen Host
        /// Vorbedingungen:
        /// Ein TCP-EchoServer ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void TCP_Send_valid_host()
        {
            Assert.IsTrue(tcp.Send("True\r").StartsWith("True"));
        }

        /// <summary>
        /// Testet das Verhalten bei einer ungültigen Ip-Adresse
        /// Vorbedingungen:
        /// Ein TCP-EchoServer ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void TCP_Send_invalid_ip()
        {
            tcp = new Tcp(port, "0.0.0.0");
            Assert.IsTrue(tcp.Send("True\r").StartsWith("TCP Verbindung fehlgeschlagen.\n Befehl konnte nicht ausgeführt werden."));
        }

        /// <summary>
        /// Testet das Verhalten bei einem ungültigen Port
        /// Vorbedingungen:
        /// Ein TCP-EchoServer ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void TCP_Send_invalid_port()
        {
            tcp = new Tcp(-1, ip);
            Assert.IsTrue(tcp.Send("True\r").StartsWith("TCP Verbindung fehlgeschlagen.\n Befehl konnte nicht ausgeführt werden."));
        }
    }
}
