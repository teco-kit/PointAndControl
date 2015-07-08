using IGS.Classifier;
using System;
namespace IGS.Server.IGS
{
    /// <summary>
    ///     This class representates a user which will be identified by a wlan adress and also
    ///     eventually be assocated with a skeleton
    ///     @author Frederik Reiche
    /// </summary>
    public class User
    {
        private int _skeletonId = -1;



        //public bool deviceIDChecked { get; set; }

        //public WallProjectionSample lastClassDevSample { get; set; }
        /// <summary>
        ///     Constructor of a userobject
        ///     <param name="wlanAdr">wlan adress of a user.</param>
        /// </summary>
        public User(String wlanAdr)
        {
            WlanAdr = wlanAdr;
            Errors = "";
            TrackingState = false;
            //lastChosenDeviceID = "";
            //lastClassDevSample = null;
            //deviceIDChecked = true;
        }

        
        /// <summary>
        ///     With the "set"-method the wlan adress can be set.
        ///     With the "get"-method the wlan adress can be returned.
        ///     <returns>Returns the wlan adress</returns>
        /// </summary>
        public String WlanAdr { get; set; }
        //public String lastChosenDeviceID { get; set; }


        /// <summary>
        ///     With the "set"-method the id of the associated skeleton can be set.
        ///     With the "get"-method the id of the associated skeleton can be returned.
        ///     <returns>Returns the id of the associated skeleton</returns>
        /// </summary>
        public int SkeletonId
        {
            get { return _skeletonId; }
            set { _skeletonId = value; }
        }

        /// <summary>
        ///     With the "set"-method the tracking state can be set.
        ///     With the "get"-method the tracking state can be returned.
        ///     <returns>Returns the tracking state</returns>
        /// </summary>
        public bool TrackingState
        {
            get;
            set;
        }

        /// <summary>
        ///     The string of messages which should be send to a user.
        ///     With the "get"-method the string will be returned.
        ///     <returns>Returns the messages</returns>
        /// </summary>
        public String Errors { get; private set; }

        /// <summary>
        ///     appends another error the string of messages.
        ///     <param name="msg">the message to be appended</param>
        /// </summary>
        public void AddError(String msg)
        {
            Errors += (msg + "<br/>");
        }

        /// <summary>
        ///     resets the string of messages.
        /// </summary>
        public void ClearErrors()
        {
            Errors = "";
        }
    }
}