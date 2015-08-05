using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Server.IGS
{
    interface ICoreMethods
    {

        List<Device> chooseDevice(User usr);

        String train(Device dev);

        int getMinVectorsPerDevice();
     

    }
}
