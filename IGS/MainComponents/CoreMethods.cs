using PointAndControl.Devices;
using System;
using System.Collections.Generic;

namespace PointAndControl.MainComponents
{
    interface ICoreMethods
    {

        List<Device> chooseDevice(User usr);

        String train(Device dev);

        int getMinVectorsPerDevice();
     

    }
}
