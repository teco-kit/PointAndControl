using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Diese Klasse spezialisiert die "Device" Klasse zu der Klasse "Plugwise".
    ///     Sie beinhaltet alle Information sowie die Funktionen, welche für ein Plugwise verfügbar sind.
    ///     Es sind folgende Funktionen verfügar:
    ///     An ("on")
    ///     Aus ("off")
    ///     @author Florian Kinn
    /// </summary>
    public class Plugwise : Device
    {
        private readonly String _commandString = "http://cumulus.teco.edu:5000/plugwise/";
        private readonly Http _connection;
        private readonly String[] adresses;

        /// <summary>
        ///     Konstruktor eines Plugwise-Objektes
        /// </summary>
        /// <param name="id">ID des Objektes, um es identifizieren 	zu können</param>
        /// <param name="name">Benutzerdefinierter Name des 	Gerätes</param>
        /// <param name="form">Kugelmodell des Gerätes im 	Raum</param>
        /// <param name="adress">die Adresse</param>
        /// <param name="port">der Port</param>
        public Plugwise(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            _connection = new Http(Convert.ToInt32(port), "127.0.0.1");
            _commandString += address;

            CommandString = _commandString;
        }

        /// <summary>
        ///     Die Transmit-Methode ist für das korrekte Übermitteln einer, durch die "commandId"
        ///     implizierte, Funktion an das Plugwise zuständig.
        /// </summary>
        /// <param name="cmdId">
        ///     Durch die commandId wird der Transmitmethode mitgeteilt,
        ///     welcher Befehl an das Gerät(Plugwise) zum Ausführen gesendet werden soll
        /// </param>
        /// <param name="value">
        ///     Wert der übergeben werden möchte für z.b. Lautstärke.
        /// </param>
        /// <returns>Ausführung erfolgreich</returns>
        public override String Transmit(String cmdId, String value)
        {
            String response = "ungueltiger Befehl";
            Console.WriteLine(CommandString);
            switch (cmdId)
            {

                case "on":
                    if (_connection.Send(CommandString + "/on").StartsWith("{\n  \"plugwise\": {\n    \"state\":"))
                        response = "true";
                    break;
                case "off":
                    if (_connection.Send(CommandString + "/off").StartsWith("{\n  \"plugwise\": {\n    \"state\":"))
                        response = "true";
                    break;
            }
            return response;
        }
    }
}