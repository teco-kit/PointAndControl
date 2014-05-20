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
    public class devKinect
    {
        public devKinect(String name, Ball ball, double tiltingDegree, double roomOrientation)
        {
            this.name = name;
            this.ball = ball;
            this.tiltingDegree = tiltingDegree;
            this.roomOrientation = roomOrientation;
        }

        public Ball ball { get; set; }
        public String name { get; set; }
       
        public double roomOrientation { get; set; }
        public double tiltingDegree { get; set; }


    }
}
