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
    class CollisionMethod : CoreMethods
    {

         public Locator locator { get; set; }
    
        public CollisionMethod(DataHolder data, UserTracker tracker, CoordTransform transformer)
        {
            this.locator = new Locator(data, tracker, transformer);
        }

        public override List<Device> chooseDevice(String wlanAdr, CoordTransform Transformer, UserTracker Tracker)
        {
            User tempUser = locator.Data.GetUserByIp(wlanAdr);
            return tempUser != null ? CollisionDetection.Calculate(locator.Data.Devices, Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId))) : null;
        }

        public override void train(List<Vector3D[]> vectorList, Device dev, String value)
        {
            locator.ChangeDeviceLocation(dev.Id, value);
        }
    }
}
