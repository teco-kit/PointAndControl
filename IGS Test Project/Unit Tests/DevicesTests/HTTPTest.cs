using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Net.Sockets;
using System.Threading;

namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class HTTPTest
    {
        Http http;

        /// <summary>
        /// Testet get, set.
        /// </summary>
        [Test]
        public void HTTP_get_set()
        {
            http = new Http(1, "http://1.1.1.1");
            Assert.AreEqual("http://1.1.1.1", http.Ip);
            Assert.AreEqual(1, http.Port);
            http.Ip = "http://2.2.2.2";
            http.Port = 2;
            Assert.AreEqual("http://2.2.2.2", http.Ip);
            Assert.AreEqual(2, http.Port);
        }
    }
}
