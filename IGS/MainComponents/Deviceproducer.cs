using PointAndControl.Devices;
using System;
using System.Collections.Generic;

namespace PointAndControl.MainComponents
{
    class Deviceproducer
    {
        public Deviceproducer() { }


        public Device produceDevice(string type, string id, string name, string path, List<Device> devices)
        {
            //int count = 1;

            string idparam = "";


            if (id != "")
            {
                idparam = id;
            }
            else
            {
                //Uses LINQ to findall devices with the object type == given type. Counts the result list and increases it by one
                idparam = type + '_' + (devices.FindAll(q => q.GetType().Name == type).Count + 1);
            }


            Type typeObject = null;

            foreach(Type t in Device.deviceTypes)
            {
                if(type == t.Name)
                {
                    typeObject = t;
                    break;
                }
            }

            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, name, idparam, path, new List<Ball>());
                return ((Device)instance);
            }

            return null;
        }
    }
}
