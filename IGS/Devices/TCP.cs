using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Bietet die Möglichkeit über eine TCP-Schnittstelle Befehle zu verschicken
    ///     @author Florian Kinn
    /// </summary>
    public class Tcp : Connection
    {
        /// <summary>
        ///     Konstruktor eines Tcp-Objektes.
        ///     <param name="port">Port des Gerätes</param>
        ///     <param name="ip">Ip-Adresse des Gerätes</param>
        /// </summary>
        public Tcp(int port, String ip)
            : base(port, ip)
        {
        }

        /// <summary>
        ///     Sendet den Befehl über eine TCP-Schnittstelle
        /// </summary>
        /// <param name="command">Befehl der gesendet werden soll</param>
        /// <returns>Rückgabewert des Gerätes</returns>
        public override String Send(String command)
        {
            Socket socket = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse(Ip);
                socket = Connect(ipAddress, Port, 400);
                socket.ReceiveTimeout = 1000;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return "TCP Verbindung fehlgeschlagen.\n Befehl konnte nicht ausgeführt werden.";
            }
            byte[] buffer = Encoding.ASCII.GetBytes(command);

            socket.Send(buffer);

            byte[] message = new byte[1024];
            int bytesRead = socket.Receive(message);
            socket.Close();
            return Encoding.ASCII.GetString(message).Remove(bytesRead);
        }

        /// <summary>
        /// Stellt die TCP Verbindung zum Zielgerät her.
        /// </summary>
        /// <param name="ip">Die Ip des Zielgeräts</param>
        /// <param name="port">der Port der Zielgeräts</param>
        /// <param name="timeoutMSec">das Timeout des Verbindungsaufbaus</param>
        /// <returns></returns>
        private Socket Connect(IPAddress ip, int port, int timeoutMSec)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult asyncResult = socket.BeginConnect(ip, port, null, null);
            if (asyncResult.AsyncWaitHandle.WaitOne(timeoutMSec, false))
            {
                try
                {
                    socket.EndConnect(asyncResult);
                    return socket;
                }
                catch (Exception e)
                {
                    socket.Close();
                    throw e;
                }
            }
            else
            {
                socket.Close();
                throw new TimeoutException();
            }
        }
    }
}