using System.Windows.Media.Media3D;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     The class ball presents a ballform, used to represent devices (their coordinates and radius in the room) as spheres and for the 
    ///     computation of collisions for the gesture control.
    ///     @author Florian Kinn
    /// </summary>
    public class Ball
    {
        /// <summary>
        ///     Constructor of the ball class
        ///     <param name="center">Vector to the center of the sphere</param>
        ///     <param name="radius">Raidus of the sphere</param>
        /// </summary>
        public Ball(Point3D center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        ///     Vector to the center of the ball
        ///     With the "set"-method the vector to the center of the ball can be set.
        ///     With the "get"-method the vector to the center of the ball can be returned.
        ///     <returns>Returns the vector to the center</returns>
        /// </summary>
        public Point3D Center { get; set; }

        /// <summary>
        ///     The radius of the ball.
        ///     With the "set"-method the radius of the ball can be set.
        ///     With the "get"-method the radius of the ball can be returned.
        ///     <returns>Returns the radius of the the ball</returns>
        /// </summary>
        public float Radius { get; set; }
    }
}