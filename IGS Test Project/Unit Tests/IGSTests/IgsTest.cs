using System;
using NUnit.Framework;
using IGS.Server.IGS;
using System.Collections.Generic;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using IGS.Server.WebServer;
using System.Net;
using System.Windows.Media.Media3D;
using System.Net.Sockets;


namespace IGS_Unit_Test_Project.IGS
{
    [TestFixture]
    public class IgsTest
    {
        Igs testIgs;
        DataHolder testDataHolder;
        UserTracker testTracker;
        HttpServer testServer;
        TcpClient testTcpClient;
        HttpEventArgs testArgs;
        Device testDevice1;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void init()
        {
            testDataHolder = new DataHolder(new List<Device>());
            testTracker = new UserTracker(new HandsUp(), new Lfu());
            testServer = new MyHttpServer(8080, new IPAddress(new byte[] { 192, 168, 2, 106}));
            testIgs = new Igs(testDataHolder, testTracker, testServer);
            testTcpClient = new TcpClient();
            testArgs = new HttpEventArgs("192.168.0.1", "", "", "", new HttpProcessor(testTcpClient, testServer));
            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            testDevice1 = new Boxee("Boxee1", "Boxee1", new List<Ball> { ball1 }, "192.168.0.31", "8800");
            testIgs.Data.AddDevice(testDevice1);
        }

        /// <summary>
        /// Setzt die Testklasse zurück.
        /// </summary>
        [TearDown]
        public void reset()
        {
            testIgs = null;
            testDataHolder = null;
            testTracker = null;
            testServer = null;
            testTcpClient = null;
            testArgs = null;
            testDevice1 = null;
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void igs_set_get()
        {
            Assert.AreEqual(testDataHolder, testIgs.Data);
            Assert.AreEqual(testTracker, testIgs.Tracker);
            Assert.AreEqual(testServer, testIgs.Server);
        }
    }
}