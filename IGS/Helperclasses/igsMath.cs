using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Helperclasses
{
    static class  igsMath
    {

        public static Double l2Norm(Vector3D vec1, Vector3D vec2)
        {
            return norm(vec1, vec2, 2.0);
        }

        private static Double norm(Vector3D vec1, Vector3D vec2, double p)
        {
            Double result = 0;
            double diffX = Math.Abs(vec1.X - vec2.X);
            double diffY = Math.Abs(vec1.Y - vec2.Y);
            double diffZ = Math.Abs(vec1.Z - vec2.Z);

            if (p == 2.0)
            {
                result = Math.Sqrt(Math.Pow(diffX, p) + Math.Pow(diffY, p) + Math.Pow(diffZ, p));
            }
            else
            {
                Double pow = 1.0 / p;
                result = Math.Pow((Math.Pow(diffX, p) + Math.Pow(diffY, p) + Math.Pow(diffZ, p)), pow);
            }
            return result;
        }
    }
}
