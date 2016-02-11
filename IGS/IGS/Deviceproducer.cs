using IGS.Server.Devices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IGS.IGS
{
    class Deviceproducer
    {
        private readonly List<String> internalDevices = new List<string>{ "Boxee", "Beamer", "Kodi", "Plugwise", "XBMCv11", "NecLcdMonitorMultiSyncV421" };
        public Deviceproducer() { }


        public Device produceDevice(string type, string id, string name, string path, List<Device> devices)
        {
            int count = 1;
         
            string idparam = "";
            if (type != "ExternalDevice" && id == "")
            {
                foreach (Device dev in devices)
                {
                    string devType = getDeviceType(dev);
                    if (devType == type)
                        count++;
                }
                idparam = type + "_" + count;
            }
            else if (type == "ExternalDevice")
            {
                if(id == "")
                {
                    idparam = id;
                } else
                {
                    //generateExternalDevice ID 
                }
            } else 
            {
                return null;
            }
            // TODO: for testing we do not wand to add the device to XML
            // XMLComponentHandler.addDeviceToXML(parameter, count);

            Type typeObject = Type.GetType("IGS.Server.Devices." + type);
            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, name, idparam, new List<Ball>(),
                                                           path);
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
