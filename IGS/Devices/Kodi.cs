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

         
            private readonly String _commandString;
            public String _absolutePathToKodi { get; set; } //TODO: this is not nice;

            /// <summary>
            ///     Constructor of a boxee object.
            ///     <param name="id">ID of the object for identifying it</param>
            ///     <param name="name">Userdefined name of the device</param>
            ///     <param name="form">Shape of the device in the room</param>
            ///     <param name="path">The Path to communicate with the device</param>       
            /// </summary>
            public Kodi(String name, String id, List<Ball> form, String path)
                : base(name, id, path, form)
            {

                String[] ipAndPort = splitPathToIPAndPort();
                connection = new Http(Convert.ToInt32(ipAndPort[1]), ipAndPort[0]);

                _commandString = putPrefixHTTP(path);
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
                    response = connection.Send(_commandString +
                        "/jsonrpc?request={\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Input.ExecuteAction\",\"params\":{\"action\":\"" + action + "\"}}");

                if (cmdId == "off")
                    response = connection.Send(_commandString +
                        "/jsonrpc?request={\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Application.Quit\"}");

                //ugly hack for keyboard input
                if (cmdId.Length == 1)
                    response = connection.Send(_commandString + 
                        "/jsonrpc?request={\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"Input.SendText\",\"params\":{\"text\":\"" + cmdId + "\"}}");
                else
                    response = "False";


                if (response.Contains("\"result\":\"OK\"")) return "True";
                return response;
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

                // kill the screen saver
                foreach (var process in Process.GetProcessesByName("gPhotoShow.scr"))
                {
                    process.Kill();
                }
            }

        public void setKodiPath(String path)
        {
            _absolutePathToKodi = path;
            
        }
        }
    }

