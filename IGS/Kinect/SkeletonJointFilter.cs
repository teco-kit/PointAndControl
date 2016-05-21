using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace PointAndControl.Kinect
{
    interface ISkeletonJointFilter
    {
        Point3D[] jointFilter(List<Point3D[]> jointLists);
    }
}
