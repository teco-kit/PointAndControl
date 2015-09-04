using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Kinect
{
    interface ISkeletonJointFilter
    {
        Point3D[] jointFilter(List<Point3D[]> jointLists);
    }
}
