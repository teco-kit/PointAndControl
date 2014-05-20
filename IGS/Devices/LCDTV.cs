using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     Diese Klasse spezialisiert die "Device" Klasse zu der Klasse "LCDTV".
    ///     Sie beinhaltet alle Information sowie die Funktionen, welche für einen LCDTV verfügbar sind.
    ///     Es sind folgende Funktionen verfügar:
    ///     An ("on")
    ///     Aus ("off")
    ///     Lautstärke (erhöhen ("volup"), erniedrige n("voldown"), mute ("mute"))
    ///     Quelle ("source")(VGA ("1"), RGB ("2"), DVI "3", HDMI "4", Video1 "5", Video2 "6", S-Video "7", DVD HD1 "12", DVD HD2 "14", HDMI (VESA STD) "17")
    ///     Audio Input ("audio")(Audio 1 PC ("1"), Audio 2 "2", Audio 3 "3", HDMI "4")
    ///     @author Christopher Baumgärtner
    /// </summary>
    public class NECLCDmonitorMultiSyncV421 : Device
    {
        /// <summary>
        ///     Konstruktor eines LCDTV-Objektes
        ///     <param name="id">ID des Objektes, um es identifizieren zu können.</param>
        ///     <param name="name">Benutzerdefinierter Name des Gerätes.</param>
        ///     <param name="form">Kugelmodell des Gerätes im Raum.</param>
        ///     <param name="address">Ip-Adresse des Gerätes</param>
        ///     <param name="port">Port des Gerätes</param>
        /// </summary>
        public NECLCDmonitorMultiSyncV421(String name, String id, List<Ball> form, String address, String port)
            : base(name, id, form)
        {
            Connection = new Tcp(Convert.ToInt32(port), address);
        }

        /// <summary>
        ///     Die Verbindung, die Zwischen einem LCDTV und dem Server besteht.
        ///     Mit der "set"-Methode kann die Verbindung gesetzt werden.
        ///     Mit der "get"-Methode kann die Verbindung zurückgegeben werden.
        ///     <returns>Gibt die Verbindung zurück</returns>
        /// </summary>
        public Tcp Connection { get; set; }

        /// <summary>
        ///     Die Transmit-Methode ist für das korrekte Übermitteln einer, durch die "commandId"
        ///     implizierte, Funktion an den LCDTV zuständig.
        ///     <param name="cmdId">
        ///         Durch die commandId wird der Transmitmethode mitgeteilt,
        ///         welcher Befehl an das Gerät(LCDTV) zum Ausführen gesendet werden soll
        ///     </param>
        ///     <param name="value">
        ///         Der Wert, der dem Befehl zugehörig ist.
        ///     </param>
        ///     <returns>Ausführung erfolgreich.</returns>
        /// </summary>
        public override String Transmit(String cmdId, String value)
        {
            switch (cmdId)
            {
                case "on":
                    return Power(0x31);
                case "off":
                    return Power(0x34);
                case "volup":
                    return Vol(1);
                case "voldown":
                    return Vol(-1);
                case "mute":
                    return Mute();
                case "source":
                    return Input((byte)(0x30 + Convert.ToInt32(value)));
                case "audio":
                    return Audio((byte)(0x30 + Convert.ToInt32(value)));
            }
            return "ungueltiger Befehl";
        }

        private String Power(byte b)
        {
            byte[] msg = new byte[21];
            byte[] message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x41, 0x30, 0x43, 0x02, 0x43, 0x32, 0x30, 0x33, 0x44, 0x36, 0x30, 0x30, 0x30, b, 0x03 }; //Message
            message.CopyTo(msg, 0);
            msg[19] = CalcBcc(msg);
            msg[20] = 0x0D;
            return Connection.Send(Encoding.ASCII.GetString(msg));
        }

        private String Vol(int i)
        {
            byte[] msg = new byte[15];
            byte[] message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x43, 0x30, 0x36, 0x02, 0x30, 0x30, 0x36, 0x32, 0x03 };

            //Message
            message.CopyTo(msg, 0);
            msg[13] = CalcBcc(msg);
            msg[14] = 0x0D;

            byte[] response;

            try
            {
                response = Encoding.ASCII.GetBytes
                    (Connection.Send(Encoding.ASCII.GetString(msg)));
            }
            catch (SocketException e)
            {
                throw e;
            }

            byte newValue = (byte)(response[23] + i);

            msg = new byte[15];
            message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x30, 0x36, newValue, 0x03 };

            //Message
            message.CopyTo(msg, 0);
            msg[13] = CalcBcc(msg);
            msg[14] = 0x0D;

            return Connection.Send(Encoding.ASCII.GetString(msg));

        }

        private String Mute()
        {
            byte[] msg = new byte[15];
            byte[] message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x43, 0x30, 0x36, 0x02, 0x30, 0x30, 0x38, 0x3D, 0x03 };

            //Message
            message.CopyTo(msg, 0);
            msg[13] = CalcBcc(msg);
            msg[14] = 0x0D;

            byte[] response;

            try
            {
                response = Encoding.ASCII.GetBytes
                    (Connection.Send(Encoding.ASCII.GetString(msg)));
            }
            catch (SocketException e)
            {
                throw e;
            }

            byte newValue = response[16];
            if (newValue == 0x31)
            {
                newValue = 0x30;
            }
            else
            {
                newValue = 0x31;
            }

            msg = new byte[19];
            message = new byte[]
                {
                    0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x30, 0x38, 0x3D, 0x30, 0x30, 0x30, newValue,
                    0x03
                }; //Message
            message.CopyTo(msg, 0);
            msg[17] = CalcBcc(msg);
            msg[18] = 0x0D;

            return Connection.Send(Encoding.ASCII.GetString(msg));
        }

        private String Input(byte b)
        {
            byte[] msg = new byte[19];
            byte[] message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x30, 0x36, 0x30, 0x30, 0x30, 0x30, b, 0x03 };

            //Message
            message.CopyTo(msg, 0);
            msg[17] = CalcBcc(msg);
            msg[18] = 0x0D;

            return Connection.Send(Encoding.ASCII.GetString(msg));
        }

        private String Audio(byte b)
        {
            byte[] msg = new byte[19];
            byte[] message = new byte[] { 0x01, 0x30, 0x41, 0x30, 0x45, 0x30, 0x3A, 0x02, 0x30, 0x32, 0x32, 0x3E, 0x30, 0x30, 0x30, b, 0x03 };

            //Message
            message.CopyTo(msg, 0);
            msg[17] = CalcBcc(msg);
            msg[18] = 0x0D;

            return Connection.Send(Encoding.ASCII.GetString(msg));
        }

        private static byte CalcBcc(byte[] command)
        {
            byte temp = command[1];
            for (int i = 2; i < command.Length; i++)
            {
                temp ^= command[i];
            }
            return temp;
        }
    }
}