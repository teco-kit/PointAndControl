using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Drawing;
using IGS.Helperclasses;

namespace IGS.KNN
{
    public class Room
    {
       
        float width { get; set; }
        float depth { get; set; }
        float height { get; set; }

        public struct labelEntry
        {
            public String label;
            public List<Point3D> pointList;
        }

       
        RoomPlane ceiling { get; set; }
        RoomPlane floor { get; set; }
        RoomPlane rightWall { get; set; }
        RoomPlane leftWall { get; set; }
        RoomPlane frontWall { get; set; }
        RoomPlane backWall { get; set; }
        List<RoomPlane> wallList { get; set; }

       
        public Room(float width,  float height, float depth)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
            createRoomWalls();
        }

        public Room()
        {
            this.width = 0;
            this.depth = 0;
            this.height = 0;
        }
        public void createRoomWalls ()
        {
            
             floor = new RoomPlane("floor", new Vector3D(0, 1, 0), width, 0, depth);

             rightWall = new RoomPlane("rightWall", new Vector3D(1, 0, 0), 0, height, depth);

             backWall = new RoomPlane("backWall", new Vector3D(0, 0, 1), width, height, 0);

             frontWall = new RoomPlane("frontWall", new Vector3D(0, 0, -1), width, height, depth);
             
             leftWall = new RoomPlane("leftWall", new Vector3D(-1, 0, 0), width, height, depth);
                
             ceiling = new RoomPlane("ceiling", new Vector3D(0, -1, 0), width, height, depth);
                
            wallList = new List<RoomPlane>();
            putWallsInList();
        }

        private void putWallsInList()

        {
            wallList.Add(ceiling);
            wallList.Add(floor);
            wallList.Add(backWall);
            wallList.Add(frontWall);
            wallList.Add(rightWall);
            wallList.Add(leftWall);
            
        }

        public Point3D intersectAndTestAllWalls(Ray3D ray)
        {
            Point3D wallPoint = new Point3D();
            foreach (RoomPlane wall in wallList)
            {
                if ((ray.PlaneIntersection(wall.plane.Position, wall.plane.Normal)) != null)
                {
                    wallPoint = (Point3D)ray.PlaneIntersection(wall.plane.Position, wall.plane.Normal);
                    if ((width >= wallPoint.X && wallPoint.X >= 0
                        &&
                        depth >= wallPoint.Y && wallPoint.Y >= 0
                        &&
                        height >= wallPoint.Z && wallPoint.Z >= 0) == true)
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

        public void calculateDeviceAreas(KNNClassifier knn)
        {
            
            //a List of Lists containing vectors for devices to retrieve the classificationboxes for 
            //every device

            float step = 0.01f;
            int virtualWidthReducer = 1;
            int virtualHeightReducer = 1;

            int wallIndikator = -1; // 0 for Sidewall, 1 for floor/bottom, 2 for front/backwall
            float normalWallPoint = 0;
       
            int maxStepVirtualWidth = 0;
            int maxStepVirtualHeight = 0;
        


            foreach (RoomPlane wall in wallList)
            {
                
                    Console.WriteLine(wall.name);
                    Console.WriteLine("w:" + wall.width + " h:" + wall.heigth + " d:" + wall.depth);
                    Point3D point = new Point3D(wall.width, wall.heigth, wall.depth);
                    virtualWidthReducer = 1;
                    virtualHeightReducer = 1;
                    wallIndikator = -1;
                    normalWallPoint = 0;
                    maxStepVirtualWidth = 0;
                    maxStepVirtualHeight = 0;
                    Bitmap bitmap = null;

                    if (wall.plane.Normal.X != 0)
                    {
                        maxStepVirtualWidth = (int)Math.Ceiling(depth / step);
                        maxStepVirtualHeight = (int)Math.Ceiling(height / step);

                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        wallIndikator = 0;
                        if (wall.plane.Normal.X < 0)
                        {
                            normalWallPoint = width;
                        }

                        Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
                        Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

                    }
                    else if (wall.plane.Normal.Y != 0)
                    {
                        maxStepVirtualWidth = (int)Math.Ceiling(width / step);
                        maxStepVirtualHeight = (int)Math.Ceiling(depth / step);
                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        wallIndikator = 1;
                        if (wall.plane.Normal.Y < 0)
                        {
                            normalWallPoint = height;
                        }

                        Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
                        Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

                    }
                    else if (wall.plane.Normal.Z != 0)
                    {
                        maxStepVirtualWidth = (int)Math.Ceiling(width / step);
                        maxStepVirtualHeight = (int)Math.Ceiling(height / step);
                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        wallIndikator = 2;
                        if (wall.plane.Normal.Z < 0)
                        {
                            normalWallPoint = depth;
                        }

                        Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
                        Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

                        //virtualWidthResetter = wall.width;
                    }

                    
                    for (int stepCounterVirtualHeight = 0; stepCounterVirtualHeight < maxStepVirtualHeight; stepCounterVirtualHeight++)
                    {
                        float virtualHightReduce = (virtualHeightReducer * step);

                        Point3D newPoint = new Point3D(wall.width, wall.heigth, wall.depth);

                        for (int stepCounterVirtualWidth = 0; stepCounterVirtualWidth < maxStepVirtualWidth; stepCounterVirtualWidth++)
                        {

                            if (wallIndikator == 0)
                            {
                                newPoint.X = normalWallPoint;
                                newPoint.Y = Math.Round(wall.heigth - virtualHightReduce, 4);
                                if (newPoint.Y < 0) newPoint.Y = 0;
                                newPoint.Z = Math.Round(wall.depth - (virtualWidthReducer * step), 4);
                                if (newPoint.Z < 0) newPoint.Z = 0;


                            }
                            else if (wallIndikator == 1)
                            {

                                newPoint.X = Math.Round(wall.width - (virtualWidthReducer * step), 4);
                                if (newPoint.X < 0) newPoint.X = 0;
                                newPoint.Y = normalWallPoint;
                                newPoint.Z = Math.Round(wall.depth - virtualHightReduce, 4);
                                if (newPoint.Z < 0) newPoint.Z = 0;

                            }
                            else if (wallIndikator == 2)
                            {
                                newPoint.X = Math.Round((wall.width - (virtualWidthReducer * step)), 4);
                                if (newPoint.X < 0) newPoint.X = 0;
                                newPoint.Y = Math.Round((wall.heigth - virtualHightReduce), 4);
                                if (newPoint.Y < 0) newPoint.Y = 0;
                                newPoint.Z = normalWallPoint;
                            }
                            else
                            {

                            }
                            virtualWidthReducer++;
                         
                            WallProjectionSample sample = new WallProjectionSample(newPoint);
                            sample = knn.classify(sample);
                          
                            Color c = knn.deviceColorLookupByName(sample.sampleDeviceName);
                           
                            bitmap.SetPixel(stepCounterVirtualWidth, stepCounterVirtualHeight, c);
                            Console.Write(".");
                        }
                        virtualWidthReducer = 1;
                        virtualHeightReducer++;

                    }

                    wall.deviceAreas = bitmap;
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + wall.name + ".bmp");
                    Console.WriteLine("");

                
            }

            XMLComponentHandler.writeLogEntry("Calculated Device BMP");
        }
              
    

        
    }


}
