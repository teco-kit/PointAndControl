using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.IGS
{
    public abstract class ChooseDeviceMethod
    {

  
        public abstract List<Device> chooseDevice(String wlanAdr, CoordTransform Transformer, UserTracker Tracker, DataHolder Data);
      
    }
}
