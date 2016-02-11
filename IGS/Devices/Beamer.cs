using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.Server.Devices
{
    class Beamer : Device
    {
        
        private readonly String _commandString;

        /// <summary>
        ///     Constructor of a boxee object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="path">The Path to communicate with the device</param>  
        /// </summary>
        public Beamer(String name, String id, List<Ball> form, String path)
            : base(name, id, path, form)
        {
            String[] ipAndPort = splitPathToIPAndPort();
            connection = new Http(Convert.ToInt32(ipAndPort[1]), ipAndPort[0]);

            _commandString = path;
        }

        /// <summary>
        ///     The Transmit method is responsible for the correct invocation of a function of the beamer 
        ///     which is implicated by the "commandID"
        ///     <param name="cmdId">
        ///         With the commandID the Transmit-method recieves which command
        ///         should be send to the device (beamer)
        ///     </param>
        ///     <param name="value">
        ///         The value belonging to the command
        ///     </param>
        ///     <returns>
        ///     If execution was successful
        ///     </returns>
        /// </summary>
        public override String Transmit(String cmdId, String value)
        {
            String response = "";
            switch (cmdId)
            {
                case "volup":
                    response = connection.Send(_commandString + "Action(88)");
                    break;

            }
            return response;
        }

    }
}
