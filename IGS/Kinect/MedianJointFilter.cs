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
        public override Vector3D[] jointFilter(List<Vector3D[]> jointLists)
        {
            Vector3D[] smoothed = new Vector3D[4];
            smoothed[0] = new Vector3D();
            smoothed[1] = new Vector3D();

            List<double> workingListXShoulder = new List<double>();
            List<double> workingListYShoulder = new List<double>();
            List<double> workingListZShoulder = new List<double>();

            List<double> workingListXWrist = new List<double>();
            List<double> workingListYWrist = new List<double>();
            List<double> workingListZWrist = new List<double>();

            foreach (Vector3D[] vectors in jointLists)
            {
                workingListXShoulder.Add(vectors[2].X);
                workingListYShoulder.Add(vectors[2].Y);
                workingListZShoulder.Add(vectors[2].Z);

                workingListXWrist.Add(vectors[3].X);
                workingListYWrist.Add(vectors[3].Y);
                workingListZWrist.Add(vectors[3].Z);
            }

            workingListXShoulder.Sort();
            workingListYShoulder.Sort();
            workingListZShoulder.Sort();

            workingListXWrist.Sort();
            workingListYWrist.Sort();
            workingListZWrist.Sort();

            double quantil = 0.50;
            double median = jointLists.Count * quantil;

            if (median % 1 == 0)
            {
                Vector3D smoothedShoulder = new Vector3D(
                                                        0.5 * (workingListXShoulder[(int)median - 1] + workingListXShoulder[(int)median]),
                                                        0.5 * (workingListYShoulder[(int)median - 1] + workingListYShoulder[(int)median]),
                                                        0.5 * (workingListZShoulder[(int)median - 1] + workingListZShoulder[(int)median])
                                                        );
                Vector3D smoothedWrist = new Vector3D(
                                                       0.5 * (workingListXWrist[(int)median - 1] + workingListXWrist[(int)median]),
                                                       0.5 * (workingListYWrist[(int)median - 1] + workingListYWrist[(int)median]),
                                                       0.5 * (workingListZWrist[(int)median - 1] + workingListZWrist[(int)median])
                                                       );

                smoothed[2] = smoothedShoulder;
                smoothed[3] = smoothedWrist;
                return smoothed;
            }
            else if (median % 1 != 0)
            {
                int ceiled = (int)Math.Ceiling(median);
                Vector3D smoothedShoulder = new Vector3D(
                                                        workingListXShoulder[ceiled - 1],
                                                        workingListYShoulder[ceiled - 1],
                                                        workingListZShoulder[ceiled - 1]
                                                        );
                Vector3D smoothedWrist = new Vector3D(
                                                        workingListXWrist[ceiled - 1],
                                                        workingListYWrist[ceiled - 1],
                                                        workingListZWrist[ceiled - 1]
                                                        );
                smoothed[2] = smoothedShoulder;
                smoothed[3] = smoothedWrist;
                return smoothed;
            }

         

            return null;
        }
    }
}
