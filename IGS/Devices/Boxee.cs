using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Diese Klasse spezialisiert die "Device" Klasse zu der Klasse "Boxee".
    ///     Sie beinhaltet alle Information sowie die Funktionen, welche für einen Boxee verfügbar sind.
    ///     Es sind folgende Funktionen verfügar:
    ///     Navigation (4 Richtungen)
    ///     Auswählen
    ///     Zurück
    ///     Lautstärke (erhöhen, erniedrigen, mute)
    ///     Play, Pause, Stop
    ///     Texteingabe
    /// </summary>
    public class Boxee : Device
    {
        private readonly String _commandString;
        private Http _connection;

        /// <summary>
        ///     Konstruktor eines Boxxee-Objektes.
        ///     <param name="id">ID des Objektes, um es identifizieren zu können</param>
        ///     <param name="name">Benutzerdefinierter Name des Gerätes</param>
        ///     <param name="form">Kugelmodell des Gerätes im Raum</param>
        ///     <param name="address">Ip-Adresse des Gerätes</param>
        ///     <param name="port">Port des Gerätes</param>
        /// </summary>
        public Boxee(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), address);
            _commandString = "http://" + _connection.Ip + ":" + _connection.Port + "/xbmcCmds/xbmcHttp?command=";
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
            String response;
            switch (cmdId)
            {
                case "volup":
                    response = _connection.Send(_commandString + "Action(88)");
                    break;

                case "voldown":
                    response = _connection.Send(_commandString + "Action(89)");
                    break;

                case "up":
                    response = _connection.Send(_commandString + "Action(3)");
                    break;

                case "right":
                    response = _connection.Send(_commandString + "Action(2)");
                    break;

                case "down":
                    response = _connection.Send(_commandString + "Action(4)");
                    break;

                case "left":
                    response = _connection.Send(_commandString + "Action(1)");
                    break;

                case "select":
                    response = _connection.Send(_commandString + "Action(7)");
                    break;

                case "mute":
                    response = _connection.Send(_commandString + "Mute()");
                    break;

                case "back":
                    response = _connection.Send(_commandString + "SendKey(275)");
                    break;

                case "play":
                    response = _connection.Send(_commandString + "Pause()");
                    break;

                case "pause":
                    response = _connection.Send(_commandString + "Pause()");
                    break;

                case "stop":
                    response = _connection.Send(_commandString + "Stop()");
                    break;

                default:
                    if (cmdId.Length == 1)
                    {
                        response = _connection.Send(_commandString + "SendKey(" + CmdIdToAscii(cmdId) + ")");
                    }
                    else
                    {
                        response = "ungueltiger Befehl";
                    }
                    break;
            }
            if (response.StartsWith("<html>\n<li>OK</html>\n")) return "True";
            return response;
        }

        /// <summary>
        ///    Erzeugt den Boxee spezifischen ASCII code.
        ///     <returns>Gibt den Kommando String zurück.</returns>
        /// </summary>
        private String CmdIdToAscii(String cmdId)
        {
            int cmdIdAscii = cmdId.ToCharArray()[0];
            cmdIdAscii += 61696;
            return cmdIdAscii.ToString();
        }
    }
}