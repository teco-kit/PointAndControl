using PointAndControl.ComponentHandling;
using PointAndControl.Devices;
using PointAndControl.ThirdPartyRepos;
using System;
using System.Collections.Generic;

namespace PointAndControl.MainComponents
{
    public class Deviceproducer
    {
        public Deviceproducer() { }


        public Device produceDevice(string type, string id, string name, string path, List<Device> devices)
        {
            string idparam = "";

            if (id != "")
            {
                idparam = id;
            }
            else
            {
                //Uses LINQ to find all devices with the object type == given type. Counts the result list and increases it by one
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
                if(typeObject.IsSubclassOf(typeof(RepositoryRepresentation)))
                {
                    object instance = Activator.CreateInstance(typeObject, name, idparam, path, new List<Ball>(), new DeviceHolder());
                    return ((Device)instance);
                } else
                {
                    object instance = Activator.CreateInstance(typeObject, name, idparam, path, new List<Ball>());
                    return ((Device)instance);
                }
            }

            return null;
        }
    }
}
