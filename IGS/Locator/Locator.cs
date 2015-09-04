using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using IGS.Helperclasses;

namespace IGS.Server.Location
{
    public class Locator
    {
        public const int MIN_NUMBER_OF_VECTORS = 3;

        private static Ray3D[] gLines;

        /// <summary>
        ///     Constructor.\n
        /// </summary>
        public Locator()
        {
          
        }

        public Point3D getDeviceLocation(List<Point3D[]> positions)
        {

            //set new Position
            if (positions.Count < MIN_NUMBER_OF_VECTORS) // cancel if not enough vectors in list 
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);

            List<Ray3D> lines = new List<Ray3D>();

            foreach (Point3D[] p in positions)
            {
                // pointing ray goes from p[0] to p[1]
                Ray3D line = new Ray3D(p[0], p[1]);

                lines.Add(line);
            }
            //Line3D.updateWeight(lines);

            return Locator.cobylaCentralPoint(lines.ToArray());

        }

        /// <summary>
        ///     Finds central point of all lines with COBYLA
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Point3D cobylaCentralPoint(Ray3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Point3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobyla(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            Point3D point = new Point3D(x[0], x[1], x[2]);

            for (int i = 0; i < gLines.Length; i++)
            {
                // sum over squared distance
                sum += (gLines[i].nearestPoint(point) - point).LengthSquared;
            }
            f = sum;

            //implicit requirement con[] >= 0 for all constrains

            con[0] = 30 - Math.Abs(x[0]);
            con[1] = 30 - Math.Abs(x[1]);
            con[2] = 30 - Math.Abs(x[2]);
        }

        /// <summary>
        ///     Finds central point of all lines with COBYLA using weight
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Point3D cobylaCentralPointWithWeight(Ray3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Point3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobylaWeight(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            Point3D point = new Point3D(x[0], x[1], x[2]);

            for (int i = 0; i < gLines.Length; i++)
            {
                // sum over weighted squared distance
                sum += (gLines[i].nearestPoint(point) - point).LengthSquared * gLines[i].weight;
            }
            f = sum;

            //implicit requirement con[] >= 0 for all constrains

            con[0] = 30 - Math.Abs(x[0]);
            con[1] = 30 - Math.Abs(x[1]);
            con[2] = 30 - Math.Abs(x[2]);
        }

        /// <summary>
        ///     Finds central point of all lines with BOBYQA
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Point3D bobyqaCentralPoint(Ray3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqa, 3, xyz, lBounds, uBounds, -1, -1, -1, 0, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Point3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqa(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            Point3D point = new Point3D(x[0], x[1], x[2]);

            for (int i = 0; i < gLines.Length; i++)
            {
                // sum over squared distance
                sum += (gLines[i].nearestPoint(point) - point).LengthSquared;
            }
            return sum;
        }

        /// <summary>
        ///     Finds central point of all lines with BOBYQA using weight
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Point3D bobyqaCentralPointWithWeight(Ray3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqaWeight, 3, xyz, lBounds, uBounds, -1, -1, -1, 1, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Point3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqaWeight(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            Point3D point = new Point3D(x[0], x[1], x[2]);

            for (int i = 0; i < gLines.Length; i++)
            {
                // sum over weighted squared distance
                sum += (gLines[i].nearestPoint(point) - point).LengthSquared * gLines[i].weight;
            }
            return sum;
        }

        /// <summary>
        ///     Updates the weight of all Lines in list
        ///     <param name="list">list of lines</param>
        /// </summary>
        private static void updateWeight(List<Ray3D> list)
        {
            DateTime currentTime = DateTime.Now;
            //Console.Out.WriteLine("currentTime:"+currentTime.ToString());

            List<Ray3D> noWeight = new List<Ray3D>();

            foreach (Ray3D line in list)
            {
                if (currentTime.Subtract(line.creationTime).TotalMinutes <= 15)
                { line.weight = 1; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 30)
                { line.weight = 0.95; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 45)
                { line.weight = 0.9; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 1)
                { line.weight = 0.85; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 2)
                { line.weight = 0.8; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 6)
                { line.weight = 0.75; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 12)
                { line.weight = 0.7; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 1)
                { line.weight = 0.65; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 3)
                { line.weight = 0.6; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 7)
                { line.weight = 0.55; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 30)
                { line.weight = 0.5; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 90)
                { line.weight = 0.4; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 180)
                { line.weight = 0.3; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 270)
                { line.weight = 0.2; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 360)
                { line.weight = 0.1; continue; }
                else
                {
                    line.weight = 0.0;
                    noWeight.Add(line);
                    continue;
                }
            }

            //remove all lines from list that do not have a weight
            foreach (Ray3D line in noWeight)
            {
                list.Remove(line);
            }

        }

    }
}
