using IGS.Server.Devices;
using System;
using System.Collections.Generic;

namespace IGS.IGS
{
    class Deviceproducer
    {
        public Deviceproducer() { }

        public Device produceDevice(string type, string name, string address, string port, List<Device> devices)
        {
            int count = 0;

            foreach(Device dev in devices)
            {
                string devType = getDeviceType(dev);
                if (devType == type)
                    count++;
            }

            string idparam = type + "_" + count;

            // TODO: for testing we do not wand to add the device to XML
            // XMLComponentHandler.addDeviceToXML(parameter, count);

            Type typeObject = Type.GetType("IGS.Server.Devices." + type);
            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, name, idparam, new List<Ball>(),
                                                           address, port);
                return ((Device)instance);
            }
            return null;
        }

        public string getDeviceType(Device dev)
        {
            string[] split = dev.GetType().ToString().Split('.');
            return split[split.Length - 1];
        }
    }
}
