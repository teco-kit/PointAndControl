using System;
using System.Collections.Generic;

namespace IGS.Server.Kinect
{
    /// <summary>
    ///     This class LFU implements a concrete replacement strategy.
    ///     If a replacement takes place, a skeleton has to be removed.
    ///     The skeleton with the least actions since the last replacement will be removed.
    ///     (least-frequently-used)
    ///     @author Sven Ochs
    /// </summary>
    public class Lfu : ReplacementStrategy
    {
        /// <summary>
        ///     Part of the design pattern: Observer(TrackingStateEvent)
        /// </summary>
        public override event TrackingStateEventHandler TrackingStateEvents;


        /// <summary>
        ///     This method enforces the replacement strategy. 
        ///     If a replacement must be done, a skeleton will be removed.
        ///     <param name="skeletons">Already tracked skeletons</param>
        ///     <returns>List of skeletons which will be tracked</returns>
        /// </summary>
        public override List<TrackedSkeleton> Replace(List<TrackedSkeleton> skeletons)
        {
            
            if (skeletons.Count == 6)
            {

                int minActions = int.MaxValue;
                int replacePosition = -1;
                for (int i = 0; i < skeletons.Count; i++)
                {
                    if (minActions > skeletons[i].Actions)
                    {
                        //find the user with the least actions. 
                        minActions = skeletons[i].Actions;
                        replacePosition = i;
                    }
                    //reset of the actions
                    skeletons[i].Actions = 0;
                }
                SwitchTrackingState(new TrackingStateEventArgs(skeletons[replacePosition].Id));
                skeletons.Remove(skeletons[replacePosition]);
            }
            
            return skeletons;
        }

        public override void SwitchTrackingState(TrackingStateEventArgs args)
        {
            //If there is a subscription the event will be executed
            if (TrackingStateEvents != null)
            {
                TrackingStateEvents(this, args);
            }
        }


    }
}