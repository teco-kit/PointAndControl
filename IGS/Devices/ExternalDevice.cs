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
        private readonly String _commandString;

        /// <summary>
        ///     Constructor of a External Device object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="path">The Path to communicate with the device</param>  
        /// </summary>
        public ExternalDevice(String name, String id, List<Ball> form, String path)
            : base(name, id, path, form)
        {
            String[] ipAndPort = splitPathToIPAndPort();
            connection = new Http(Convert.ToInt32(ipAndPort[1]), ipAndPort[0]);

            _commandString = path;
        }

        public override string Transmit(string cmdId, string value)
        {
            throw new NotImplementedException();
        }
    }
}
