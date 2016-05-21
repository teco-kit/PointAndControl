using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Helperclasses
{
    public class Ray3D
    {

        public Ray3D()
        {
            weight = 1;
            creationTime = DateTime.Now;
        }

        /// <summary>
        ///     Creates a new Ray with base point and vector
        ///     <param name="Base">base point of line.</param>
        ///     <param name="vectorDirection">direction vector of line.</param>
        /// </summary>
        public Ray3D(Point3D Base, Vector3D vectorDirection)
        {
            origin = Base;
            direction = vectorDirection;
            weight = 1;
            creationTime = DateTime.Now;
        }

        /// <summary>
        ///     Creates a new Ray with two points
        ///     <param name="Base">base point of line</param>
        ///     <param name="pointOnLine">second point on the line</param>
        /// </summary>
        public Ray3D(Point3D Base, Point3D pointOnLine)
        {
            origin = Base;
            direction = pointOnLine - Base;
            weight = 1;
            creationTime = DateTime.Now;
        }


        /// <summary>
        ///     get and set for base vector of line
        /// </summary>
        public Point3D origin { get; set; }

        /// <summary>
        ///     get and set for direction vector of line
        /// </summary>
        public Vector3D direction { get; set; }

        /// <summary>
        ///     get and set for weight of line
        /// </summary>
        public Double weight { get; set; } // 0 <= weight <= 1, 1 representing maximum weight

        /// <summary>
        ///     get and set for creation time of line
        /// </summary>
        public DateTime creationTime { get; set; } //time when the line was created

        /// <summary>
        ///     get nearest point on ray to a specific point
        ///     <param name="point">point</param>
        ///     <returns>neares point on ray</returns>
        /// </summary>
        public Point3D nearestPoint(Point3D point)
        {
            return origin - (direction *
                (Vector3D.DotProduct((origin - point), direction) / direction.LengthSquared));
        }

        /// <summary>
        ///     toString
        ///     <returns> Ray3D to String</returns>
        /// </summary>
        public override String ToString()
        {
            return "Ray: Origin[" + origin.ToString() + "] Dir[" + direction.ToString() + "] Weight[" + weight + "] Time[" + creationTime + "]";
        }


    }

}
