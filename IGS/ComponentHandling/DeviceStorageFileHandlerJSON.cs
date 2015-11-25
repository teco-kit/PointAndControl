using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGS.Server.Devices;
using Newtonsoft.Json;
using System.IO;

namespace IGS.ComponentHandling
{
    public class DeviceStorageFileHandlerJSON
    {
        readonly string DEVICE_SAVE_PATH = AppDomain.CurrentDomain.BaseDirectory + "\\devices.txt";

        public DeviceStorageFileHandlerJSON() { }

        public void addDevice(Device dev)
        { 
            List<Device> devices = readDevices();

            devices.Add(dev);

            writeDevicesToFile(devices);
        }

        public void addDeviceCoord(string devId,  Ball ball)
        {
            List<Device> devices = readDevices();

            foreach(Device dev in devices)
            {
                if(dev.Id == devId)
                {
                    dev.Form.Add(ball);
                    break;
                }
            }

            writeDevicesToFile(devices);
        }

        public List<Device> readDevices()
        {
            List<Device> devs = new List<Device>();

            string devices = File.ReadAllText(DEVICE_SAVE_PATH);

            devs = JsonConvert.DeserializeObject<List<Device>>(devices);

            return devs;
        }

        private void writeDevicesToFile(List<Device> devices)
        {
            string devicesString = JsonConvert.SerializeObject(devices);
            File.WriteAllText(DEVICE_SAVE_PATH, devicesString);
        }
        

        
    }
}
