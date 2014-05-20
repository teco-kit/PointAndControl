using System;
using NUnit.Framework;
using IGS.Server.Kinect;
using System.Collections.Generic;

namespace IGS_Unit_Test_Project.KinectTests
{
    [TestFixture]
    public class LFUTest
    {
        TrackedSkeleton testSkeleton1;
        TrackedSkeleton testSkeleton2;
        Lfu lfu;

        [SetUp]
        public void init()
        {
            lfu = new Lfu();
            testSkeleton1 = new TrackedSkeleton(1);
            testSkeleton2 = new TrackedSkeleton(2);
        }

        [Test]
        public void LFU_invalid_SkeletonCount() {
            List<TrackedSkeleton> list = new List<TrackedSkeleton> { };

            Assert.AreEqual(list, lfu.Replace(list));

            list = new List<TrackedSkeleton> { testSkeleton1};

            Assert.AreEqual(list, lfu.Replace(list));
        }

        [Test]
        public void LFU_valid_SkeletonCount_sameActionCount()
        {
            List<TrackedSkeleton> inList = new List<TrackedSkeleton> { testSkeleton1, testSkeleton2 };
            List<TrackedSkeleton> outlist = new List<TrackedSkeleton> { testSkeleton2 };

            Assert.AreEqual(outlist, lfu.Replace(inList));
        }

        [Test]
        public void LFU_valid_SkeletonCount_differentActionCount()
        {
            List<TrackedSkeleton> inList = new List<TrackedSkeleton> { testSkeleton1, testSkeleton2 };
            List<TrackedSkeleton> outlist = new List<TrackedSkeleton> { testSkeleton1 };

            testSkeleton1.Actions = testSkeleton1.Actions + 1;
            
            Assert.AreEqual(outlist, lfu.Replace(inList));
        }



    }
}
