using HelixToolkit.Wpf;
using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Classifier
{
    public class WPSampleCalculator
    {
        public Room calcRoomModel { get; set; }

        
        public WPSampleCalculator(Room room)
        {
            this.calcRoomModel = room;
        }

        public WallProjectionSample calculateSample(Vector3D[] vectors, String label)
        {
            
            Vector3D direction = Vector3D.Subtract(vectors[3], vectors[2]);
            Ray3D ray = new Ray3D(vectors[2].ToPoint3D(), direction);
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
            Point3D wallPoint = new Point3D();
            foreach (Plane3D wall in calcRoomModel.wallList)
            {

                if (
                    (ray.PlaneIntersection(wall.Position, wall.Normal)) != null
                   )
                {
                    wallPoint = (Point3D)ray.PlaneIntersection(wall);
                    wallPoint.X = Math.Round(wallPoint.X, 7);
                    wallPoint.Y = Math.Round(wallPoint.Y, 7);
                    wallPoint.Z = Math.Round(wallPoint.Z, 7);
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
            wallPoint.X = float.NaN;
            wallPoint.Y = float.NaN;
            wallPoint.Z = float.NaN;

            return wallPoint;
        }
    }
}
