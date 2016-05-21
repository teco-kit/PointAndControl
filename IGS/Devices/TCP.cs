using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PointAndControl.Devices
{
    /// <summary>
    ///     Provides the posibility to send commands over a TCP-interface.
    ///     @author Florian Kinn
    /// </summary>
    public class Tcp : Connection
    {
        /// <summary>
        ///     Constructor of the TCP object
        ///     <param name="port">Port of the device</param>
        ///     <param name="ip">IP-adress of the device</param>
        /// </summary>
        public Tcp(int port, String ip)
            : base(port, ip)
        {
        }

        /// <summary>
        ///     Sends a command over a TCP-Interface
        /// </summary>
        /// <param name="command">Command which should be send</param>
        /// <returns>Returnvalue of the device</returns>
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
                return Properties.Resources.TCPConnFailure;
            }
            byte[] buffer = Encoding.ASCII.GetBytes(command);

            socket.Send(buffer);

            byte[] message = new byte[1024];
            int bytesRead = socket.Receive(message);
            socket.Close();
            return Encoding.ASCII.GetString(message).Remove(bytesRead);
        }

        /// <summary>
        /// Establishes the TCP connection to the target device.
        /// </summary>
        /// <param name="ip">IP of the target device</param>
        /// <param name="port">port of the target device</param>
        /// <param name="timeoutMSec">time out of the connection establishment</param>
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