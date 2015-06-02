using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.IGS
{
    public abstract class CoreMethods
    {

        public abstract List<Device> chooseDevice(String wlanAdr, CoordTransform Transformer, UserTracker Tracker);

        public abstract void train(List<Vector3D[]> vectorList, Device dev, String value);
     

    }
}
