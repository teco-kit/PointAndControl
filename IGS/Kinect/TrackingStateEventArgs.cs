using System;

namespace IGS.Server.Kinect
{
    public class TrackingStateEventArgs : EventArgs
    {
        public TrackingStateEventArgs(int skeletonId)
        {
            SkeletonId = skeletonId;
        }

        /// <summary>
        ///     The ID of the skeleton which should be change its trackingstate 
        ///     With the "set"-method the ID of the skeleton can be set.
        ///     With the "get"-method the ID of the skeleton can be returned.
        ///     <returns>Returns the ID of the skeleton</returns>
        /// </summary>
        public int SkeletonId { get; set; }
    }
}
