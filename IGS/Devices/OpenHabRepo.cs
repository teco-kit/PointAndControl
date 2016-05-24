using PointAndControl.MainComponents;
using PointAndControl.ThirdPartyRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAndControl.Devices
{
    public class OpenHabRepo : Device
    { 
      
        public IRepoDeviceReader repoReader { get; set; }
        public DataHolder data { get; set; }
        public List<Device> childDevices { get; set; }

        public OpenHabRepo(string name, string id, string path, List<Ball> form, DataHolder holder) : 
            base(name, id, path, form)
        {
            if (!path.StartsWith("http://"))
                path = "http://" + path;
            repoReader = new OpenHABDeviceReader(path);
            childDevices = new List<Device>();
            data = holder;

            actualizeDevices(repoReader.read());
        }

        public override string Transmit(string cmdId, string value)
        {
            throw new NotImplementedException();
        }

        public void addDevice(Device dev)
        {
            data.AddDevice("ExternalDevice", dev.Id, dev.Name, dev.Path);
        }

        public void actualizeDevices(List<Device> updateList)
        {
            if (hasParent())
                return;

            List<Device> tmpList = new List<Device>();
            bool tempFound;
            foreach (Device child in childDevices)
            {
                tempFound = false;
                foreach (Device update in updateList)
                {
                    if (child.Id == update.Id)
                    {
                        updateList.Remove(update);
                        tempFound = true;
                        break;
                    }
                }

                if (!tempFound)
                    tmpList.Add(child);

            }

            tmpList.ForEach(x => childDevices.Remove(x));

            updateList.ForEach(dev => data.AddDevice(dev.GetType().Name, dev.Id, dev.Name, dev.Path, this ));

            data._deviceStorageHandling.updateDevice(this);
        }

        public static bool isRepo(Device dev)
        {
            return dev.GetType().Name == "OpenHabRepo";
        }
    }
}
