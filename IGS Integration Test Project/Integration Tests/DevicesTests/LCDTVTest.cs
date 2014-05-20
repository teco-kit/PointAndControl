using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Text;

namespace IGS_Integration_Test_Project.DevicesTests
{
    [TestFixture]
    public class LCDTVTest {

        NECLCDmonitorMultiSyncV421 tv;
        private String ip = "";
        private String port = "";

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void LCDTV_Init()
        {
            port = "15555";

            //hier die IP Adresse des TCP Dummy abgeben.
            ip = "192.168.2.106";

            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            tv = new NECLCDmonitorMultiSyncV421("tv", "tv1", new List<Ball> { ball1 }, ip, port);
        }

        /// <summary>
        /// Testet das Verhalten bei PowerOn
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Power_On()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x41, 0x30, 0x43, 0x02, 0x43, 0x32, 0x30, 0x33, 0x44, 0x36, 0x30, 0x30, 0x30, 0x31, 0x03, 0x73, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("on", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei PowerOff
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Power_Off()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x41, 0x30, 0x43, 0x02, 0x43, 0x32, 0x30, 0x33, 0x44, 0x36, 0x30, 0x30, 0x30, 0x34, 0x03, 0x76, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("off", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei VolUp
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Volup()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x43, 0x30, 0x36, 0x02, 0x30, 0x30, 0x36, 0x32, 0x03, 0x01, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("volup", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei VolDown
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Voldown()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x43, 0x30, 0x36, 0x02, 0x30, 0x30, 0x36, 0x32, 0x03, 0x00, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("voldown", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei Mute
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Mute()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x43, 0x30, 0x36, 0x02, 0x30, 0x30, 0x38, 0x3D, 0x03, 0x00, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("mute", ""));
        }

        /// <summary>
        /// Testet das Verhalten bei Audio
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Audio()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x32, 0x32, 0x3E, 0x30, 0x30, 0x30, 0x31, 0x03, 0x00, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("audio", "1"));
        }

        /// <summary>
        /// Testet das Verhalten bei input
        /// Vorbedingungen:
        /// Ein Dummy eines NECLCDmonitorMultiSyncV421 ist an der angegebenen Adresse gestartet.
        /// </summary>
        [Test]
        public void LCDTV_Input()
        {
            byte[] bytes = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x30, 0x36, 0x30, 0x30, 0x30, 0x30, 0x31, 0x03, 0x08, 0x0D };
            String expected = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(expected, tv.Transmit("source", "1"));
        }
    }
}
