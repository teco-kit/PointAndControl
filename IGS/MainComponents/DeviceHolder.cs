using PointAndControl.ComponentHandling;
using PointAndControl.Devices;
using PointAndControl.ThirdPartyRepos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PointAndControl.MainComponents
{
    public class DeviceHolder
    {
        public List<Device> devices { get; set; }
        public DeviceStorageFileHandlerJSON storageFileHandler { get; set; }
        public Deviceproducer deviceProducer { get; set; }
        public EnvironmentInfoHandler environmentHandler { get; set; }

        public DeviceHolder()
        {
            devices = new List<Device>();
            storageFileHandler = new DeviceStorageFileHandlerJSON();
            deviceProducer = new Deviceproducer();
            environmentHandler = new EnvironmentInfoHandler();
        }

        public void initializeDevices()
        {
            devices = storageFileHandler.readDevices();
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
            storageFileHandler.addDevice(dev);
            devices.Add(dev);
            return true;
        }

        public string AddDevice(string type, string id, string name, string path)
        {

            if (checkForSameDevID(id))
                return Properties.Resources.SameDevIDEx;

            if (!Device.checkForIpAndPort(path) && type != "ExternalDevice")
                return Properties.Resources.UnknownError;

            Device newDevice = deviceProducer.produceDevice(type, id, name, path, getCompleteDeviceList());

            if (newDevice == null)
                return Properties.Resources.UnknownError;


            checkAndWriteColorForNewDevice(newDevice);
            devices.Add(newDevice);
            storageFileHandler.addDevice(newDevice);
            return newDevice.Id;
        }

        public string AddDevice(string type, string id, string name, string path, string repoID)
        {
            if (checkForSameDevID(id))
                return Properties.Resources.SameDevIDEx;

            if (name == null)
                name = "";

            Device repoDevice = getDeviceByID(repoID);



            if (repoDevice == null || !RepositoryRepresentation.isRepo(repoDevice))
            {
                return "RepoDevice not found"; //TODO: Create good response and localize ist
            }

            if (repoDevice.hasParent())
                return "Device cant be added to a childDevice"; //TODO: Create good Response and localize it

            Device newDevice = deviceProducer.produceDevice(type, id, name, path, getCompleteDeviceList());

            if (newDevice == null)
                return Properties.Resources.UnknownError;

            checkAndWriteColorForNewDevice(newDevice);
            newDevice.addParent(repoDevice);
            ((RepositoryRepresentation)repoDevice).getDevices().Add(newDevice);
            storageFileHandler.addDevice(newDevice);
            return newDevice.Id;

        }

        public string AddDevice(string type, string id, string name, string path, RepositoryRepresentation repoDevice)
        {
            if (checkForSameDevID(id))
                return Properties.Resources.SameDevIDEx;

            if (name == null)
                name = "";

            if (repoDevice.hasParent())
                return "Device cant be added to a childDevice"; //TODO: Create good Response and localize it

            Device newDevice = deviceProducer.produceDevice(type, id, name, path, getCompleteDeviceList());

            if (newDevice == null)
                return Properties.Resources.UnknownError;

            checkAndWriteColorForNewDevice(newDevice);
            newDevice.addParent(repoDevice);
            repoDevice.getDevices().Add(newDevice);
            return newDevice.Id;
        }

        public Device getDeviceByID(String id)
        {
            if (id == null || id == "")
                return null;

            foreach (Device dev in devices)
            {
                if (dev.Id == id)
                {
                    return dev;
                }

                if (RepositoryRepresentation.isRepo(dev))
                {
                    foreach (Device childDev in ((RepositoryRepresentation)dev).getDevices())
                    {
                        if (childDev.Id == id)
                        {
                            return childDev;
                        }
                    }
                }
            }
            return null;
        }

        public List<Device> getCompleteDeviceList()
        {
            List<Device> returnList = new List<Device>();

            foreach (Device dev in devices)
            {
                if (RepositoryRepresentation.isRepo(dev))
                    ((RepositoryRepresentation)dev).getDevices().ForEach(x => returnList.Add(x));
                else
                {
                    returnList.Add(dev);
                }
            }

            return returnList;
        }


        /// <summary>
        /// Publishes the given adress to all plugwise devices
        /// </summary>
        /// <param name="input">the new adress for all plugwises</param>
        private void change_PlugWise_Adress(String input)
        {
            String[] splitted;

            foreach (Device dev in getCompleteDeviceList())
            {
                //if (getDeviceType(dev).Equals("Plugwise"))
                if (dev.GetType().Name.Equals("Plugwise"))
                {
                    splitted = dev.CommandString.Split('/',
                        ':');

                    String newCommandString = input + splitted[6];

                    dev.CommandString = newCommandString;
                }
            }
        }
        public string change_PlugWise_Adress(string host, string port, string path)
        {
            getRemainingPlugComponents(host, port, path);
            environmentHandler.writePWcomponents(host, port, path);
            string completeAdr = environmentHandler.getPWAdress();
            change_PlugWise_Adress(completeAdr);
            return String.Format("New PlugwiseAdress: {0}", completeAdr);
        }

        public Color pickRandomColor()
        {
            bool uniqueFound = true;

            Random random = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[random.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);

            foreach (Device d in devices)
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

            if (dev.color != null)
                return;

            dev.color = pickRandomColor();

            return;
        }
        public String addDeviceCoordinates(String devId, String radius, Point3D position)
        {
            Ball coord = new Ball(position, double.Parse(radius));
            Device dev = getDeviceByID(devId);

            dev.Form.Add(coord);

            if (dev.hasParent())
            {
                Device parent = getDeviceByID(dev.parentID);
                if (RepositoryRepresentation.isRepo(parent))
                {
                    return storageFileHandler.updateDevice(parent);
                }
            }


            return storageFileHandler.addDeviceCoord(devId, coord);
        }

        public String changeDeviceCoordinates(String devId, String radius, Point3D position)
        {
            Ball coord = new Ball(position, double.Parse(radius));
            Device dev = getDeviceByID(devId);
            dev.Form.Clear();
            dev.Form.Add(coord);
            return storageFileHandler.changeDeviceCoord(devId, coord);
            //return storageFileHandler.addDeviceCoord(devId, coord);
        }

        public bool checkForSameDevID(String id)
        {
            return getCompleteDeviceList().Exists(x => x.Id == id);
        }

        public string deleteDevice(String id)
        {
            Device tempDev;
            String retStr = Properties.Resources.DevNotFoundDeletion;

            foreach (Device d in getCompleteDeviceList())
            {
                if (d.Id == id)
                {
                    if (d.hasParent())
                    {
                        tempDev = d;
                        Device parent = getDeviceByID(tempDev.parentID);
                        ((RepositoryRepresentation)parent).getDevices().Remove(d);
                        storageFileHandler.updateDevice(parent);
                        retStr = Properties.Resources.DevDeleted;
                    }
                    else
                    {
                        storageFileHandler.deleteDevice(d.Id);
                        devices.Remove(d);
                        retStr = Properties.Resources.DevDeleted;
                    }
                    break;
                }
            }

            return retStr;
        }
        public List<Device> getDevicesWithoutAssignedName()
        {
            return getCompleteDeviceList().Where(dev => dev.GetType().Name == "ExternalDevice" && ((ExternalDevice)dev).hasAssignedName == false).ToList();
            
        }

        private void getRemainingPlugComponents(string host, string port, string path)
        {
            if (host == null || host == "")
            {
                host = environmentHandler.getPWHost();
            }

            if (port == null || port == "")
            {
                port = environmentHandler.getPWPort();
            }

            if (path == null || path == "")
            {
                path = environmentHandler.getPWPath();
            }
        }

        public bool assignName(string id, string name)
        {
            Device dev = getDeviceByID(id);

            if (dev == null)
                return false;

            if (dev.GetType().Name == "ExternalDevice")
            {
                ExternalDevice ext = (ExternalDevice)dev;

                if (!ext.hasAssignedName)
                {
                    ext.assignName(name);

                    if (ext.hasParent())
                    {
                        storageFileHandler.updateDevice(getDeviceByID(ext.parentID));
                    } else
                    {
                        storageFileHandler.updateDevice(ext);
                    }

                    return true;
                }
            }

            return false;
        }

        public List<RepositoryRepresentation> getAllRepos()
        {
            List<RepositoryRepresentation> repos = new List<RepositoryRepresentation>();

            foreach(Device dev in devices)
            {
                if (RepositoryRepresentation.isRepo(dev))
                {
                    repos.Add((RepositoryRepresentation)dev);
                }
            }

            return repos;
        }

        public void actualizeAllRepos()
        {
            getAllRepos().ForEach(repo => repo.actualizeDevices());
        }
    }
}
