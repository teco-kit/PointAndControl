using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using IGS.Server.Devices;
using System.Diagnostics;
using IGS.Helperclasses;

namespace IGS.Server.IGS
{
    /// <summary>
    ///     This class is meant to be a utility-class and is responsible for the collision detection.
    ///     @author Sven Ochs, Frederik Reiche
    /// </summary>
    public static class CollisionDetection
    {
        public class DeviceMinDistance : IComparable<DeviceMinDistance>
        {
            public Device device;
            public double minDist;

            public int DeviceMinDistance(DeviceMinDistance compareTo)
            {
                return this.minDist.CompareTo(compareTo.minDist);
            }

        }

        /// <summary>
        ///    The Methode does the collision detection
        ///    First the pointing vector is calculated.
        ///    This vector will be "extended" to a straight line. Following it will be calculated if this line hits a sphere of a device.
        ///    Hit devices will be saved in a list.
        /// </summary>
        /// <param name="devices">Devices which are available in the system</param>
        /// <param name="vectors">The position vectors of the ellbows and wrists</param>
        /// <returns>Devicelist with hit devices</returns>
        internal static List<Device> Calculate(List<Device> devices, Point3D[] vectors)
        {
            List<Device> found = new List<Device>();

            if (vectors == null)
                return found;

            List<DeviceMinDistance> minDevices = new List<DeviceMinDistance>();

            // pointing ray goes from vectors[0] to vectors[1]
            Ray3D pointer = new Ray3D(vectors[0], vectors[1]);

            foreach (Device dev in devices)
            {
                DeviceMinDistance minDevice = new DeviceMinDistance();
                minDevice.device = dev;
                minDevice.minDist = -1;

                foreach (Ball ball in dev.Form)
                {
                    // check if Ball is in front of pointing ray
                    if (Vector3D.AngleBetween(pointer.direction, ball.Centre - pointer.origin) < 90)
                    {
                        double distance = Point3D.Subtract(pointer.nearestPoint(ball.Centre), ball.Centre).Length;

                        // check distance
                        if (distance <= ball.Radius && (minDevice.minDist < 0 || minDevice.minDist > distance))
                            minDevice.minDist = distance;
                    }
                }

                if (minDevice.minDist >= 0)
                    minDevices.Add(minDevice);
            }

            // add found devices in order
            minDevices.Sort();
            foreach (DeviceMinDistance dev in minDevices)
            {
                found.Add(dev.device); 
            }
            return found;
        }

        
        internal static Device CalculateClosestMatch(List<Device> devices, Point3D[] vectors)
        {
            Device currDev = null;
            double currDist = -1;
            
            if (vectors == null)
                return null;

            // pointing ray goes from vectors[0] to vectors[1]
            Ray3D pointer = new Ray3D(vectors[0], vectors[1]);

            Double tempDist;

            foreach (Device dev in devices)
            {
                foreach (Ball ball in dev.Form)
                {
                    // check if Ball is in front of pointing ray
                    if (Vector3D.AngleBetween(pointer.direction, ball.Centre - pointer.origin) < 90)
                    {
                        // check distance
                        tempDist = Point3D.Subtract(pointer.nearestPoint(ball.Centre), ball.Centre).Length;
                        if (currDist == -1 || tempDist < currDist)
                        {
                            currDev = dev;
                            currDist = tempDist;
                        }
                    }
                }
            }

            return currDev;
        }

    }
}