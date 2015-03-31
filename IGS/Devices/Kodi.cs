using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace IGS.Server.Devices
{
    class Kodi : Device
    {
            // Dll Import to nudge mouse
            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            internal extern static Int32 SetCursorPos(Int32 x, Int32 y);

            private Http _connection;
            private readonly String _commandString;
            private readonly String _absolutePathToKodi = "C:\\Program Files (x86)\\Kodi\\Kodi.exe"; // this is not nice;

            /// <summary>
            ///     Constructor of a XBMC object.
            ///     <param name="id">ID of the object for identifying it</param>
            ///     <param name="name">Userdefined name of the device</param>
            ///     <param name="form">Shape of the device in the room</param>
            ///     <param name="address">IP-adress of the device</param>
            ///     <param name="port">Port of the device</param>
            /// </summary>
            public Kodi(String name, String id, List<Ball> form,String address,String port)
                : base(name, id, form)
            {
                this._connection = new Http(Convert.ToInt32(port), address);
                this._commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/jsonrpc?request=";
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
                if (cmdId == "on")
                {
                    Process p = Process.Start(_absolutePathToKodi);
                    if (p != null)
                    {
                        // wait a second before deactivating the screensaver
                        System.Timers.Timer aTimer = new System.Timers.Timer();
                        aTimer.Elapsed += new ElapsedEventHandler(DisableScreensaver);
                        aTimer.Interval = 1000;
                        aTimer.AutoReset = false;
                        aTimer.Enabled = true;

                        return "True";
                    }
                    else
                        return "False";
                }

                String response = "";
                String action = "";                
                switch (cmdId)
                {
                     case "up":
                        action = "up";
                        break;
                    case "right":
                        action = "right";
                        break;
                    case "down":
                        action = "down";
                        break;
                    case "left":
                        action = "left";
                        break;
                    case "volup":
                        action = "volumeup";
                        break;
                    case "voldown":
                        action = "volumedown";
                        break;
                    case "mute":
                        action = "mute";
                        break;
                    case "select":
                        action = "select";
                        break;
                    case "back":
                        action = "back";
                        break;
                    case "play":
                        action = "play";
                        break;
                    case "pause":
                        action = "pause";
                        break;
                    case "stop":
                        action = "stop";
                        break;
                    case "next":
                        action = "skipnext";
                        break;
                    case "prev":
                        action = "skipprevious";
                        break;
                } 
                if (action != "")
                    response = _connection.Send(_commandString +
                        "{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Input.ExecuteAction\",\"params\":{\"action\":\"" + action + "\"}}");

                if (cmdId == "off")
                    response = _connection.Send(_commandString +
                        "{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Application.Quit\"}");

                //ugly hack for keyboard input
                if (cmdId.Length == 1)
                    response = _connection.Send(_commandString + 
                        "{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Input.SendText\",\"params\":{\"text\":\"" + cmdId + "\"}}");
                else
                    response = "False";


                if (response.Contains("\"result\":\"OK\"")) return "True";
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

            private static void DisableScreensaver(object source, ElapsedEventArgs e)
            {
                // Stops the screen saver by moving the cursor.
                SetCursorPos(new Random().Next(100), new Random().Next(100));
            }
        }
    }

