using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Bietet die Möglichkeit über eine HTTP-Schnittstelle Befehle zu verschicken
    ///     @author Florian Kinn
    /// </summary>
    public class Http : Connection
    {
        /// <summary>
        ///     Konstruktor eines Http-Objektes.
        ///     <param name="port">Port des Gerätes</param>
        ///     <param name="ip">Ip-Adresse des Gerätes</param>
        /// </summary>
        public Http(int port, String ip)
            : base(port, ip)
        {
        }

        /// <summary>
        ///     Sendet den Befehl über eine HTTP-Schnittstelle
        /// </summary>
        /// <param name="command">Befehl der gesendet werden soll</param>
        /// <returns>Rückgabewert des Gerätes</returns>
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
                return "HTTP Verbindung konnte nicht hergestellt werden.";
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
                return "HTTP Verbindung fehlgeschlagen .\n Befehl konnte nicht ausgeführt werden.";

            }

            return responseString;
        }
    }
}