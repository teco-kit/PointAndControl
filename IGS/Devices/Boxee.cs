using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     This class specializes the device class to the class "Boxee"
    ///     It contrais all information and function which are available for a Boxee.
    ///     Follwing functions are available.
    ///     Navicagetion (4 directions)
    ///     Select
    ///     Return
    ///     Audio volume (raise, lower, mute)
    ///     Play, pause, stop
    ///     Textentry
    /// </summary>
    public class Boxee : Device
    {
        private readonly String _commandString;
        private Http _connection;

        /// <summary>
        ///     Constructor of a boxee object.
        ///     <param name="id">ID of the object for identifying it</param>
        ///     <param name="name">Userdefined name of the device</param>
        ///     <param name="form">Shape of the device in the room</param>
        ///     <param name="address">IP-adress of the device</param>
        ///     <param name="port">Port of the device</param>
        /// </summary>
        public Boxee(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), address);
            _commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/xbmcCmds/xbmcHttp?command=";
        }

        /// <summary>
        ///     The connection existing between a boxee and a server.
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
        ///     The Transmit method is responsible for the correct invocation of a function of the boxee 
        ///     which is implicated by the "commandID"
        ///     <param name="cmdId">
        ///         With the commandID the Transmit-method recieves which command
        ///         should be send to the device (boxee)
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
            String response;
            switch (cmdId)
            {
                case "volup":
                    response = _connection.Send(_commandString + "Action(88)");
                    break;

                case "voldown":
                    response = _connection.Send(_commandString + "Action(89)");
                    break;

                case "up":
                    response = _connection.Send(_commandString + "Action(3)");
                    break;

                case "right":
                    response = _connection.Send(_commandString + "Action(2)");
                    break;

                case "down":
                    response = _connection.Send(_commandString + "Action(4)");
                    break;

                case "left":
                    response = _connection.Send(_commandString + "Action(1)");
                    break;

                case "select":
                    response = _connection.Send(_commandString + "Action(7)");
                    break;

                case "mute":
                    response = _connection.Send(_commandString + "Mute()");
                    break;

                case "back":
                    response = _connection.Send(_commandString + "SendKey(275)");
                    break;

                case "play":
                    response = _connection.Send(_commandString + "Pause()");
                    break;

                case "pause":
                    response = _connection.Send(_commandString + "Pause()");
                    break;

                case "stop":
                    response = _connection.Send(_commandString + "Stop()");
                    break;

                default:
                    if (cmdId.Length == 1)
                    {
                        response = _connection.Send(_commandString + "SendKey(" + CmdIdToAscii(cmdId) + ")");
                    }
                    else
                    {
                        response = "ungueltiger Befehl";
                    }
                    break;
            }
            if (response.StartsWith("<html>\n<li>OK</html>\n")) return "True";
            return response;
        }

        /// <summary>
        ///     Creates boxee specific ASCII code
        ///     <returns>Returns the command string.</returns>
        /// </summary>
        private String CmdIdToAscii(String cmdId)
        {
            int cmdIdAscii = cmdId.ToCharArray()[0];
            cmdIdAscii += 61696;
            return cmdIdAscii.ToString();
        }
    }
}