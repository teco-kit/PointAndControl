using IGS.Server.Devices;
using IGS.Server.IGS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IGS.IGS
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
                idparam = type + '_' + (devices.FindAll(q => DataHolder.getDeviceType(q) == type).Count + 1);

                //foreach (Device dev in devices)
                //{
                //    string devType = DataHolder.getDeviceType(dev);
                //    if (devType == type)
                //        count++;
                //}
                ////idparam = type + "_" + count;
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
