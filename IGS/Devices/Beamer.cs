using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.Server.Devices
{
    class Beamer : Device
    {
        private Http _connection;
        private readonly String _commandString;

        /// <summary>
        ///     Konstruktor eines Beamer-Objektes.
        ///     <param name="id">ID des Objektes, um es identifizieren zu können</param>
        ///     <param name="name">Benutzerdefinierter Name des Gerätes</param>
        ///     <param name="form">Kugelmodell des Gerätes im Raum</param>
        ///     <param name="address">Ip-Adresse des Gerätes</param>
        ///     <param name="port">Port des Gerätes</param>
        /// </summary>
        public Beamer(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), address);
           
        }

        /// <summary>
        ///     Die Verbindung, die Zwischen einem Boxee und dem Server besteht.
        ///     Mit der "set"-Methode kann die Verbindung gesetzt werden.
        ///     Mit der "get"-Methode kann die Verbindung zurückgegeben werden.
        ///     <returns>Gibt die Verbindung zurück</returns>
        /// </summary>
        public Http Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        ///     Die Transmit-Methode ist für das korrekte Aufrufen einer, durch die "commandId"
        ///     implizierte, Funktion an den Boxee zuständig.
        ///     <param name="cmdId">
        ///         Durch die commandId wird der Transmitmethode mitgeteilt,
        ///         welcher Befehl an das Gerät (Boxee) gesendet werden soll.
        ///     </param>
        ///     <param name="value">
        ///         Der Wert, der dem Befehl zugehörig ist.
        ///     </param>
        ///     <returns>Ausführung erfolgreich</returns>
        /// </summary>
        public override String Transmit(String cmdId, String value)
        {
            String response = "";
            switch (cmdId)
            {
                case "volup":
                    response = _connection.Send(_commandString + "Action(88)");
                    break;

            }

            return response;
        }

    }
}
