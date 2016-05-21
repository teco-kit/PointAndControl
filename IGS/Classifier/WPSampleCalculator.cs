using HelixToolkit.Wpf;
using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    public class WPSampleCalculator
    {
        public Room calcRoomModel { get; set; }

        
        public WPSampleCalculator(Room room)
        {
            this.calcRoomModel = room;
        }

        public WallProjectionSample calculateSample(Point3D[] vectors, String label)
        {

            Ray3D ray = new Ray3D((Point3D)vectors[0], (Point3D)vectors[1]);
            Point3D samplePoint = intersectAndTestAllWalls(ray);
            WallProjectionSample sample = new WallProjectionSample(new Point3D(), "nullSample");

            if ((samplePoint.X.Equals(float.NaN) == false))
            {
                if (label.Equals(""))
                {
                    sample = new WallProjectionSample(samplePoint);
                  
                }
                else
                {
                    sample = new WallProjectionSample(samplePoint, label);
                    
                }
                return sample;
            }
            
            return sample;
            
        }

        public Point3D intersectAndTestAllWalls(Ray3D ray)
        {
            Point3D wallPoint;
            foreach (Plane3D wall in calcRoomModel.wallList)
            {
                if (ray.PlaneIntersection(wall.Position, wall.Normal, out wallPoint))
                {
                    wallPoint.X = Math.Round(wallPoint.X, 7);
                    wallPoint.Y = Math.Round(wallPoint.Y, 7);
                    wallPoint.Z = Math.Round(wallPoint.Z, 7);

                    // check if point is within room
                    if ((calcRoomModel.width >= wallPoint.X && wallPoint.X >= 0
                        &&
                        calcRoomModel.height >= wallPoint.Y && wallPoint.Y >= 0
                        &&
                        calcRoomModel.depth >= wallPoint.Z && wallPoint.Z >= 0) == true)
                    {
                        return wallPoint;
                    }
                }
            }

            wallPoint = new Point3D(float.NaN, float.NaN, float.NaN);

            return wallPoint;
        }
    }
}
