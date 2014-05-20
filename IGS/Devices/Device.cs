using System;
using System.Collections.Generic;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Die Klasse representiert ein Gerät, das einen Namen, eine Form aus Bällen und eine noch weiter zu definierende Transmit Methode hat.
    ///     Die Geräteklasse wird durch weiterdefiniertere Geräte definiert.
    ///     @author Florian Kinn
    /// </summary>
    public abstract class Device
    {
        /// <summary>
        ///     Konstruktor für Device.
        ///     <param name="name">Name des Geräts.</param>
        ///     <param name="id">Id des Geräts.</param>
        ///     <param name="form">Die Form des Geräts.</param>
        /// </summary>
        protected Device(String name, String id, List<Ball> form)
        {
            Name = name;
            Id = id;
            Form = form;
        }

        /// <summary>
        ///     Der Name des Geräts.
        ///     Mit der "set"-Methode kann der Name gesetzt werden.
        ///     Mit der "get"-Methode kann der Name zurückgegeben werden.
        ///     <returns>Gibt den Namen des Geräts zurück.</returns>
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        ///     Die Form des Geräts wird durch einen oder mehrere Ballkörper,
        ///     welche in einer Liste gespeichert werden, dargestellt.
        ///     Mit der "set"-Methode kann die Liste gesetzt werden.
        ///     Mit der "get"-Methode kann die Liste zurückgegeben werden.
        ///     <returns>Gibt die Liste der Bälle zurück.</returns>
        /// </summary>
        public List<Ball> Form { get; set; }

        /// <summary>
        ///     Die Id des Devices.
        ///     Mit der "set"-Methode kann die ID gesetzt werden.
        ///     Mit der "get"-Methode kann die ID zurückgegeben werden.
        /// </summary>
        /// <returns>Gibt die Id des Devices zurück.</returns>
        public String Id { get; set; }

        public String CommandString { get; set; }

        /// <summary>
        ///     Die Transmit-Methode ist für das korrekte Übermitteln einer, durch die "commandId"
        ///     implizierte, Funktion an des Geräts zuständig.
        ///     <param name="cmdId">
        ///         Durch die commandId wird der Transmitmethode mitgeteilt,
        ///         welcher Befehl an das Gerät zum Ausführen gesendet werden soll.
        ///     </param>
        ///     <param name="value">
        ///         Der Wert, der dem Befehl zugehörig ist.
        ///     </param>
        ///     <returns>Ausführung erfolgreich.</returns>
        /// </summary>
        public abstract String Transmit(String cmdId, String value);
    }
}