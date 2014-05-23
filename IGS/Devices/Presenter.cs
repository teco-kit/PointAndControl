using System;
using System.Collections.Generic;


namespace IGS.Server.Devices
{
    class Presenter : Device
    {
        
        private readonly String _commandString;
        private Http _connection;

        /// <summary>
        ///     Constructor of a presenter object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="address">IP-adress of the device</param>
        ///     <param name="port">Port of the device</param>
        /// </summary>
        public Presenter (String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), address);
            _commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/?key=";
        }

        /// <summary>
        ///     The connection existing between a presenter and a server.
        ///     With the "set"-method the connection can be set.
        ///     With the "get"-method the connection can be returned.
        ///     <returns>Returns the connection</returns>
        /// </summary>
        public Http Connection
        {
            get { return _connection; }
            set { _connection = value; }
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
                    response = _connection.Send(_commandString + "left");
                    break;
                case "right":
                    response = _connection.Send(_commandString + "right");
                    break;
                case "return":
                    response = _connection.Send(_commandString + "ret");
                    break;
                case "f5":
                    response = _connection.Send(_commandString + "f5");
                    break; 
            }
            if (response.StartsWith("<html><body><h1>")) return "True";
            return response;
        }

        
    }
 }

