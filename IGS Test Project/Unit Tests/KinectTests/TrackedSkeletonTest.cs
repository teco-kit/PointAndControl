using System;
using NUnit.Framework;
using IGS.Server.IGS;
using IGS.Server.Kinect;

namespace IGS_Unit_Test_Project.KinectTests
{
    [TestFixture]
    public class TrackedSkeletonTest
    {
        TrackedSkeleton testSkeleton1;
        TrackedSkeleton testSkeleton2;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void init()
        {
            testSkeleton1 = new TrackedSkeleton(5);
            testSkeleton2 = new TrackedSkeleton(2);
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void TrackedSkeleton_set_get()
        {
            testSkeleton1.Actions = 1;
            testSkeleton1.Id = 3;

            Assert.AreEqual(1, testSkeleton1.Actions);
            Assert.AreEqual(3, testSkeleton1.Id);      
        }

        /// <summary>
        /// Testet die Equals Methode
        /// </summary>
        [Test]
        public void TrackedSkeleton_Equals_sameId()
        {
            testSkeleton1.Id = 1;
            testSkeleton2.Id = 1;

            Assert.AreEqual(true, testSkeleton1.Equals(testSkeleton2));

        }

        /// <summary>
        /// Testet die Equals Methode
        /// </summary>
        [Test]
        public void TrackedSkeleton_Equals_differentId()
        {
            testSkeleton1.Id = 1;
            testSkeleton2.Id = 2;

            Assert.AreEqual(false, testSkeleton1.Equals(testSkeleton2));
        }
    }
}
