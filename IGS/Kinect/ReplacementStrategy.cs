using System.Collections.Generic;

namespace IGS.Server.Kinect
{
    public delegate void TrackingStateEventHandler(object sender, TrackingStateEventArgs e);

    /// <summary>
    ///     This abstract class defines a replacement stratigy.
    ///     If a replacement has to be made, a skeleton will be removed. 
    ///     @author Sven Ochs, Frederk Reiche
    /// </summary>
    public abstract class ReplacementStrategy
    {
        /// <summary>
        ///     This method enforces the replacement strategy.
        ///     If a replacement must be done, a skeleton will be removed.
        ///     <param name="skeletons">Already tracked skeletons</param>
        ///     <returns>List of skeletons which will be tracked</returns>
        /// </summary>
        public abstract List<TrackedSkeleton> Replace(List<TrackedSkeleton> skeletons);

        /// <summary>
        ///     Part of designpattern: Observer(KinectUserEvent)
        /// </summary>
        public virtual event TrackingStateEventHandler TrackingStateEvents;

        public virtual void SwitchTrackingState(TrackingStateEventArgs args)
        {
        }

    }
}