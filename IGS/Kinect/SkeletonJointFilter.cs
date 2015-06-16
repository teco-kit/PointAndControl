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
        Vector3D[] jointFilter(List<Vector3D[]> jointLists);
    }
}
