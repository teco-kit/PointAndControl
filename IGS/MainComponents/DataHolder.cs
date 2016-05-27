using System.Collections.Generic;
using System.Net;
using System;
using System.Drawing;
using System.Windows.Media.Media3D;
using PointAndControl.Devices;
using PointAndControl.Classifier;
using PointAndControl.ComponentHandling;
using PointAndControl.ThirdPartyRepos;

namespace PointAndControl.MainComponents
{
    /// <summary>
    ///     This class holds and manages the devices and the users.
    ///     @author Christopher Baumgaertner, Florian Kinn, Sven Ochs, Frederik Reiche
    /// </summary>
    public class DataHolder
    {
        private List<Device> _newDevices { get; set; }
        public Room _roomModel { get; set; }
        public EnvironmentInfoHandler _environmentHandler { get; set; }
        private EventLogger logger { get; set; }
        private DeviceHolder deviceHolder { get; set; }
        private UserHolder userHolder { get; set; }



        /// <summary>
        ///     Constructor for the DataHolder
        ///     <param name="devices">The devices for the DataHolder know at initialization</param>
        /// </summary>
        public DataHolder(EventLogger eventLogger)
        {
            _newDevices = new List<Device>();
            
            _environmentHandler = new EnvironmentInfoHandler();


            double roomWidth = _environmentHandler.getRoomWidht();
            double roomHeight = _environmentHandler.getRoomHeight();
            double roomDepth = _environmentHandler.getRoomDepth();
            _roomModel = new Room(roomWidth, roomHeight, roomDepth);

            logger = eventLogger;

            initializeDevices(_environmentHandler);
          
            userHolder = new UserHolder();
        }
        //Remark: Only start the DataHolder deviceHolder with this method, moving the storageFileHandler.readDevices() in the deviceHolder contructor will result in a JSON infinit recursion with RepositoryRepresentations
        private void initializeDevices(EnvironmentInfoHandler environmentHandler)
        {
            deviceHolder = new DeviceHolder();
            deviceHolder.initializeDevices();
            deviceHolder.actualizeAllRepos();
        }

        public List<Device> getDevices()
        {
            return deviceHolder.devices;
        }

        /// <summary>
        ///     With the "set"-method the device list can be set
        ///     With the "get"-method, the device list can be returned
        ///     <returns>Returns the list of devices</returns>
        /// </summary>
        public List<Device> newDevices
        {
            get { return _newDevices; }
            set { _newDevices = value; }
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
        ///     <param name="wlanAdr"> Used to identify and to add a user</param>
        /// </summary>
        public bool AddUser(String wlanAdr)
        {
            return userHolder.AddUser(wlanAdr);
        }

        public string AddUser(String wlanAdr, out bool success)
        {
            return userHolder.AddUser(wlanAdr, out success);
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
            return userHolder.SetTrackedSkeleton(wlanAdr, bodyID);
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
            return userHolder.DelUser(wlanAdr);
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
            return userHolder.GetUserBySkeleton(bodyId);
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
            return userHolder.GetUserByIp(wlanAdr);
        }
        /// <summary>
        ///     Adds a given device to the device list.
        ///     <param name="dev">The device which will be added to the list.</param>
        /// </summary>
        public bool AddDevice(Device dev)
        {
            return deviceHolder.AddDevice(dev);
        }

        public string AddDevice(string type, string id, string name, string path)
        {
            return deviceHolder.AddDevice(type, id, name, path);
        }

        public string AddDevice(string type, string id, string name, string path, string repoID)
        {
            return deviceHolder.AddDevice(type, id, name, path, repoID);
        }

        public string AddDevice(string type, string id, string name, string path, RepositoryRepresentation repoDevice)
        {
            return AddDevice(type, id, name, path, repoDevice);
        }


        public Device getDeviceByID(String id)
        {
            return deviceHolder.getDeviceByID(id);
        }

        public List<Device> getCompleteDeviceList()
        {
            return deviceHolder.getCompleteDeviceList();
        }

        /// <summary>
        ///     Deletes the associated bodyID from the through ID implicated user.
        ///     Löscht die zugewiesene ID des Skeletts von dem, durch die ID implizierten, User.
        ///     <param name="id">Wird benutzt um das Gerät zu indentifizieren.</param>
        ///     <returns>Löschen erfolgreich</returns>
        /// </summary>
        public bool DelTrackedSkeleton(int id)
        {
            return userHolder.DelTrackedSkeleton(id);
        }

        public void change_PlugWise_Adress(string host, string port, string path)
        {
            logger.enqueueEntry(deviceHolder.change_PlugWise_Adress(host, port, path));
        }

        public void changeRoomSize(double width, double height, double depth)
        {
            _roomModel.setRoomMeasures(width, depth, height);
            logger.enqueueEntry(String.Format("Room rezized to: Width: {0}, Depth: {1}, Height {2}", width, depth, height));
        }

        public String addDeviceCoordinates(String devId, String radius, Point3D position)
        {
            return deviceHolder.addDeviceCoordinates(devId, radius, position);
        }

        public String changeDeviceCoordinates(String devId, String radius, Point3D position)
        {
            return changeDeviceCoordinates(devId, radius, position);
        }

        private bool checkForSameDevID(String id)
        {
            return deviceHolder.checkForSameDevID(id);
        }

        public string deleteDevice(String id)
        {
            return deviceHolder.deleteDevice(id);
        }

        public void changeRoomSizeRemote(String width, String height, String depth)
        {
            Double parsedWidth;
            Double parsedHeight;
            Double parsedDepth;

            if(!Double.TryParse(width, out parsedWidth))
            {
                parsedWidth = _environmentHandler.getRoomWidht();
            }

            if(!Double.TryParse(height, out parsedHeight))
            {
                parsedHeight = _environmentHandler.getRoomHeight();
            }

            if(!Double.TryParse(depth, out parsedDepth))
            {
                parsedDepth = _environmentHandler.getRoomDepth();
            }

            changeRoomSize(parsedWidth, parsedHeight, parsedDepth);
        }

        public List<Device> getDevicesWithoutAssignedName()
        {
            return deviceHolder.getDevicesWithoutAssignedName(); 
        }
    }
}
