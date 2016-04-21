using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using PointAndControl.Devices;

namespace PointAndControl.ComponentHandling
{
    public class DeviceStorageFileHandlerJSON
    {
        readonly string DEVICE_SAVE_PATH = AppDomain.CurrentDomain.BaseDirectory + "\\devices.txt";
        private JsonSerializerSettings setting { get; set; }

        public DeviceStorageFileHandlerJSON()
        {
            setting = new JsonSerializerSettings();
            setting.TypeNameHandling = TypeNameHandling.All;
        }

        public void addDevice(Device dev)
        {
            if (!File.Exists(DEVICE_SAVE_PATH))
            {
                File.Create(DEVICE_SAVE_PATH).Close();
            }


            List<Device> devices = readDevices();

            devices.Add(dev);

            writeDevicesToFile(devices);
        }

        public string addDeviceCoord(string devId,  Ball ball)
        {
            
            List<Device> devices = readDevices();

            if (devices == null || devices.Count == 0)
                return Properties.Resources.SpecifiedDeviceNotFound;

            foreach(Device dev in devices)
            {
                if(dev.Id == devId)
                {
                    dev.Form.Add(ball);
                    writeDevicesToFile(devices);
                    return Properties.Resources.CoordinatesAdded;
                }
            }

           

            return Properties.Resources.NoCoordAdded;
        }

        public string changeDeviceCoord(string devId, Ball ball)
        {
            List<Device> devices = readDevices();

            if (devices == null || devices.Count == 0)
                return Properties.Resources.SpecifiedDeviceNotFound;

            foreach (Device dev in devices)
            {
                if (dev.Id == devId)
                {
                    dev.Form.Clear();
                    dev.Form.Add(ball);
                    writeDevicesToFile(devices);
                    return Properties.Resources.CoordinatesAdded;
                }
            }



            return Properties.Resources.NoCoordAdded;
        }

        public List<Device> readDevices()
        {
            if (!File.Exists(DEVICE_SAVE_PATH))
            {
                File.Create(DEVICE_SAVE_PATH).Close();
            }

            String devices = File.ReadAllText(DEVICE_SAVE_PATH);



            List<Device> devs = JsonConvert.DeserializeObject<List<Device>>(devices,setting);


            if (devs == null)
            {
                devs = new List<Device>();
            }

            return devs;
        }

        private void writeDevicesToFile(List<Device> devices)
        { 
            string devicesString = JsonConvert.SerializeObject(devices, Formatting.Indented, setting);
            File.WriteAllText(DEVICE_SAVE_PATH, devicesString);
        }

        public void deleteDevice(String id)
        {
            List<Device> devs = readDevices();

            foreach(Device device in devs)
            {
                if(device.Id == id)
                {
                    devs.Remove(device);
                    break;
                }
            }

            writeDevicesToFile(devs);

        }

        
        

        
    }
}
