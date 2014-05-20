using System;
using NUnit.Framework;
using IGS.Server.Devices;

namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class TCPTest
    {
        Tcp tcp;

        /// <summary>
        /// Testet get, set.
        /// </summary>
        [Test]
        public void TCP_get_set()
        {
            tcp = new Tcp(1, "1.1.1.1");
            Assert.AreEqual("1.1.1.1", tcp.Ip);
            Assert.AreEqual(1, tcp.Port);
            tcp.Ip = "2.2.2.2";
            tcp.Port = 2;
            Assert.AreEqual("2.2.2.2", tcp.Ip);
            Assert.AreEqual(2, tcp.Port);
        }
    }
}
