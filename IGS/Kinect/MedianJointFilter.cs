using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Kinect
{
    public class MedianJointFilter : SkeletonJointFilter
    {
   

        //Median vector filterting Paper: Noise reduction by vector median filtering by Yike Liu
        public override Vector3D[] jointFilter(List<Vector3D[]> jointLists)
        {
           
            Vector3D[] filtered = new Vector3D[4];
            double minDist = 0;
            int indexOfMinDist = 0;
            double[] distArray = new double[jointLists.Count];
            List<double> distanceList = new List<double>();
         
            for (int jointMarker = 0; jointMarker < jointLists[1].Length; jointMarker++)
            {

                minDist = double.MaxValue;
                indexOfMinDist = 0;
                
                for (int i = 0; i < jointLists.Count; i++)
                {
                    for (int j = i+1; j <  jointLists.Count; j++)
                    {
                            double tmpDist = l2Norm(jointLists[i][jointMarker], jointLists[j][jointMarker]);
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
                distArray = new double[jointLists.Count];
                
            }


            return filtered;
        }

        public Double l2Norm(Vector3D vec1, Vector3D vec2)
        {
            return norm(vec1,  vec2, 2.0);
        }

        private Double norm(Vector3D vec1, Vector3D vec2, double p)
        {
            Double result = 0;
            double diffX = Math.Abs(vec1.X - vec2.X);
            double diffY = Math.Abs(vec1.Y - vec2.Y);
            double diffZ = Math.Abs(vec1.Z - vec2.Z);


            

            if (p == 2.0)
            {
                result = Math.Sqrt(Math.Pow(diffX, p) + Math.Pow(diffY, p) + Math.Pow(diffZ, p));
            }
            else
            {
                Double pow = 1.0/p;
                result = Math.Pow((Math.Pow(diffX, p) + Math.Pow(diffY, p) + Math.Pow(diffZ, p)), pow);
            }
            return result;
        }

    }
}
