using System;
using System.Collections.Generic;


namespace IGS.Server.Devices
{
    class Presenter : Device
    {
        
        private readonly String _commandString;




        /// <summary>
        ///     Constructor of a boxee object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="path">The Path to communicate with the device</param>  
        /// </summary>
        public Presenter(String name, String id, List<Ball> form, String path)
            : base(name, id, path, form)
        {
            String[] ipAndPort = splitPathToIPAndPort();
            connection = new Http(Convert.ToInt32(ipAndPort[1]), ipAndPort[0]);

            _commandString = putPrefixHTTP(path);
        }

        /// <summary>
        ///     The Transmit method is responsible for the correct invocation of a function of the presenter 
        ///     which is implicated by the "commandID"
        ///     <param name="cmdId">
        ///         With the commandID the Transmit-method recieves which command
        ///         should be send to the device (presenter)
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
                case "left":
                    response = connection.Send(_commandString + "left");
                    break;
                case "right":
                    response = connection.Send(_commandString + "right");
                    break;
                case "return":
                    response = connection.Send(_commandString + "ret");
                    break;
                case "f5":
                    response = connection.Send(_commandString + "f5");
                    break; 
            }
            if (response.StartsWith("<html><body><h1>")) return "True";
            return response;
        }
    }
 }

