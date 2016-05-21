using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Helperclasses
{

    /// <summary>
    /// This is a utility-class to transform coordinates from the camerar related coordinate system 
    /// into a world related one
    /// </summary>
    public class CoordTransform
    {
        /// <summary>
        /// The constructor of the Coordinate Transformer. 
        /// </summary>
        /// <param name="tiltDegree">The tilting degree the camera has</param>
        /// <param name="roomOrientation">The horizontal orientation the camera has (the direction she "looks" in the room)</param>
        /// <param name="position">the position where the camera stands in the room</param>
        public CoordTransform(double tiltDegree, double roomOrientation, Point3D position)
        {
            this.rotationMatrix = new Matrix3D();
            this.calculateRotationMatrix(tiltDegree, roomOrientation);
            this.transVector = (Vector3D)position;
        }
        /// <summary>
        /// Getter and setter of the matrix defining how the camera is placed in the room
        /// </summary>
        public Matrix3D rotationMatrix { get; set; }

        /// <summary>
        /// Getter and setter of the vector defining where the camera is placed in the room in world coordinates
        /// </summary>
        public Vector3D transVector { get; set; }


        /// <summary>
        /// Calulates a 3D matrix to rotate around the x-axis
        /// </summary>
        /// <param name="elevationAngle">the degree the camera is elevated so the rotation around the x-axis</param>
        /// <returns>a rotation matrix around the x-axis with the given angle</returns>
        public Matrix3D Rotate_X(double elevationAngle)
        {

            double cos = Math.Round(Math.Cos((360 - elevationAngle) * (Math.PI / 180.0f)), 5);
            double sin = Math.Round(Math.Sin((360 - elevationAngle) * (Math.PI / 180.0f)), 5);

            Matrix3D tmp = new Matrix3D(1, 0, 0, 0,
                                        0, cos, -sin, 0,
                                        0, sin, cos, 0,
                                        0, 0, 0, 1);
            
            return tmp;
        }

        /// <summary>
        /// Calculates a matrix to rotate around the y-axis
        /// </summary>
        /// <param name="roomOrientation">the horizontal orientation for the rotation around the y-axis</param>
        /// <returns>a rotation matrix around the y-axis with the given angle</returns>
        public Matrix3D Rotate_Y(double roomOrientation)
        {

            double cos = Math.Round(Math.Cos((360.0f - roomOrientation) * (Math.PI / 180.0f)), 5);
            double sin = Math.Round(Math.Sin((360.0f - roomOrientation) * (Math.PI / 180.0f)), 5);

            Matrix3D tmp = new Matrix3D(cos, 0, sin, 0,
                                        0,   1, 0,   0,
                                        -sin, 0, cos, 0,
                                        0,    0, 0,   1);
            return tmp;
        }

        /// <summary>
        /// Calculates a matrix to rotate around the z-axis
        /// </summary>
        /// <param name="z">used to give the degree of rotation around the z-axis</param>
        /// <returns>a rotation matrix around the x-axis with the given angle</returns>
        public Matrix3D Rotate_Z(double z)
        {
            double cos = Math.Round(Math.Cos((360.0f - z) * (Math.PI / 180.0f)), 5);
            double sin = Math.Round(Math.Sin((360.0f - z) * (Math.PI / 180.0f)), 5);

            Matrix3D tmp = new Matrix3D(cos, -sin, 0, 0,
                                        sin, cos, 0, 0,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1);
            return tmp;
        }


        /// <summary>
        /// Calculates a new matrix with the specified angles of rotation and 
        /// sets this matrix as new rotation matrix
        /// </summary>
        /// <param name="elevationAngle"></param>
        /// <param name="roomOrientation"></param>
        public void calculateRotationMatrix(double elevationAngle, double roomOrientation)
        {
            Matrix3D newMat = new Matrix3D();

            rotationMatrix = Rotate_Y(roomOrientation) * Rotate_X(elevationAngle) * newMat;
        }


        /// <summary>
        /// Transforms the given joint coordinates to world coordinates.
        /// </summary>
        /// <param name="joints">the body joints (their positions) which will be transformed</param>
        /// <returns></returns>
        public Point3D[] transformJointCoords(Point3D[] joints)
        {
            if (joints == null) return null;
            Point3D[] result = new Point3D[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {

                result[i] = Point3D.Multiply(joints[i], rotationMatrix);
                result[i] = result[i] + transVector;
            }
            return result;
        }

        public void transformJointCoordsReference(Point3D[] joints)
        {
            
            

            for (int i = 0; i < joints.Length; i++)
            {

                joints[i] = Point3D.Multiply(joints[i], rotationMatrix);
                joints[i] = joints[i] + transVector;
            }
            
        }


        /// <summary>
        /// Transforms all Vector3Ds in the Array to Point3D
        /// </summary>
        /// <param name="input">The vectors which will be returned as Points</param>
        /// <returns></returns>
        public Point3D[] Vec3DToPoint3DArray(Vector3D[] input)
        {
            Point3D[] result = new Point3D[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                Point3D p = new Point3D(input[i].X, input[i].Y, input[i].Z);

                result[i] = p;
            }

            return result;
        }
        /// <summary>
        /// Transforms the vector from camera to world coordinates and creates a point with the vectors x,y, and z.
        /// </summary>
        /// <param name="vec">the vector to be transformed</param>
        /// <returns>the point with world coordinates</returns>
        public Point3D Vect3DToTransformedPoint3D(Vector3D vec)
        {

            Vector3D newVec = new Vector3D();

            newVec = Vector3D.Multiply(vec, rotationMatrix);
            newVec = vec + transVector;

            Point3D resPoint = new Point3D(vec.X, vec.Y, vec.Z);

            return resPoint;
        }

        public Vector3D transformVector3D(Vector3D vec)
        {
            Vector3D newVec = vec;

            newVec = Vector3D.Multiply(vec, rotationMatrix);
            newVec = vec + transVector;

            return newVec;
        }

        /// <summary>
        /// This method transforms a given point to world coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <returns>a point point with the given points world coordinates</returns>
        public Point3D TransformPoint3D(Point3D point)
        {
            Point3D newPoint3D = new Point3D();
            point = Point3D.Multiply(point, rotationMatrix);
            point = point + transVector;
            

            newPoint3D.X = point.X;
            newPoint3D.Y = point.Y;
            newPoint3D.Z = point.Z;

            return newPoint3D;
        }
    }
}
