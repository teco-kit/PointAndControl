using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    class XBMCv11 : Device
    {
        
            
            private readonly String _commandString;

            /// <summary>
            ///     Constructor of a XBMC object.
            ///     <param name="id">ID of the object for identifying it</param>
            ///     <param name="name">Userdefined name of the device</param>
            ///     <param name="form">Shape of the device in the room</param>
            ///     <param name="address">IP-adress of the device</param>
            ///     <param name="port">Port of the device</param>
            /// </summary>
            //public XBMCv11(String name, String id, List<Ball> form,String address,String port)
            //    : base(name, id, form)
            //{
            //    connection = new Http(Convert.ToInt32(port), address);
            //    this._commandString = "http://" + connection.Ip + ":" + connection.Port + "/xbmcCmds/xbmcHttp?command=";
            //}

            /// <summary>
            ///     Constructor of a boxee object.
            ///     <param name="id">ID of the object for identifying it</param>
            ///     <param name="name">Userdefined name of the device</param>
            ///     <param name="form">Shape of the device in the room</param>
            ///     <param name="address">IP-adress of the device</param>
            ///     <param name="port">Port of the device</param>
            /// </summary>
            public XBMCv11(String name, String id, List<Ball> form, String path)
                : base(name, id, path, form)
            {
                String[] ipAndPort = splitPathToIPAndPort();
                connection = new Http(Convert.ToInt32(ipAndPort[1]), ipAndPort[0]);

                _commandString = path;
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
                        response = connection.Send(_commandString + "Action(3)");
                        break;
                    case "right":
                        response = connection.Send(_commandString + "Action(2)");
                        break;
                    case "down":
                        response = connection.Send(_commandString + "Action(4)");
                        break;
                    case "left":
                        response = connection.Send(_commandString + "Action(1)");
                        break;
                    case "volup":
                        response = connection.Send(_commandString + "Action(88)");
                        break;
                    case "voldown":
                        response = connection.Send(_commandString + "Action(89)");
                        break;
                    case "mute":
                        response = connection.Send(_commandString + "Mute()");
                        break;
                    case "select":
                        response = connection.Send(_commandString + "Action(7)");
                        break;
                    case "back":
                        response = connection.Send(_commandString + "SendKey(275)");
                        break;
                    case "play":
                        response = connection.Send(_commandString + "Pause()");
                        break;
                    case "pause":
                        response = connection.Send(_commandString + "Pause()");
                        break;
                    case "stop":
                        response = connection.Send(_commandString + "Stop()");
                        break;
                    case "next":
                        response = connection.Send(_commandString + "PlayNext()");
                        break;
                    case "prev":
                        response = connection.Send(_commandString + "PlayPrev()");
                        break;
                }
                if (response.StartsWith("<html>\n<li>OK</html>\n")) return "True";
                return response;
            }



            private string cmdIdToAscii(string cmdId)
            {
                int cmdIdAscii = (int)cmdId.ToCharArray()[0];
                cmdIdAscii += 61696;
                return cmdIdAscii.ToString();
            }
        }
    }

