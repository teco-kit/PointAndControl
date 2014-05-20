using System;
using NUnit.Framework;
using IGS.Server.IGS;

namespace IGS_Unit_Test_Project.IGS
{
    [TestFixture]
    public class UserTest
    {
        User testUser;

        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void init()
        {
            testUser = new User("192.168.0.1");
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void User_set_get()
        {
            testUser.SkeletonId = 1;
            testUser.WlanAdr = "192.168.0.1";

            Assert.AreEqual(1, testUser.SkeletonId);
            Assert.AreEqual("192.168.0.1", testUser.WlanAdr);
            Assert.AreEqual("", testUser.Errors);
        }

        /// <summary>
        /// Testet die AddError Methode.
        /// </summary>
        [Test]
        public void User_AddError_NotEmpty()
        {
            testUser.AddError("bla");

            Assert.AreEqual("bla\n", testUser.Errors);
        }

        /// <summary>
        /// Testet die AddError Methode.
        /// </summary>
        [Test]
        public void User_AddError_Empty()
        {
            testUser.AddError("");

            Assert.AreEqual("\n", testUser.Errors);
        }

        /// <summary>
        /// Testet die ClearErrors Methode.
        /// </summary>
        [Test]
        public void User_ClearErrors()
        {
            testUser.AddError("bla");
            testUser.ClearErrors();

            Assert.AreEqual("", testUser.Errors);

        }
    }
}