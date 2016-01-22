using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Text;

namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class LCDTVTest {

        NecLcdMonitorMultiSyncV421 tv;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void LCDTV_Init()
        {
            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            tv = new NecLcdMonitorMultiSyncV421("tv", "tv1", new List<Ball> { ball1 }, "1.1.1.1", "1");
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void LCDTV_set_get()
        {
            Tcp con = new Tcp(tv.Connection.Port, tv.Connection.Ip);
            tv.Connection = con;
            Assert.AreEqual(con, tv.Connection);
        }

        /// <summary>
        /// Testet die Methode Transmit
        /// </summary>
        [Test]
        public void LCDTV_Transmit_invalid_Command()
        {
            Assert.AreEqual("ungueltiger Befehl", tv.Transmit("test", "test"));
        }

        /// <summary>
        /// Testet die Methode Transmit
        /// </summary>
        [Test]
        public void LCDTV_Transmit_empty_Command()
        {
            Assert.AreEqual("ungueltiger Befehl", tv.Transmit("", "test"));
        }

        /// <summary>
        /// Testet die Methode Transmit
        /// </summary>
        [Test]
        public void LCDTV_Transmit_valid_command()
        {
            Assert.AreEqual("TCP Verbindung fehlgeschlagen.\n Befehl konnte nicht ausgeführt werden.", tv.Transmit("source", "1"));
        }
    }
}
