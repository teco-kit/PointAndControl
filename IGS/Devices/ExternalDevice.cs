using IGS.Server.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.Devices
{
    class ExternalDevice : Device
    {
        public ExternalDevice(String name, String id, List<Ball> form, String address, String port) : base(name, id, form)
        {
            connection = new Http(Convert.ToInt32(port), address);
            commandString = "";
        }

        public override string Transmit(string cmdId, string value)
        {
            throw new NotImplementedException();
        }
    }
}
