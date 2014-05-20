using System;
using System.Collections.Generic;


namespace IGS.Server.Devices
{
    class Presenter : Device
    {
        
        private readonly String _commandString;
        private Http _connection;

        /// <summary>
        ///     Konstruktor eines Presenter-Objektes.
        ///     <param name="id">ID des Objektes, um es identifizieren zu können</param>
        ///     <param name="name">Benutzerdefinierter Name des Gerätes</param>
        ///     <param name="form">Kugelmodell des Gerätes im Raum</param>
        ///     <param name="address">Ip-Adresse des Gerätes</param>
        ///     <param name="port">Port des Gerätes</param>
        /// </summary>
        public Presenter (String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), address);
            _commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/?key=";
        }

        /// <summary>
        ///     Die Verbindung, die Zwischen einem Presenter und dem Server besteht.
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
        ///         welcher Befehl an das Gerät (Presenter) gesendet werden soll.
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
                case "left":
                    response = _connection.Send(_commandString + "left");
                    break;
                case "right":
                    response = _connection.Send(_commandString + "right");
                    break;
                case "return":
                    response = _connection.Send(_commandString + "ret");
                    break;
                case "f5":
                    response = _connection.Send(_commandString + "f5");
                    break; 
            }
            if (response.StartsWith("<html><body><h1>")) return "True";
            return response;
        }

        
    }
 }

