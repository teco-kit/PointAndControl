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

        public List<Device> chooseDevice(User usr)
        {
            return usr != null ? CollisionDetection.Calculate(locator.Data.Devices, locator.Transformer.transformJointCoords(locator.Tracker.GetCoordinates(usr.SkeletonId))) : null;
        }

        public String train(Device dev)
        {
            return locator.setDeviceLocation(dev);
        }

        public int getMinVectorsPerDevice()
        {
            return 3;
        }
    }
}
