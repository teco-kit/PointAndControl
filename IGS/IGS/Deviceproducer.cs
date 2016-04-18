using IGS.Server.Devices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IGS.Server.IGS
{
    class Deviceproducer
    {
        public Deviceproducer() { }


        public Device produceDevice(string type, string id, string name, string path, List<Device> devices)
        {
            //int count = 1;
         
            string idparam = "";


            if(id != "")
            {
                idparam = id;
            }
            else
            {
                //Uses LINQ to findall devices with the object type == given type. Counts the result list and increases it by one
                //idparam = type + '_' + (devices.FindAll(q => DataHolder.getDeviceType(q) == type).Count + 1);
                idparam = type + '_' + (devices.FindAll(q => q.GetType().Name == type).Count + 1);
            }

            Type typeObject = Type.GetType("IGS.Server.Devices." + type);
            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, name, idparam, new List<Ball>(),
                                                           path);
                return ((Device)instance);
            }

            return null;
        }
    }
}
