using System;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Abstrakte Klasse deren Implementierungen Verbindungen zu den Geräte bereitstellen.
    ///     @author Florian Kinn
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        ///     Konstruktor für Connection.
        ///     <param name="port">Portnummer.</param>
        ///     <param name="ip">Ip des Geräts.</param>
        /// </summary>
        protected Connection(int port, String ip)
        {
            Port = port;
            Ip = ip;
        }

        /// <summary>
        ///     Der Port der Verbindung
        ///     Mit der "set"-Methode kann der Port gesetzt werden
        ///     Mit der "get"-Methode kann der Port zurückgegeben werden.
        /// </summary>
        /// <returns>Gibt den Port der Verbindung zurück</returns>
        public int Port { get; set; }

        /// <summary>
        ///     Die IP der Verbindung
        ///     Mit der "set"-Methode kann die IP gesetzt werden
        ///     Mit der "get"-Methode kann die IP zurückgegeben werden.
        /// </summary>
        /// <returns>Gibt die IP der Verbindung zurück</returns>
        public String Ip { get; set; }

        /// <summary>
        ///     Sendet den Befehl auf die implementierte Art und Weise.
        ///     <param name="command">Befehl der gesendet werden soll</param>
        ///     <returns>Rückgabewert des Gerätes</returns>
        /// </summary>
        public virtual String Send(String command)
        {
            throw new NotImplementedException();
        }
    }
}