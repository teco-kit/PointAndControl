using System;
using NUnit.Framework;
using IGS.Server.IGS;
using System.Collections.Generic;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using IGS.Server.WebServer;
using System.Net;
using System.Xml;
using System.Windows.Media.Media3D;
using System.Net.Sockets;
using System.IO;


namespace IGS_Integration_Test_Project.IGS
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
            createConfiguration();
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

        private void createConfiguration()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\deviceConfiguration.xml"))
            {
                return;
            }
            var docConfig = new XmlDocument();
            XmlElement rootElement = docConfig.CreateElement("deviceConfiguration");
            docConfig.AppendChild(rootElement);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\deviceConfiguration.xml");
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

      

        [Test]
        public void Igs_AddUser_valid()
        {
           Assert.AreEqual(true, testIgs.AddUser("192.168.1.1"));
        }

           [Test]
        public void Igs_DelUser_valid()
        {
            testIgs.Data.AddUser("192.168.1.1");
            Assert.AreEqual(true, testIgs.DelUser("192.168.1.1"));
        }

        [Test]
        public void Igs_DelUser_noIp()
        {
            testIgs.Data.AddUser("192.168.1.1");
            Assert.AreEqual (false, testIgs.DelUser(""));
        }

        [Test]
        public void Igs_DelUser_invalidIp()
        {
            testIgs.Data.AddUser("192.168.1.1");
            Assert.AreEqual(false, testIgs.DelUser("192.167.3"));
        }

        [Test]
        public void Igs_DelUser_noUserToDelete()
        {
            Assert.AreEqual(false, testIgs.DelUser("192.168.1.1"));
        }

        [Test]
        public void Igs_AddDevice()
        {
            String[] param;
            param = new String[4];
            param[0] = "Boxee";
            param[1] = "BoxFlo";
            param[2] = "192.168.0.123";
            param[3] = "8800";
            Assert.AreEqual("Device added to deviceConfiguration.xml and devices list", testIgs.AddDevice(param));
            param[1] = "BoxChris";
            Assert.AreEqual("Device added to deviceConfiguration.xml and devices list", testIgs.AddDevice(param));
            param[0] = "Crap";
            Assert.AreEqual("Device added to deviceConfiguration but not to devices list", testIgs.AddDevice(param));
        }

        [Test]
        public void Igs_InterpretCommand_addUser()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "addUser";
            testArgs.ClientIp = "192.168.0.1";
            Assert.AreEqual("True", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_close()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "close";
            testArgs.ClientIp = "192.168.0.1";
            testIgs.AddUser("192.168.0.1");
            Assert.AreEqual("True", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_activateGestureCtrl_UserExisting()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "activateGestureCtrl";
            testArgs.ClientIp = "192.168.0.2";
            testIgs.Data.AddUser("192.168.0.2");
            //missing functional tracker
            Assert.AreEqual("False", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_activateGestureCtrl_UserNotExisting()
        {
            testArgs.Dev = "server";
            testArgs.ClientIp = "0.0.0.0";
            testArgs.Cmd = "activateGestureCtrl";
            testIgs.Data.AddUser("192.168.0.2");
            Assert.AreEqual("Aktivierung nicht möglich.\nBitte starten Sie die App neu.", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_list()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "list";
            Assert.AreEqual("Boxee1\tBoxee1\n", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_popUp_NotExistingUser()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "popup";
            testArgs.ClientIp = "0.0.0.0";
            Assert.AreEqual("", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_popUp_ExistingUser()
        {
            testArgs.Dev = "server";
            testArgs.Cmd = "popup";

            testIgs.Data.AddUser("192.168.0.3");
            testArgs.ClientIp = "192.168.0.3";
            testIgs.Data.GetUserByIp("192.168.0.3").AddError("blabla");
            Assert.AreEqual("blabla\n", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_addDeviceCoord_nullValue()
        {
            testArgs.Dev = "";
            testArgs.Cmd = "addDeviceCoord";
            testArgs.Val = null;
            testArgs.ClientIp = "192.168.0.3";
            testIgs.Data.AddUser("192.168.0.3");

            Assert.AreEqual("keine Koordinaten hinzugefügt,\nRadius fehlt oder hat falsches Format", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_addDeviceCoord_noValue()
        {
            testArgs.Dev = "";
            testArgs.Cmd = "addDeviceCoord";
            testArgs.Val = "";
            testArgs.ClientIp = "192.168.0.3";
            testIgs.Data.AddUser("192.168.0.3");

            Assert.AreEqual("keine Koordinaten hinzugefügt,\nRadius fehlt oder hat falsches Format", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_addDeviceCoord_ValidValue()
        {
            testArgs.Dev = "";
            testArgs.Cmd = "addDeviceCoord";
            testArgs.Val = "3";
            testArgs.ClientIp = "192.168.0.3";
            testIgs.Data.AddUser("192.168.0.3");

            //fehlender Eintrag in XML testfile -> kann knoten nicht finden
            Assert.AreEqual("keine Koordinaten hinzugefügt", testIgs.InterpretCommand(new Object(), testArgs));
        }

        [Test]
        public void Igs_InterpretCommand_validDevId_invalidCmdId()
        {
            testArgs.Dev = "Boxee1";
            testArgs.Cmd = "volUp";
            testArgs.Val = "0";
            testArgs.ClientIp = "192.168.0.3";
            
           
            testArgs.P = new HttpProcessor(testTcpClient, testServer);
            Assert.AreEqual("ungueltiger Befehl", testIgs.InterpretCommand(new Object(), testArgs));

        }

        //[Test]
        public void Igs_InterpretCommand_invalidDevIdCmdId()
        {
            testArgs.Dev = "blabla";
            testArgs.Cmd = "blabla";
            testArgs.Val = "3";
            testArgs.P = new HttpProcessor(testTcpClient, testServer);

            Assert.AreEqual(null, testIgs.InterpretCommand(new Object(), testArgs));
        }
    }
}