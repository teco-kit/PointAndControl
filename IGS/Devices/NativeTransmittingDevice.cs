using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAndControl.Devices
{
    public abstract class NativeTransmittingDevice : Device
    {
        public NativeTransmittingDevice(string name, string id, string path, List<Ball> form)
            : base(name, id, path, form)
        {
        }

        public abstract String Transmit(String cmdId, String value);

        public static bool checkIfTransmitting(Device dev)
        {
            return dev.GetType().IsSubclassOf(typeof(NativeTransmittingDevice));
        }
    }
}
