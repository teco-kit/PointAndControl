using System.Collections.Generic;
using Microsoft.Kinect;

namespace PointAndControl.Kinect
{
    /// <summary>
    ///     This abstract class defines a gesture. 
    ///     This class describes a gesture a person can activate the gesture control with. 
    ///     It adds the skeleton performing the defined gesture to the given list.
    ///     @author Sven Ochs
    /// </summary>
    public abstract class GestureStrategy
    {
        /// <summary>
        ///     This method returns the skeleton which is chosen to be tracked. It adds the skeleton perfoming the definded gesture to the given list. 
        ///     <param name="skeletons">Skeletons of the last frame</param>
        ///     <param name="trackedSkeletons">actual tracked skeletons </param>
        ///     <param name="sensor">Active kinect-sensor</param>
        ///     <param name="id">the id which is meant to be (re-)activated</param>
        ///     <returns>The skeletons which will be tracked</returns>
        ///     <returns>Returns the skeleton which performs the gesture, else null.</returns>
        /// </summary>
        public abstract List<TrackedSkeleton> Filter(Body[] body, List<TrackedSkeleton> trackedSkeletons,
                                                     int id, BodyFrameReader reader);
    }
}