using System;
using NUnit.Framework;
using IGS.Server.IGS;
using IGS.Server.Kinect;

namespace IGS_Unit_Test_Project.KinectTests
{
    [TestFixture]
    public class KinectUserEventArgsTest
    {
        KinectUserEventArgs testUserEventArgs;

        [SetUp]
        public void init() 
        {
            testUserEventArgs = new KinectUserEventArgs(4);
        }

        [Test]
        public void KinectUserEventArgs_set_get()
        {
            testUserEventArgs.SkeletonId = 1;
            Assert.AreEqual(1, testUserEventArgs.SkeletonId);
        }
    }
}
