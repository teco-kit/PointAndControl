using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Diagnostics;

namespace IGS.Server.Kinect
{
    /// <summary>
    ///     The class HandsUp implements a concrete gesture.
    ///     This class describes the gesture of raising a hand to activate the gesture control. 
    ///     @author Sven Ochs
    /// </summary>
    public class HandsUp : GestureStrategy
    {
        /// <summary>
        ///     This method returns the skeletons which will be tracked. It adds the skeleton which raises the hand (hand above head and ellbow above shoulder) to the given list. 
        ///     <param name="bodies">Skeletons which will be checked if they raise a hand.</param>
        ///     <param name="trackedSkeletons">currently tracked skeletons</param>
        ///     <param name="id">id which is meant to be (re-)activated</param>
        ///     <param name="sensor">KinectSensor</param>
        ///     <returns>Returns the tracked skeletons</returns>
        /// </summary>
        public override List<TrackedSkeleton> Filter(Body[] bodies, List<TrackedSkeleton> trackedSkeletons, int id, BodyFrameReader reader)
        {
            List<Body> handsUpSkeletons = new List<Body>();

            foreach (Body t in bodies.Where(t => (int)t.TrackingId == id))
            {
                trackedSkeletons.Add(new TrackedSkeleton((int)t.TrackingId));
                return trackedSkeletons;
            }
            foreach (Body s in bodies.Where(s => s.TrackingId != 0))
            {
                Debug.WriteLine(s.TrackingId);
                Body[] tempSkeletons = new Body[6];
                bool quit = false;
                while (!quit)
                {
                    reader.AcquireLatestFrame().GetAndRefreshBodyData(tempSkeletons);

                    foreach (Body tracked in tempSkeletons.Where(tracked => tracked.TrackingId == s.TrackingId &&
                                                                                tracked.IsTracked))
                    {
                        quit = true;
                    }
                }
                //checks if an arm is raised
                handsUpSkeletons.AddRange(from tempS in tempSkeletons
                                          where tempS.TrackingId == s.TrackingId && tempS.IsTracked
                                          let head = tempS.Joints[JointType.Head]
                                          let wristL = tempS.Joints[JointType.WristLeft]
                                          let elbowL = tempS.Joints[JointType.ElbowLeft]
                                          let shoulderL = tempS.Joints[JointType.ShoulderLeft]
                                          let wristR = tempS.Joints[JointType.WristRight]
                                          let elbowR = tempS.Joints[JointType.ElbowRight]
                                          let shoulderR = tempS.Joints[JointType.ShoulderRight]
                                          where (head.Position.Y < wristL.Position.Y && shoulderL.Position.Y < elbowL.Position.Y) || (head.Position.Y < wristR.Position.Y && shoulderR.Position.Y < elbowR.Position.Y)
                                          select s);
            }
            if (trackedSkeletons != null && handsUpSkeletons.Count == 1)
            {
                trackedSkeletons.Add(new TrackedSkeleton((int)handsUpSkeletons[0].TrackingId));
            }
            return trackedSkeletons;
        }
    }
}