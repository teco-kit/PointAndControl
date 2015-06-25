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
        private static double _maxX;
        private static double _maxY;
        private static double _maxZ;

        public struct minDistForDev
        {
            public Device dev;
            public double minDist;
        }
        /// <summary>
        ///    The Methode does the collision detection
        ///    First the pointing vector of the underarm is calculated.
        ///    This vector will be "extendet" to a straight line. Following it will be calculated if this line hits a sphere of a device.
        ///    Hit devices will be saved in a list.
        /// </summary>
        /// <param name="devices">Devices which are available in the system</param>
        /// <param name="vectors">The position vectors of the ellbows and wrists</param>
        /// <returns>Devicelist with hit devices</returns>
        internal static List<Device> Calculate(List<Device> devices, Vector3D[] vectors)
        {
            GetMax(devices, vectors);

            List<Device> found = new List<Device>();

            if (vectors == null)
                return found;

            //Wrist - ellbow (direction vector of the line)
            Vector3D leftForearm = Vector3D.Subtract(vectors[1], vectors[0]);
            Vector3D rightForearm = Vector3D.Subtract(vectors[3], vectors[2]);

            Debug.WriteLine(vectors[3].ToString());

            Vector3D curr;


            foreach (Device dev in devices)
            {
                foreach (Ball ball in dev.Form)
                {
                    curr = vectors[3];
                    while ((Math.Abs(curr.X) < _maxX) && (Math.Abs(curr.Y) < _maxY) && (Math.Abs(curr.Z) < _maxZ))
                    {
                        curr = Vector3D.Add(rightForearm, curr);
                        if (Vector3D.Subtract(curr, ball.Centre).Length <= ball.Radius)
                        {
                            if (!found.Contains(dev))
                            {
                                found.Add(dev);
                            }
                        }
                    }
                }
            }

            curr = vectors[1];
            foreach (Device dev in devices)
            {
                minDistForDev tmpEntry = new minDistForDev();
                tmpEntry.dev = dev;



                foreach (Ball ball in dev.Form)
                {
                    while ((Math.Abs(curr.X) < _maxX) && (Math.Abs(curr.Y) < _maxY) && (Math.Abs(curr.Z) < _maxZ))
                    {
                        curr = Vector3D.Add(leftForearm, curr);
                        if (Vector3D.Subtract(curr, ball.Centre).Length <= ball.Radius)
                        {
                            if (!found.Contains(dev))
                            {
                                found.Add(dev);
                            }
                        }
                    }
                }
            }
            return found;
        }



        internal static List<minDistForDev> CalculateMinDist(List<Device> devices, Vector3D[] vectors)
        {
            GetMax(devices, vectors);
            //double stepSize = 1.0;
            List<minDistForDev> distances = new List<minDistForDev>();

            if (vectors == null)
                return distances;

            //Wrist - ellbow (direction vector of the line)
            Vector3D leftForearm = Vector3D.Subtract(vectors[1], vectors[0]);
            Vector3D rightForearm = Vector3D.Subtract(vectors[3], vectors[2]);

            Debug.WriteLine(vectors[3].ToString());

            Vector3D curr;


            foreach (Device dev in devices)
            {

                minDistForDev entry = new minDistForDev();
                entry.dev = dev;
                entry.minDist = double.MaxValue;
                foreach (Ball ball in dev.Form)
                {
                   // int iterator = 1;
                    curr = vectors[3];
                    while ((Math.Abs(curr.X) <= _maxX) && (Math.Abs(curr.Y) <= _maxY) && (Math.Abs(curr.Z) <= _maxZ))
                    {
                        //curr = Vector3D.Add(rightForearm, Vector3D.Multiply(iterator, Vector3D.Multiply(stepSize, rightForearm)));
                        curr = Vector3D.Add(rightForearm, curr);
                        double tmpDist = igsMath.l2Norm(curr, ball.Centre);
                        //double tmpDist = Vector3D.Subtract(curr, ball.Centre).Length;
                        if (tmpDist <= entry.minDist)
                        {
                            entry.minDist = tmpDist;

                        }
                    }
                }

                distances.Add(entry);

            }


            return distances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="vectors"></param>
        private static void GetMax(IEnumerable<Device> devices, Vector3D[] vectors)
        {
            foreach (Device dev in devices)
            {
                foreach (Ball ball in dev.Form)
                {
                    foreach (Vector3D vector in vectors)
                    {
                        _maxX = Math.Max(Math.Max(_maxX, Math.Abs(ball.Centre.X)), vector.X);
                        _maxY = Math.Max(Math.Max(_maxY, Math.Abs(ball.Centre.Y)), vector.Y);
                        _maxZ = Math.Max(Math.Max(_maxZ, Math.Abs(ball.Centre.Z)), vector.Z);
                    }
                }
            }
        }


        public static String getNameOfDeviceWithMinDist(List<Device> devices, Vector3D[] vectors)
        {
            List<minDistForDev> distances = CalculateMinDist(devices, vectors);
            minDistForDev min = new minDistForDev();
            min.minDist = double.MaxValue;

            foreach (minDistForDev mdd in distances)
            {
                if (min.minDist > mdd.minDist)
                {
                    min = mdd;
                }
            }

            return min.dev.Id;
        }
    }
}