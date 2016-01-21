using System;
using NUnit.Framework;
using IGS.Server.Devices;
using System.Windows.Media.Media3D;

namespace IGS_Unit_Test_Project.DevicesTests
{
    [TestFixture]
    public class BallTest
    {
        Ball ball;
        
        /// <summary>
        /// Initialisiert die Testklasse.
        /// </summary>
        [SetUp]
        public void Init()
        {
            ball = new Ball(new Vector3D(1, 1, 1), 1);
        }

        /// <summary>
        /// Testet die Getter und Setter
        /// </summary>
        [Test]
        public void Ball_set_get()
        {
            Vector3D vec = new Vector3D(1,1,1);
            ball.Center = vec;
            Assert.AreEqual(vec, ball.Center);

            float rad = 1;
            ball.Radius = rad;
            Assert.AreEqual(rad, ball.Radius);
        }
    }
}
