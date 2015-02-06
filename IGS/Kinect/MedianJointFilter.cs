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
        //public override Vector3D[] jointFilter(List<Vector3D[]> jointLists)
        //{
        //    Vector3D[] smoothed = new Vector3D[4];
        //    smoothed[0] = new Vector3D();
        //    smoothed[1] = new Vector3D();

        //    List<double> workingListXShoulder = new List<double>();
        //    List<double> workingListYShoulder = new List<double>();
        //    List<double> workingListZShoulder = new List<double>();

        //    List<double> workingListXWrist = new List<double>();
        //    List<double> workingListYWrist = new List<double>();
        //    List<double> workingListZWrist = new List<double>();

        //    foreach (Vector3D[] vectors in jointLists)
        //    {
        //        workingListXShoulder.Add(vectors[2].X);
        //        workingListYShoulder.Add(vectors[2].Y);
        //        workingListZShoulder.Add(vectors[2].Z);

        //        workingListXWrist.Add(vectors[3].X);
        //        workingListYWrist.Add(vectors[3].Y);
        //        workingListZWrist.Add(vectors[3].Z);
        //    }

        //    workingListXShoulder.Sort();
        //    workingListYShoulder.Sort();
        //    workingListZShoulder.Sort();

        //    workingListXWrist.Sort();
        //    workingListYWrist.Sort();
        //    workingListZWrist.Sort();

        //    double quantil = 0.50;
        //    double median = jointLists.Count * quantil;

        //    if (median % 1 == 0)
        //    {
        //        Vector3D smoothedShoulder = new Vector3D(
        //                                                0.5 * (workingListXShoulder[(int)median - 1] + workingListXShoulder[(int)median]),
        //                                                0.5 * (workingListYShoulder[(int)median - 1] + workingListYShoulder[(int)median]),
        //                                                0.5 * (workingListZShoulder[(int)median - 1] + workingListZShoulder[(int)median])
        //                                                );
        //        Vector3D smoothedWrist = new Vector3D(
        //                                               0.5 * (workingListXWrist[(int)median - 1] + workingListXWrist[(int)median]),
        //                                               0.5 * (workingListYWrist[(int)median - 1] + workingListYWrist[(int)median]),
        //                                               0.5 * (workingListZWrist[(int)median - 1] + workingListZWrist[(int)median])
        //                                               );

        //        smoothed[2] = smoothedShoulder;
        //        smoothed[3] = smoothedWrist;
        //        return smoothed;
        //    }
        //    else if (median % 1 != 0)
        //    {
        //        int ceiled = (int)Math.Ceiling(median);
        //        Vector3D smoothedShoulder = new Vector3D(
        //                                                workingListXShoulder[ceiled - 1],
        //                                                workingListYShoulder[ceiled - 1],
        //                                                workingListZShoulder[ceiled - 1]
        //                                                );
        //        Vector3D smoothedWrist = new Vector3D(
        //                                                workingListXWrist[ceiled - 1],
        //                                                workingListYWrist[ceiled - 1],
        //                                                workingListZWrist[ceiled - 1]
        //                                                );
        //        smoothed[2] = smoothedShoulder;
        //        smoothed[3] = smoothedWrist;
        //        return smoothed;
        //    }

         

        //    return null;
        //}


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
