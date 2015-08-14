using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IGS.Server.Location
{
    public class Locator
    {
        public const int MIN_NUMBER_OF_VECTORS = 3;

        private static Line3D[] gLines;

        /// <summary>
        ///     Constructor.\n
        /// </summary>
        public Locator()
        {
          
        }

 
        public Vector3D setDeviceLocation(List<Vector3D[]> positions)
        {

            //set new Position
            if (positions.Count >= MIN_NUMBER_OF_VECTORS) // if enough vectors in list to calculate Position
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);

            List<Line3D> lines = new List<Line3D>();

            foreach (Vector3D[] v in positions)
            {
                Vector3D rightDir = Vector3D.Subtract(v[3], v[2]);
                Vector3D rightWrist = v[3];

                Line3D line = new Line3D(rightWrist, rightDir);

                lines.Add(line);
            }
            //Line3D.updateWeight(lines);

            return Locator.cobylaCentralPoint(lines.ToArray());

        }


        ///TODO: move geometric calculation functions to respective classes

        /// <summary>
        ///     Calculates distance between line and point
        ///     <param name="line">line</param>
        ///     <param name="point">point</param>
        ///     <returns>Returns distance</returns>
        /// </summary>
        private static double distanceBetweenLineAndPoint(Line3D line, Vector3D point)
        {
            //shortes distance from point to line: d = |(point - base) x dir| / |dir|
            return Vector3D.Divide(Vector3D.CrossProduct(Vector3D.Subtract(point, line.Base), line.Direction), line.Direction.Length).Length;
        }

        /// <summary>
        ///     Calculates distance between line and point
        ///     <param name="line">line</param>
        ///     <param name="x">point.x</param>
        ///     <param name="y">point.y</param>
        ///     <param name="z">point.z</param>
        ///     <returns>Returns distance</returns>
        /// </summary>
        private static double distanceBetweenLineAndPoint(Line3D line, Double x, Double y, Double z)
        {
            return Vector3D.Divide(Vector3D.CrossProduct(Vector3D.Subtract(new Vector3D(x, y, z), line.Base), line.Direction), line.Direction.Length).Length;
        }

        /// <summary>
        ///     Finds central point of all lines with COBYLA
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D cobylaCentralPoint(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobyla(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2); //* gLines[i].Weight;
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
        private static Vector3D cobylaCentralPointWithWeight(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobylaWeight(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2) * gLines[i].Weight;
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
        private static Vector3D bobyqaCentralPoint(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqa, 3, xyz, lBounds, uBounds, -1, -1, -1, 0, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqa(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2); //* gLines[i].Weight;
            }
            return sum;
        }

        /// <summary>
        ///     Finds central point of all lines with BOBYQA using weight
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D bobyqaCentralPointWithWeight(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqaWeight, 3, xyz, lBounds, uBounds, -1, -1, -1, 1, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqaWeight(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2) * gLines[i].Weight;
            }
            return sum;
        }

        /// <summary>
        ///     Updates the weight of all Lines in list
        ///     <param name="list">list of lines</param>
        /// </summary>
        private static void updateWeight(List<Line3D> list)
        {
            DateTime currentTime = DateTime.Now;
            //Console.Out.WriteLine("currentTime:"+currentTime.ToString());

            List<Line3D> noWeight = new List<Line3D>();

            foreach (Line3D line in list)
            {
                if (currentTime.Subtract(line.creationTime).TotalMinutes <= 15)
                { line.Weight = 1; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 30)
                { line.Weight = 0.95; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 45)
                { line.Weight = 0.9; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 1)
                { line.Weight = 0.85; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 2)
                { line.Weight = 0.8; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 6)
                { line.Weight = 0.75; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 12)
                { line.Weight = 0.7; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 1)
                { line.Weight = 0.65; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 3)
                { line.Weight = 0.6; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 7)
                { line.Weight = 0.55; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 30)
                { line.Weight = 0.5; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 90)
                { line.Weight = 0.4; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 180)
                { line.Weight = 0.3; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 270)
                { line.Weight = 0.2; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 360)
                { line.Weight = 0.1; continue; }
                else
                {
                    line.Weight = 0.0;
                    noWeight.Add(line);
                    continue;
                }
            }

            //remove all lines from list that do not have a weight
            foreach (Line3D line in noWeight)
            {
                list.Remove(line);
            }

        }

    }
}
