using IGS.Classifier;
using IGS.Server.Location;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace IGS.Server.Devices
{
    /// <summary>
    ///     The abstract class represents a device with a name, a shape made out of balls(spheres), 
    ///     and a transmit method which has to be specified.
    ///     @author Florian Kinn
    /// </summary>
    public abstract class Device
    {
        /// <summary>
        ///     Constructor for a device
        ///     <param name="name">name of a device.</param>
        ///     <param name="id">id of a device.</param>
        ///     <param name="form">Shape of a device.</param>
        /// </summary>
        protected Device(String name, String id, List<Ball> form)
        {
            Name = name;
            Id = id;
            Form = form;
            skelPositions = new List<Point3D[]>();
        }

        ///     Name of the device.
        ///     With the "set"-method the name can be set.
        ///     With the "get"-method the name can be returned.
        ///     <returns>Returns the name of the device</returns>
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
        ///     Returns the center of gravity of an object, computed by the
        ///     weighted mean of the form-defining spheres
        /// </summary>
        /// <returns></returns>
        public Point3D getCenterOfGravity()
        {
            
            Point3D cog = new Point3D(0,0,0);
            Double mass = 0.0d;

            foreach (Ball ball in this.Form)
            {
                cog = Point3D.Add(cog, Vector3D.Multiply((Vector3D)ball.Centre, ball.Radius));
                mass += ball.Radius;
            }

            if (this.Form.Count > 0 && mass > 0) cog = (Point3D) Vector3D.Multiply((Vector3D)cog, 1.0d/mass);

            return cog;
        }

        ///     The ID of the device.
        ///     With the "set"-method the ID of the device can be set.
        ///     With the "get"-method the ID of the device can be returned.
        ///     <returns>Returns the ID of the device</returns>
        public String Id { get; set; }

        public String CommandString { get; set; }

        public String address { get; set; }

        public String port { get; set; }

        /// <summary>
        ///     The Transmit method is responsible for the correct invocation of a function of the device
        ///     which is implicated by the "commandID"
        ///     <param name="cmdId">
        ///         With the commandID the Transmit-method recieves which command
        ///         should be send to the device 
        ///     </param>
        ///     <param name="value">
        ///         The value belonging to the command
        ///     </param>
        ///     <returns>
        ///     If execution was successful
        ///     </returns>
        public abstract String Transmit(String cmdId, String value);

        public List<Point3D[]> skelPositions { get; set; }

        public Color color { get; set; }

    }
}