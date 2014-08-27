using System.Collections.Generic;
using System.Net;
using IGS.Server.Devices;
using System;
using System.Xml;

namespace IGS.Server.IGS
{
    /// <summary>
    ///     This class holds and manages the devices and the users.
    ///     @author Christopher Baumgaertner, Florian Kinn, Sven Ochs, Frederik Reiche
    /// </summary>
    public class DataHolder
    {
        /// <summary>
        ///     List of the devices
        /// </summary>
        private List<Device> _devices;

        /// <summary>
        ///     List of the users
        /// </summary>
        private List<User> _users;

        /// <summary>
        ///     Constructor for the DataHolder
        ///     <param name="devices">The devices for the DataHolder know at initialization</param>
        /// </summary>
        public DataHolder(List<Device> devices)
        {
            _devices = devices;
            _users = new List<User>();
        }

        /// <summary>
        ///     With the "set"-method the device list can be set
        ///     With the "get"-method, the device list can be returned
        ///     <returns>Returns the list of devices</returns>
        /// </summary>
        public List<Device> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        /// <summary>
        ///     With the "set"-method the user list can be set
        ///     With the "get"-method, the user list can be returned
        ///     <returns>Returns the list of users</returns>
        /// </summary>
        public List<User> Users
        {
            get { return _users; }
            set { _users = value; }
        }

        /// <summary>
        ///     With the "set"-method the local ip can be set
        ///     With the "get"-method, the local ip can be returned
        ///     <returns>Returns the local ip</returns>
        /// </summary>
        public IPAddress LocalIp { get; set; }


        /// <summary>
        ///     Creates the user, who will be identified by his wlan adress 
        ///     and adds the user to the user-list.
        ///     
        ///     <param name="wlanAdr"> Used to identify and to add a user</param>
        /// </summary>
        public bool AddUser(String wlanAdr)
        {
            for (int i = 0; i < _users.Count; i++)
            {
                if (_users[i].WlanAdr == wlanAdr)
                {
                    return false;
                }
            }
            User createdUser = new User(wlanAdr);
            Users.Add(createdUser);
            return true;
        }

        /// <summary>
        ///     The user with the wlan adress wlanAdr will be connected with a bodyID.
        ///     <param name="wlanAdr">Used to identify the user.</param>
        ///     <param name="bodyID">The ID of the Body which will be connected to the user specified 
        ///     by the wLan Adress.</param>
        ///     <returns>If the process was successful</returns>
        /// </summary>
        public bool SetTrackedSkeleton(String wlanAdr, int bodyID)
        {
           
            bool success = false;

            for (int i = 0; i < _users.Count && success == false; i++)
            {
                if (_users[i].WlanAdr == wlanAdr)
                {
                    _users[i].SkeletonId = bodyID;
                    success = true;
                }
            }
          
            return success;
        }

        /// <summary>
        ///     Deletes a user identified by his wlan adress 
        ///     from the user-list 
        ///     <param name="wlanAdr">
        ///         Used to identify the user which will be deleted
        ///     </param>
        ///     <returns>If the process was successful</returns>
        /// </summary>
        public bool DelUser(String wlanAdr)
        {
            bool success = false;

            for (int i = 0; i < _users.Count && success == false; i++)
            {
                if (_users[i].WlanAdr == wlanAdr)
                {
                    _users.RemoveAt(i);
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        ///     Returns the user by the id of his/her associated body.
        ///     <param name="bodyId">
        ///         Used to return the user with the specified bodyId
        ///     </param>
        ///     <returns>Returns the userobject, if no user is associated with the bodyID, null will be returned</returns>
        /// </summary>
        public User GetUserBySkeleton(int bodyId)
        {
            User tempUser = null;
            bool found = false;

            for (int i = 0; i < _users.Count && found == false; i++)
            {
                if (_users[i].SkeletonId == bodyId)
                {
                    tempUser = _users[i];
                    found = true;
                }
            }

            return tempUser;
        }

        /// <summary>
        ///     Returns the user through its wlan adress.
        ///     <param name="wlanAdr">
        ///         Is used to identify the user and retrun the userobject
        ///     </param>
        ///     <returns>Returns the userobject. If no user with the wlanAdr exists NULL will be returned</returns>
        /// </summary>
        public User GetUserByIp(String wlanAdr)
        {
            User tempUser = null;
            bool found = false;

            for (int i = 0; i < _users.Count && found == false; i++)
            {
                if (_users[i].WlanAdr == wlanAdr)
                {
                    tempUser = _users[i];
                    found = true;
                }
            }
            return tempUser;
        }


        /// <summary>
        ///     Adds a given device to the device list.
        ///     <param name="dev">The device which will be added to the list.</param>
        /// </summary>
        public void AddDevice(Device dev)
        {
            bool contains = false;

            for (int i = 0; i < _devices.Count && contains == false; i++)
            {
                if (_devices[i].Id == dev.Id)
                {
                    contains = true;
                }
            }

            if (contains == false)
            {
                _devices.Add(dev);
            }
        }

        /// <summary>
        ///     Returns a device with its id.
        ///     <param name="id">Is used to identify a device.</param>
        ///     <returns>Returns the deviceobject. If no device with the id exists NULL will be returned<returns>
        /// </summary>
        public Device GetDevice(String id)
        {
            Device tempDevice = null;
            bool found = false;

            for (int i = 0; i < _devices.Count && found == false; i++)
            {
                if (_devices[i].Id == id)
                {
                    tempDevice = _devices[i];
                    found = true;
                }
            }
            return tempDevice;
        }


        /// <summary>
        ///     Deletes the associated bodyID from the through ID implicated user.
        ///     Löscht die zugewiesene ID des Skeletts von dem, durch die ID implizierten, User.
        ///     <param name="id">Wird benutzt um das Gerät zu indentifizieren.</param>
        ///     <returns>Löschen erfolgreich</returns>
        /// </summary>
        public bool DelTrackedSkeleton(int id)
        {
            bool success = false;

            for (int i = 0; i < Users.Count && success == false; i++)
            {
                if (_users[i].SkeletonId == id)
                {
                    _users[i].SkeletonId = -1;
                    success = true;
                }
            }
            return success;
        }

        /// <summary>
        /// Propagates the given adress to all plugwise devices
        /// </summary>
        /// <param name="input">the new adress for all plugwises</param>
        public void Change_PlugWise_Adress(String input)
        {

            String[] splitted;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i].Id.Contains("Plugwise"))
                {

                 

                    splitted = _devices[i].CommandString.Split('/',
                        ':');

                    String newCommandString = input + splitted[6];

                    _devices[i].CommandString = newCommandString;

                    
                }
            }
        }

    }

}