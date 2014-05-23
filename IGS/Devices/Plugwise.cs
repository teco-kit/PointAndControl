using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     This class specializes the device class to the class plugwise
    ///     It contains all information as well as functions available for a plugwise.
    ///     Follwing functions are available:
    ///     On
    ///     Off
    ///     @author Florian Kinn
    /// </summary>
    public class Plugwise : Device
    {
        private readonly String _commandString = "http://cumulus.teco.edu:5000/plugwise/";
        private readonly Http _connection;
        private readonly String[] adresses;

        /// <summary>
        ///     Constructor of a plugwise object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="address">IP-adress of the device</param>
        ///     <param name="port">Port of the device</param>
        /// </summary>
        public Plugwise(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), "127.0.0.1");
            _commandString += address;

            CommandString = _commandString;
        }

        /// <summary>
        ///     The Transmit method is responsible for the correct invocation of a function of the plugwise 
        ///     which is implicated by the "commandID"
        ///     <param name="cmdId">
        ///         With the commandID the Transmit-method recieves which command
        ///         should be send to the device (plugwise)
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
            String response = "ungueltiger Befehl";
            Console.WriteLine(CommandString);
            switch (cmdId)
            {

                case "on":
                    if (_connection.Send(CommandString + "/on").StartsWith("{\n  \"plugwise\": {\n    \"state\":"))
                        response = "true";
                    break;
                case "off":
                    if (_connection.Send(CommandString + "/off").StartsWith("{\n  \"plugwise\": {\n    \"state\":"))
                        response = "true";
                    break;
            }
            return response;
        }
    }
}