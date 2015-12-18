using IGS.Server.Devices;
using System;
using System.Collections.Generic;

namespace IGS.Server.IGS
{
    interface ICoreMethods
    {

        List<Device> chooseDevice(User usr);

        String train(Device dev);

        int getMinVectorsPerDevice();
     

    }
}
