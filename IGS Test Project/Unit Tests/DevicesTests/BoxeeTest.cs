using System;
using System.Threading;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;
using System.Collections.Generic;


namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class BoxeeTest
    {
        Boxee box;
        
        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Boxee_Init()
        {
            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            box = new Boxee("Boxee1", "Boxee1", new List<Ball> { ball1 }, "1.1.1.1", "8080");
        }
        
        /// <summary>
        /// Testet gültige Eingaben
        /// </summary>
        [Test]
        public void Boxee_Transmit_valid_cmdId()
        {
            Assert.AreEqual("HTTP Verbindung fehlgeschlagen .\n Befehl konnte nicht ausgeführt werden.", box.Transmit("select", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei einer ungültigen Command-Id
        /// </summary>
        [Test]
        public void Boxee_Transmit_invalid_cmdId()
        {
            Assert.AreEqual("ungueltiger Befehl", box.Transmit("selects", ""));
            Thread.Sleep(10);
        }

        /// <summary>
        /// Testet das Verhalten bei einer leeren Command-Id
        /// </summary>
        [Test]
        public void Boxee_Transmit_empty_cmdId()
        {
            Assert.AreEqual("ungueltiger Befehl", box.Transmit("", ""));
            Thread.Sleep(10);
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void Boxee_set_get()
        {
            Http con = new Http(box.Connection.Port, box.Connection.Ip);
            box.Connection = con;
            Assert.AreEqual(con, box.Connection);
        }
    }
}
