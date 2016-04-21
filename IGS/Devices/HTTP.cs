using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace PointAndControl.Devices
{
    /// <summary>
    ///     
    ///     Bietet die Möglichkeit über eine HTTP-Schnittstelle Befehle zu verschicken
    ///     @author Florian Kinn
    /// </summary>
    public class Http : Connection
    {
        /// <summary>
        ///     Constructor of a http object
        ///     <param name="port">Port of the device</param>
        ///     <param name="ip">IP adress of the device</param>
        /// </summary>
        public Http(int port, String ip)
            : base(port, ip)
        {
        }

        /// <summary>
        ///     Sends the command over a HTTP- interface
        /// </summary>
        /// <param name="command">The command which should be send</param>
        /// <returns>Return value of the deivce</returns>
        public override String Send(String command)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(command);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return Properties.Resources.HTTPConnEstablishFailure;
            }
            request.Proxy = null;
            request.Timeout = 300;
            String responseString = "";

            // Get the response.
            try
            {
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);
                responseString = reader.ReadToEnd();
                response.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return Properties.Resources.HTTPConnFailure;

            }

            return responseString;
        }
    }
}