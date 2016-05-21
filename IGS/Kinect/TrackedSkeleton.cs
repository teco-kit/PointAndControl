using System;

namespace PointAndControl.Kinect
{
    /// <summary>
    ///     This class reperesents a tracked skeleton and additional the number of interactions with the kinect. 
    ///     @author Sven Ochs
    /// </summary>
    public class TrackedSkeleton : IEquatable<TrackedSkeleton>
    {
        /// <summary>
        ///     Constructor of the class Trackedskeleton.
        ///     <param name="id">Skeleton, marked as tracked</param>
        /// </summary>
        public TrackedSkeleton(int id)
        {
            Id = id;
            rightHandUp = true;
        }

        public TrackedSkeleton(int id, bool rightHand)
        {
            Id = id;
            rightHandUp = rightHand;
        }

        /// <summary>
        ///     Id of the tracked skeleton.
        ///     With the "set"-Method the id of the tracked seleton can be set.
        ///     With the "get"-Method the id of the tracked seleton can be get.
        ///     <returns>returns the id of the tracked skeleton</returns>
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Number of actions.
        ///     With the "set"-Method the number of actions of the tracked seleton can be set.
        ///     With the "set"-Method the number of actions of the tracked seleton can be get.
        ///     <returns>Gibt die Anzahl der Aktionen zurück</returns>
        /// </summary>
        public int Actions { get; set; }

        /// <summary>
        ///     true if the user used the right hand for activation, false otherwise
        /// </summary>
        public bool rightHandUp { get; set; }

        /// <summary>
        ///     Compares the id with the id of the given TrackedSkeleton.
        /// </summary>
        /// <param name="other">the TrackedSkeleton</param>
        /// <returns>this.Id == other.Id</returns>
        public bool Equals(TrackedSkeleton other)
        {
            if (Id == other.Id)
            {
                return true;
            }
            return false;
        }
    }
}