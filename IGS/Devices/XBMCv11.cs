using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    class XBMCv11 : Device
    {
        
            private Http _connection;
            private readonly String _commandString;

            /// <summary>
            ///     Constructor of a XBMC object.
            ///     <param name="id">ID of the object for identifying it</param>
            ///     <param name="name">Userdefined name of the device</param>
            ///     <param name="form">Shape of the device in the room</param>
            ///     <param name="address">IP-adress of the device</param>
            ///     <param name="port">Port of the device</param>
            /// </summary>
            public XBMCv11(String name, String id, List<Ball> form,String address,String port)
                : base(name, id, form)
            {
                this._connection = new Http(Convert.ToInt32(port), address);
                this._commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/xbmcCmds/xbmcHttp?command=";
            }




            /// <summary>
            ///     The Transmit method is responsible for the correct invocation of a function of the XBMC
            ///     which is implicated by the "commandID"
            ///     <param name="cmdId">
            ///         With the commandID the Transmit-method recieves which command
            ///         should be send to the device (XBMC)
            ///     </param>
            ///     <param name="value">
            ///         The value belonging to the command
            ///     </param>
            ///     <returns>
            ///     If execution was successful
            ///     </returns>
            /// </summary>
            public override String Transmit(String cmdId,String value)
            {
                String response = "";
                switch (cmdId)
                {
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
                    case "volup":
                        response = _connection.Send(_commandString + "Action(88)");
                        break;
                    case "voldown":
                        response = _connection.Send(_commandString + "Action(89)");
                        break;
                    case "mute":
                        response = _connection.Send(_commandString + "Mute()");
                        break;
                    case "select":
                        response = _connection.Send(_commandString + "Action(7)");
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
                    case "next":
                        response = _connection.Send(_commandString + "PlayNext()");
                        break;
                    case "prev":
                        response = _connection.Send(_commandString + "PlayPrev()");
                        break;
                }
                if (response.StartsWith("<html>\n<li>OK</html>\n")) return "True";
                return response;
            }

            /// <summary>
            ///     The connection existing between a XBMC and a server.
            ///     With the "set"-method the connection can be set.
            ///     With the "get"-method the connection can be returned.
            ///     <returns>Returns the connection</returns>
            /// </summary>
            public Http Connection
            {
                get { return _connection; }
                set { _connection = value; }
            }

            private string cmdIdToAscii(string cmdId)
            {
                int cmdIdAscii = (int)cmdId.ToCharArray()[0];
                cmdIdAscii += 61696;
                return cmdIdAscii.ToString();
            }
        }

    }

