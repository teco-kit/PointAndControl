using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Diagnostics;
using System;

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
            if (trackedSkeletons == null)
                return trackedSkeletons;

            int i = 0;
           
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
                    
                    BodyFrame f = reader.AcquireLatestFrame();
                  
                    i++;
                    if (f != null)
                    {
                    
                        f.GetAndRefreshBodyData(tempSkeletons);
                        foreach (Body tracked in tempSkeletons.Where(tracked => tracked.TrackingId == s.TrackingId &&
                                                                                    tracked.IsTracked))
                        {
                            quit = true;
                            f.Dispose();
                        }
                    }
                }

                // checks if an arm is raised
                foreach (Body skeleton in tempSkeletons)
                {
                    if (skeleton.TrackingId == s.TrackingId && skeleton.IsTracked)
                    {
                        // check left hand
                        if ((skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.WristLeft].Position.Y) &&
                            (skeleton.Joints[JointType.ShoulderLeft].Position.Y < skeleton.Joints[JointType.ElbowLeft].Position.Y))
                        {
                            trackedSkeletons.Add(new TrackedSkeleton((int)skeleton.TrackingId, false));
                        }

                        // check right hand
                        if ((skeleton.Joints[JointType.Head].Position.Y < skeleton.Joints[JointType.WristRight].Position.Y) &&
                            (skeleton.Joints[JointType.ShoulderRight].Position.Y < skeleton.Joints[JointType.ElbowRight].Position.Y))
                        {
                            trackedSkeletons.Add(new TrackedSkeleton((int)skeleton.TrackingId, true));
                        }
                    }
                }
              
            }
           
            return trackedSkeletons;
            
        }
    }

}