using IGS.Helperclasses;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Kinect
{
    public class MedianJointFilter : ISkeletonJointFilter
    {
   

        //Median vector filterting Paper: Noise reduction by vector median filtering by Yike Liu
        public Point3D[] jointFilter(List<Point3D[]> jointLists)
        {
            int listLength = jointLists.Count;

            if (listLength <= 0)
                return null;

            Point3D[] filtered = new Point3D[jointLists[0].Length];
            double minDist = 0;
            int indexOfMinDist = 0;
            double[] distArray;
            List<double> distanceList = new List<double>();
         
            for (int jointMarker = 0; jointMarker < jointLists[0].Length; jointMarker++)
            {
                distArray = new double[listLength]; 

                minDist = double.MaxValue;
                indexOfMinDist = 0;

                for (int i = 0; i < listLength; i++)
                {
                    for (int j = i + 1; j < listLength; j++)
                    {
                        double tmpDist = igsMath.l2Norm(jointLists[i][jointMarker] - jointLists[j][jointMarker]);
                        distArray[i] += tmpDist;
                        distArray[j] += tmpDist;
                    }

                    if (minDist > distArray[i])
                    {
                        minDist = distArray[i];
                        indexOfMinDist = i;
                    }
                }
               
                filtered[jointMarker] = jointLists[indexOfMinDist][jointMarker];           
            }
            return filtered;
        }

      

    }
}
