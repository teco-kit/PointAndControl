using PointAndControl.Devices;
using PointAndControl.MainComponents;
using System;
using System.Collections.Generic;

namespace PointAndControl.ThirdPartyRepos
{
    public class OpenHabRepo : RepositoryRepresentation
    {
        //private IRepoDeviceReader repoReader { get; set; }
       
        public OpenHabRepo(string name, string id, string path, List<Ball> form, DeviceHolder holder) : 
            base(name, id, path, form, holder)
        {
            if (!path.StartsWith("http://"))
                path = "http://" + path;
            this.setReader(new OpenHABDeviceReader(path));
        }

        
    }
}
