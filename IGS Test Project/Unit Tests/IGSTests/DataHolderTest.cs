using System;
using NUnit.Framework;
using IGS.Server.IGS;
using System.Collections.Generic;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;
using System.Net;

namespace IGS_Unit_Test_Project.IGS
{
    [TestFixture]
    public class DataHolderTest
    {
        DataHolder testDataHolder;
        List<Device> devices;
        List<User> users;
        Boxee testDevice1;
        Boxee testDevice2;
        User testUser1;
        User testUser2;
        IPAddress testIP;


        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Init()
        {
            devices = new List<Device>();
            testDataHolder = new DataHolder(devices);
            users = new List<User>();
            testIP = new IPAddress(new byte[] { 192, 168, 0, 17 });
            Ball ball1 = new Ball(new Vector3D(1, 1, 1), 1);
            testDevice1 = new Boxee("Boxee1", "Boxee1", new List<Ball> { ball1 }, "192.168.0.31", "8800");
            testDevice2 = new Boxee("Boxee2", "Boxee2", new List<Ball> { ball1 }, "192.168.0.99", "8800");
            testUser1 = new User("192.168.0.37");
            testUser2 = new User("192.168.0.38");
            testUser1.SkeletonId = 5;

            testDataHolder.Users.Add(testUser1);
            testDataHolder.Devices.Add(testDevice1);
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void DataHolder_set_get()
        {
            testDataHolder.Users = users;
            testDataHolder.LocalIp = testIP;
            testDataHolder.Devices = devices;
            Assert.AreEqual(devices, testDataHolder.Devices);
            Assert.AreEqual(users, testDataHolder.Users);
            Assert.AreEqual(testIP, testDataHolder.LocalIp);
        }

        /// <summary>
        /// Testet die AddUser Methode.
        /// </summary>
        [Test]
        public void DataHolder_AddUser_NotExisting()
        {
            Assert.AreEqual(true, testDataHolder.AddUser("192.168.0.38"));
            Assert.AreEqual(testDataHolder.GetUserByIp(testUser2.WlanAdr).WlanAdr, "192.168.0.38");
            Assert.AreEqual(2, testDataHolder.Users.Count);
        }

        /// <summary>
        /// Testet die AddUser Methode.
        /// </summary>
        [Test]
        public void DataHolder_AddUser_Existing()
        {
            Assert.AreEqual(false, testDataHolder.AddUser("192.168.0.37"));
            Assert.AreEqual(1, testDataHolder.Users.Count);
        }

        /// <summary>
        /// Testet die SetTrackedSkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_SetTrackedSkeleton_existingWlanAdr()
        {
            Assert.AreEqual(true, testDataHolder.SetTrackedSkeleton(testUser1.WlanAdr, 3));
            Assert.AreEqual(3, testUser1.SkeletonId);
        }

        /// <summary>
        /// Testet die SetTrackedSkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_SetTrackedSkeleton_NotExistingWlanAdr()
        {
            Assert.AreEqual(false, testDataHolder.SetTrackedSkeleton(testUser2.WlanAdr, 3));
            Assert.AreEqual(5, testUser1.SkeletonId);
        }

        /// <summary>
        /// Testet die DelUser Methode.
        /// </summary>
        [Test]
        public void Dataholder_DelUser_RightWlanAdr()
        {
            Assert.AreEqual(true, testDataHolder.DelUser(testUser1.WlanAdr));
            Assert.AreEqual(0, testDataHolder.Users.Count);
        }

        /// <summary>
        /// Testet die DelUser Methode.
        /// </summary>
        [Test]
        public void Dataholder_DelUser_Invalid_WlanAdr()
        {
            Assert.AreEqual(false, testDataHolder.DelUser(testUser2.WlanAdr));
        }

        /// <summary>
        /// Testet die GetUserBySkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetUserBySkeleton_existingSkeletonId()
        {
            Assert.AreEqual(testUser1, testDataHolder.GetUserBySkeleton(testUser1.SkeletonId));
        }

        /// <summary>
        /// Testet die GetUserBySkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetUserBySkeleton_NotExistingSkeletonId()
        {
            Assert.AreEqual(null, testDataHolder.GetUserBySkeleton(0));
        }

        /// <summary>
        /// Testet die GetUserByIp Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetUserByIp_ExistingIp()
        {
            Assert.AreEqual(testUser1, testDataHolder.GetUserByIp(testUser1.WlanAdr));
        }

        /// <summary>
        /// Testet die GetUserByIp Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetUserByIp_NotExistingIp()
        {
            Assert.AreEqual(null, testDataHolder.GetUserByIp("0.0.0.0"));
        }

        /// <summary>
        /// Testet die GetUserByIp Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetUserByIp_NotCompleteIp()
        {
            Assert.AreEqual(null, testDataHolder.GetUserByIp(""));
        }

        /// <summary>
        /// Testet die AddDevice Methode.
        /// </summary>
        [Test]
        public void DataHolder_AddDevice_regularDevice()
        {
            testDataHolder.AddDevice(testDevice2);

            Assert.AreEqual(true, testDataHolder.Devices.Contains(testDevice2));
            Assert.AreEqual(2, testDataHolder.Devices.Count);
        }

        /// <summary>
        /// Testet die AddDevice Methode.
        /// </summary>
        [Test]
        public void DataHolder_AddDevice_alreadyExistingDevice()
        {
            testDataHolder.AddDevice(testDevice1);

            Assert.AreEqual(1, testDataHolder.Devices.Count);
        }

        /// <summary>
        /// Testet die GetDevice Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetDevice_ExistingId()
        {
            Assert.AreEqual(testDevice1, testDataHolder.getDeviceByID(testDevice1.Id));
        }

        /// <summary>
        /// Testet die GetDevice Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetDevice_NotExistingId()
        {
            Assert.AreEqual(null, testDataHolder.getDeviceByID(testDevice2.Id));
        }

        /// <summary>
        /// Testet die GetDevice Methode.
        /// </summary>
        [Test]
        public void DataHolder_GetDevice_NullId()
        {
            Assert.AreEqual(null, testDataHolder.getDeviceByID(null));
        }

        /// <summary>
        /// Testet die DelTrackedSkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_DelTrackedSkeleton_existingId()
        {     
            Assert.AreEqual(true, testDataHolder.DelTrackedSkeleton(testUser1.SkeletonId));
            Assert.AreEqual(-1, testUser1.SkeletonId);
        }

        /// <summary>
        /// Testet die DelTrackedSkeleton Methode.
        /// </summary>
        [Test]
        public void DataHolder_DelTrackedSkeleton_NotExistingId()
        {
            Assert.AreEqual(false, testDataHolder.DelTrackedSkeleton(0));
        }
    }
}