using System.Collections.Generic;
using System.Net;
using IGS.Server.Devices;
using System;
using System.Drawing;
using IGS.Classifier;
using IGS.ComponentHandling;
using System.Windows.Media.Media3D;

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
        private List<Device> _devices { get; set; }

        private List<Device> _newDevices { get; set; }

        /// <summary>
        ///     List of the users
        /// </summary>
        private List<User> _users { get; set; }

        public Room _roomModel { get; set; }

        public DeviceStorageFileHandlerJSON _deviceStorageHandling { get; set; }
        public EnvironmentInfoHandler _environmentHandler { get; set; }

        private Deviceproducer devProducer { get; set; }

        private EventLogger logger { get; set; }



        /// <summary>
        ///     Constructor for the DataHolder
        ///     <param name="devices">The devices for the DataHolder know at initialization</param>
        /// </summary>
        public DataHolder(EventLogger eventLogger)
        {

            _newDevices = new List<Device>();
            _deviceStorageHandling = new DeviceStorageFileHandlerJSON();
            
            _environmentHandler = new EnvironmentInfoHandler();
            devProducer = new Deviceproducer();

            _devices = _deviceStorageHandling.readDevices();

            change_PlugWise_Adress(_environmentHandler.getPWAdress());

            double roomWidth = _environmentHandler.getRoomWidht();
            double roomHeight = _environmentHandler.getRoomHeight();
            double roomDepth = _environmentHandler.getRoomDepth();
            _roomModel = new Room(roomWidth, roomHeight, roomDepth);

            _devices.ForEach(x => x.color = pickRandomColor());

            _users = new List<User>();
            logger = eventLogger;
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
        ///     <param name="wlanAdr"> Used to identify and to add a user</param>
        /// </summary>
        public bool AddUser(String wlanAdr)
        {
            foreach (User u in _users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    //return Properties.Resources.UserExists;
                    return false;
                }
            }

            User createdUser = new User(wlanAdr);
            Users.Add(createdUser);
            Console.WriteLine("User added");
            return true;
            //return Properties.Resources.UserAdded;
        }

        public string AddUser(String wlanAdr, out bool success)
        {
            foreach (User u in _users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    success = false;
                    return String.Format(Properties.Resources.UserExists, wlanAdr);
                }
            }

            User createdUser = new User(wlanAdr);
            Users.Add(createdUser);
            Console.WriteLine("User added");
            success = true;
            return String.Format(Properties.Resources.UserAdded, wlanAdr);
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
            foreach (User u in _users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    u.SkeletonId = bodyID;
                    return true;
                }
            }

            return false;
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
            for (int i = 0; i < _users.Count; i++)
            {
                if (_users[i].WlanAdr == wlanAdr)
                {
                    _users.RemoveAt(i);
                    return true;
                }
            }
            return false;
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
            foreach (User u in _users)
            {
                if (u.SkeletonId == bodyId)
                {
                    return u;
                }
            }
            return null;
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
            foreach (User u in _users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    return u;
                }
            }
            return null;
        }


        /// <summary>
        ///     Adds a given device to the device list.
        ///     <param name="dev">The device which will be added to the list.</param>
        /// </summary>
        public bool AddDevice(Device dev)
        {
            if (checkForSameDevID(dev.Id))
                return false;

            checkAndWriteColorForNewDevice(dev);
            _deviceStorageHandling.addDevice(dev);
            _devices.Add(dev);

            return true;
        }

        public string AddDevice(string type, string id, string name, string path)
        {

            if (checkForSameDevID(id))
                return Properties.Resources.SameDevIDEx;

            if (!Device.checkForIpAndPort(path) && type != "ExternalDevice")
                return Properties.Resources.UnknownError;

            Device newDevice = devProducer.produceDevice(type, id, name, path, _devices);

            if (newDevice == null)
                return Properties.Resources.UnknownError;
            

            checkAndWriteColorForNewDevice(newDevice);
            _devices.Add(newDevice);
            _deviceStorageHandling.addDevice(newDevice);
            return newDevice.Id;
        }
        /// <summary>
        ///     Returns a device with its id.
        ///     <param name="id">Is used to identify a device.</param>
        ///     <returns>Returns the deviceobject. If no device with the id exists NULL will be returned<returns>
        /// </summary>
        public Device getDeviceByID(String id)
        {
            if (id == null || id == "")
                return null;

            foreach (Device dev in _devices)
            {
                if (dev.Id == id)
                {
                    return dev;
                }
            }
            return null;
        }

        /// <summary>
        ///     Deletes the associated bodyID from the through ID implicated user.
        ///     Löscht die zugewiesene ID des Skeletts von dem, durch die ID implizierten, User.
        ///     <param name="id">Wird benutzt um das Gerät zu indentifizieren.</param>
        ///     <returns>Löschen erfolgreich</returns>
        /// </summary>
        public bool DelTrackedSkeleton(int id)
        {
            foreach (User u in _users)
            {
                if (u.SkeletonId == id)
                {
                    u.SkeletonId = -1;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Publishes the given adress to all plugwise devices
        /// </summary>
        /// <param name="input">the new adress for all plugwises</param>
        private void change_PlugWise_Adress(String input)
        {

            String[] splitted;

            foreach(Device dev in Devices)
            {
                if (getDeviceType(dev).Equals("Plugwise"))
                {
                    splitted = dev.CommandString.Split('/',
                        ':');

                    String newCommandString = input + splitted[6];

                    dev.CommandString = newCommandString;
                }
            }
        }

        public void change_PlugWise_Adress(string host, string port, string path)
        {
            getRemainingPlugComponents(host, port, path); 
            _environmentHandler.writePWcomponents(host, port, path);
            string completeAdr = _environmentHandler.getPWAdress();
            change_PlugWise_Adress(completeAdr);
            logger.enqueueEntry(String.Format("New PlugwiseAdress: {0}", completeAdr));
        }

        private void getRemainingPlugComponents(string host, string port, string path)
        {
            if(host == null || host == "")
            {
                host = _environmentHandler.getPWHost();
            }

            if (port == null || port == "")
            {
                port = _environmentHandler.getPWPort();
            }

            if (path == null || path == "")
            {
                path = _environmentHandler.getPWPath();
            }
        }

        public Color pickRandomColor()
        {
            bool uniqueFound = true;

            Random random = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[random.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);

            foreach (Device d in Devices)
            {
                if (d.color != null)
                {
                    if (d.color.Equals(randomColor))
                    {
                        uniqueFound = false;
                    }
                }
            }
            if (uniqueFound == false)
            {
                randomColor = pickRandomColor();
            }

            return randomColor;
        }

        public void checkAndWriteColorForNewDevice(Device dev)
        {
            if (checkForSameDevID(dev.Id))
                return;

            dev.color = pickRandomColor();

            return;
        }

        public void changeRoomSize(double width, double height, double depth)
        {
            _roomModel.setRoomMeasures(width, depth, height);
            logger.enqueueEntry(String.Format("Room rezized to: Width: {0}, Depth: {1}, Height {2}", width, depth, height));
        }

        public String addDeviceCoordinates(String devId, String radius, Point3D position)
        {
            Ball coord = new Ball(position, double.Parse(radius));
            this.getDeviceByID(devId).Form.Add(coord);
            return _deviceStorageHandling.addDeviceCoord(devId, coord);
        }

        public String changeDeviceCoordinates(String devId, String radius, Point3D position)
        {
            Ball coord = new Ball(position, double.Parse(radius));
            Device dev = getDeviceByID(devId);
            dev.Form.Clear();
            dev.Form.Add(coord);
            return _deviceStorageHandling.addDeviceCoord(devId, coord);
        }

        private bool checkForSameDevID(String id)
        {
            foreach(Device d in Devices)
            {
                if (d.Id == id)
                    return true;
            }

            return false;
        }

        public static string getDeviceType(Device dev)
        {
            string[] split = dev.GetType().ToString().Split('.');
            return split[split.Length - 1];
        }

        public string deleteDevice(String id)
        {
            String retStr = Properties.Resources.DevNotFoundDeletion;
            
            foreach(Device d in Devices)
            {
                if (d.Id == id)
                {
                    _deviceStorageHandling.deleteDevice(d.Id);
                    Devices.Remove(d);
                    retStr = Properties.Resources.DevDeleted;
                    break;
                }
            }

            return retStr;
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

        


    }
}
