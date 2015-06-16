using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using IGS.Server.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Server.IGS
{
    class CollisionMethod : ICoreMethods
    {

         public Locator locator { get; set; }
    
        public CollisionMethod(DataHolder data, UserTracker tracker, CoordTransform transformer)
        {
            this.locator = new Locator(data, tracker, transformer);
        }

        public List<Device> chooseDevice(String wlanAdr)
        {
            User tempUser = locator.Data.GetUserByIp(wlanAdr);
            return tempUser != null ? CollisionDetection.Calculate(locator.Data.Devices, locator.Transformer.transformJointCoords(locator.Tracker.GetCoordinates(tempUser.SkeletonId))) : null;
        }

        public String train(String wlanAdr, String devId)
        {
            return locator.ChangeDeviceLocation(devId, wlanAdr);
        }
    }
}
