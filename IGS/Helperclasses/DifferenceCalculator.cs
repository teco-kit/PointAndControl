using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Helperclasses
{
    static class DifferenceCalculator
    {
        static public Vector3D[] calulateVecArray(Body b)
        {
            
           
            Vector3D[] result = new Vector3D[4];
            result[0] = new Vector3D(b.Joints[JointType.ShoulderLeft].Position.X,
                                     b.Joints[JointType.ShoulderLeft].Position.Y,
                                     b.Joints[JointType.ShoulderLeft].Position.Z);
            result[1] = new Vector3D(b.Joints[JointType.WristLeft].Position.X,
                                     b.Joints[JointType.WristLeft].Position.Y,
                                     b.Joints[JointType.WristLeft].Position.Z);
            result[2] = new Vector3D(b.Joints[JointType.ShoulderRight].Position.X,
                                     b.Joints[JointType.ShoulderRight].Position.Y,
                                     b.Joints[JointType.ShoulderRight].Position.Z);
            result[3] = new Vector3D(b.Joints[JointType.WristRight].Position.X,
                                     b.Joints[JointType.WristRight].Position.Y,
                                     b.Joints[JointType.WristRight].Position.Z);
            return result;
        }
        static public void calculateBodyDifference(Vector3D[] smoothed, Vector3D[] nonsmoothed)
        {
            double differenceRightWrist = 0;
            double differenceRightShoulder = 0;
            double differenceLeftWrist = 0;
            double differenceLeftShoulder = 0;



            differenceLeftShoulder = igsMath.l2Norm(smoothed[0], nonsmoothed[0]);
            differenceLeftWrist = igsMath.l2Norm(smoothed[1], nonsmoothed[1]);
            differenceRightShoulder = igsMath.l2Norm(smoothed[2], nonsmoothed[2]);
            differenceRightWrist = igsMath.l2Norm(smoothed[3], nonsmoothed[3]);

            XMLComponentHandler.writeDifferencesPerSelect(differenceLeftShoulder, differenceLeftWrist, differenceRightShoulder, differenceRightWrist);

        }
    }
}
