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

        //public ExternalDevice(String name, String id, List<Ball> form, String address, String port) 
        //    : base(name, id, path, form)
        //{
        //    connection = new Http(Convert.ToInt32(port), address);
        //    commandString = "";
        //}

        /// <summary>
        ///     Constructor of a boxee object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="address">IP-adress of the device</param>
        ///     <param name="port">Port of the device</param>
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
