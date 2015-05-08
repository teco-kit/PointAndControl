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

namespace IGS.IGS
{
    class DeviceTrainerCollision : DeviceTrainer
    {

        public Locator locator { get; set; }
   


        public DeviceTrainerCollision(DataHolder data, UserTracker tracker, CoordTransform transformer)
        {
            this.locator = new Locator(data, tracker, transformer);
        }

        public override void train(List<Vector3D[]> vectorList, Device dev, String value)
        {
            locator.ChangeDeviceLocation(dev.Id, value);
        }
    }
}
