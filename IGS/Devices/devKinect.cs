using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGS.Server.Kinect;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace IGS.Server.Devices
{
    /// <summary>
    /// This class represents a kinect camera with a name, a place in the room, its tilting angle and its horizontal view(roomOrientation).
    /// </summary>
    public class DevKinect
    {
        /// <summary>
        /// Constructor of the kinect representation
        /// </summary>
        /// <param name="name">name of the camera</param>
        /// <param name="ball">place of the camera</param>
        /// <param name="tiltingDegree">degree of tilting of the camera</param>
        /// <param name="roomOrientation">the degree of horizontal orientation of the camera</param>
        public DevKinect(String name, Ball ball, double tiltingDegree, double roomOrientation)
        {
            this.name = name;
            this.ball = ball;
            this.tiltingDegree = tiltingDegree;
            this.roomOrientation = roomOrientation;
        }

        /// <summary>
        ///     The shape and place of the camera.
        ///     With the "set"-method the shape and place can be set.
        ///     With the "get"-method the shape and place can be returned.
        ///     <returns>Returns the shape and place</returns>
        /// </summary>
        public Ball ball { get; set; }

        ///     The name of the camera.
        ///     With the "set"-method the name can be set.
        ///     With the "get"-method the name can be returned.
        ///     <returns>Returns the name</returns>
        public String name { get; set; }

        ///     The horizontal orientiation in degree of the camera
        ///     With the "set"-method the horizontal orientation can be set.
        ///     With the "get"-method the horizontal orientation can be returned.
        ///     <returns>Returns the horizontal orientation</returns>
        public double roomOrientation { get; set; }

        ///     The degree of the tilting of the camera
        ///     With the "set"-method the tilting degree can be set.
        ///     With the "get"-method the tilting degree can be returned.
        ///     <returns>Returns the tilting degree</returns>
        public double tiltingDegree { get; set; }

        public bool setKinectCoords(Point3D center)
        {
            if(center!= null)
            {
                ball.Center = center;
                return true;
            } else
            {
                return false;
            }
        }

    }
}
