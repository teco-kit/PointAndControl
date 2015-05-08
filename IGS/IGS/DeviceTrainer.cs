using IGS.Server.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.IGS
{
    abstract class DeviceTrainer
    {

        public abstract void train(List<Vector3D[]> vectorList, Device dev, String value);
     
    }
}
