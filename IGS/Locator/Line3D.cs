using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Server.Location
{
    public class Line3D
    {
        /// <summary>
        ///     Creates a new Line
        ///     <param name="vectorBase">base vector of line.</param>
        ///     <param name="vectorDirection">direction vector of line.</param>
        /// </summary>
        public Line3D(Vector3D vectorBase, Vector3D vectorDirection)
        {
            Base = vectorBase;
            Direction = vectorDirection;
            Weight = 1;
            creationTime = DateTime.Now;
        }

        /// <summary>
        ///     get and set for base vector of line
        /// </summary>
        public Vector3D Base { get; set; }

        /// <summary>
        ///     get and set for direction vector of line
        /// </summary>
        public Vector3D Direction { get; set; }

        /// <summary>
        ///     get and set for weight of line
        /// </summary>
        public Double Weight { get; set; } // 0 <= weight <= 1, 1 representing maximum weight

        /// <summary>
        ///     get and set for creation time of line
        /// </summary>
        public DateTime creationTime { get; set; } //time when the line was created

        /// <summary>
        ///     toString
        ///     <returns> Line3D to String</returns>
        /// </summary>
        public override String ToString()
        {
            return "Line: Base[" + Base.ToString() + "] Dir[" + Direction.ToString() + "] Weight[" + Weight + "] Time[" + creationTime + "]";
        }


    }

}
