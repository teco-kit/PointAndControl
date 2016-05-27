using PointAndControl.ComponentHandling;
using PointAndControl.Devices;
using PointAndControl.MainComponents;
using System.Collections;
using System.Collections.Generic;

namespace PointAndControl.ThirdPartyRepos
{
    public abstract class RepositoryRepresentation : Device
    {

        private IRepoDeviceReader repoReader { get; set; }
        public DeviceHolder deviceHolder { get; set; }

        public RepositoryRepresentation(string name, string id, string path, List<Ball> form, DeviceHolder holder) : base(name, id, path, form)
        {
            this.deviceHolder = holder;
        }

        public static bool isRepo(Device dev)
        {
            return dev.GetType().IsSubclassOf(typeof(RepositoryRepresentation));
        }

        public void actualizeDevices()
        {
            if (deviceHolder == null || deviceHolder.devices == null)
                return;

            if (hasParent())
                return;

            List<ExternalDevice> updateList = repoReader.read();
            List<ExternalDevice> tmpList = new List<ExternalDevice>();
            bool tempFound;
            foreach (ExternalDevice child in deviceHolder.devices)
            {
                tempFound = false;
                foreach (ExternalDevice update in updateList)
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

            deleteDevices(tmpList);

            addDevices(updateList);

            deviceHolder.storageFileHandler.updateDevice(this);

        }


        public void setReader(IRepoDeviceReader reader)
        {
            this.repoReader = reader;
        }

        public IRepoDeviceReader getReader()
        {
            return this.repoReader;
        }

        public List<Device> getDevices()
        {
            return deviceHolder.devices;
        }

        private void deleteDevices(List<ExternalDevice> deleteList)
        {
            deleteList.ForEach(delDev => deviceHolder.devices.Remove(delDev));
        }

        private void addDevices(List<ExternalDevice> devsToAdd)
        {
            foreach(ExternalDevice dev in devsToAdd)
            {
                deviceHolder.AddDevice(dev.GetType().Name, dev.Id, dev.Name, dev.Path, this);
            }

            return;
        }
    }
}
