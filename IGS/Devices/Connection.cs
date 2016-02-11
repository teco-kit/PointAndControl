using System;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Abstract class which implements the connection to the devices
    ///     @author Florian Kinn
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        ///     Constructor of the connection
        ///     <param name="port">portnumber.</param>
        ///     <param name="ip">IP of the device.</param>
        /// </summary>
        protected Connection(int port, String ip)
        {
            Port = port;
            Ip = ip;
        }

        /// <summary>
        ///     The port of the connection.
        ///     With the "set"-method the port can be set.
        ///     With the "get"-method the port can be returned.
        ///     <returns>Returns the port</returns>
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     The IP of the connection
        ///     With the "set"-method the IP can be set.
        ///     With the "get"-method the IP can be returned.
        ///     <returns>Returns the IP</returns>
        /// </summary>
        public String Ip { get; set; }

        
        /// <summary>
        ///     Sends the command in the implemented manner.
        ///     <param name="command">command which should be send</param>
        ///     <returns>Returnvalue of the device</returns>
        /// </summary>
        public virtual String Send(String command)
        {
            throw new NotImplementedException();
        }
    }
}