using System;
using System.Collections.Generic;

namespace PointAndControl.Devices
{
    class ExternalDevice : Device
    {
        private readonly String _commandString;
        public bool hasAssignedName { get; set; }
        /// <summary>
        ///     Constructor of a External Device object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="path">The Path to communicate with the device</param>  
        /// </summary>
        public ExternalDevice(String name, String id, String path, List<Ball> form)
            : base(name, id, path, form)
        { 
            _commandString = path;

            if (name == null || name == "" || name == id)
            {
                name = id;
                hasAssignedName = false;
            }
            else
                hasAssignedName = true;

        }

        public override string Transmit(string cmdId, string value)
        {
            throw new NotImplementedException();
        }

        public void assignName(String name)
        {
            Name = name;
            hasAssignedName = true;
        }
    }
}
