using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace IGS.Helperclasses
{
    /// <summary>
    /// Represents a plane in 3D space trough normal vector and origin
    /// </summary>
    public class Plane3D
    {
        public Vector3D normal { get; set;}

        public Point3D origin { get; set;}

        public Plane3D(Point3D p0, Vector3D vec)
        {
            this.origin = p0;

            this.normal = vec;
        }

        public Point3D rayIntersection(Ray3D ray)
        {
            double div = Vector3D.DotProduct(ray.direction, this.normal);
            // check if ray and plane are parallel
            if (div == 0)
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);

            // calculate intesection point
            double a = Vector3D.DotProduct(Point3D.Subtract(this.origin, ray.origin), this.normal) / div;

            // if a < 0, ray does not intersect with plane (wrong direction)
            if (a < 0)
                return new Point3D(Double.NaN, Double.NaN, Double.NaN);
            else
                return ray.origin + a * ray.direction;
        }

    }
}
