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
    public class DevChooseMethodCollDet : ChooseDeviceMethod
    {

        
        public override List<Device> chooseDevice(String wlanAdr, CoordTransform Transformer, UserTracker Tracker, DataHolder Data)
        {
            User tempUser = Data.GetUserByIp(wlanAdr);
            return tempUser != null ? CollisionDetection.Calculate(Data.Devices, Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId))) : null;
        }
        
    }
}
