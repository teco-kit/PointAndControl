using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace IGS.KNN
{
    public class Room
    {
       
        float width { get; set; }
        float depth { get; set; }
        float height { get; set; }

        Plane3D ceiling { get; set; }
        Plane3D floor { get; set; }
        Plane3D rightWall { get; set; }
        Plane3D leftWall { get; set; }
        Plane3D frontWall { get; set; }
        Plane3D backWall { get; set; }
        List<Plane3D> wallList { get; set; }
        
        
        public void createRoomWalls ()
        {
            floor = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
            backWall = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));
            rightWall = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
            leftWall = new Plane3D(new Point3D(width, 0, 0), new Vector3D(-1, 0, 0));
            frontWall = new Plane3D(new Point3D(0, depth, 0), new Vector3D(0, -1, 0));
            ceiling = new Plane3D(new Point3D(0, 0, height), new Vector3D(0, 0, -1));
            wallList = new List<Plane3D>();
            putWallsInList();
        }

        private void putWallsInList()
        {
            wallList.Add(floor);
            wallList.Add(backWall);
            wallList.Add(rightWall);
            wallList.Add(frontWall);
            wallList.Add(leftWall);
            wallList.Add(ceiling);
        }

        public Point3D intersectAndTestAllWalls(Ray3D ray)
        {
            Point3D wallPoint = new Point3D();
            foreach (Plane3D wall in wallList)
            {
                if ((ray.PlaneIntersection(wall.Position, wall.Normal)) != null)
                {
                    wallPoint = (Point3D)ray.PlaneIntersection(wall.Position, wall.Normal);
                    if ((width > wallPoint.X && width >= 0
                        &&
                        depth > wallPoint.Y && wallPoint.Y >= 0
                        &&
                        height > wallPoint.Z && wallPoint.Z >= 0) == true)
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

        public void setRoomMeasures(float width, float depth, float height)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
        }
    }


}
