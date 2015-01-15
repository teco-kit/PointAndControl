using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Drawing;

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

       
        public Room(float width, float depth, float height)
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
                
             backWall = new RoomPlane("backWall", new Vector3D(0, 0, -1), width, height, 0);
        
             rightWall = new RoomPlane("rightWall", new Vector3D(1, 0, 0), 0, height, depth); 
                
             leftWall = new RoomPlane("leftWall", new Vector3D(-1, 0, 0), width, height, depth);
                
             frontWall = new RoomPlane("frontWall", new Vector3D(0, 0, 1), width, height, depth);
                
             ceiling = new RoomPlane("ceiling", new Vector3D(0, -1, 0), width, height, depth);
                
            wallList = new List<RoomPlane>();
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

        public void calculateDeviceAreas(KNNClassifierHandler knn)
        {
            //a List of Lists containing vectors for devices to retrieve the classificationboxes for 
            //every device
            List<labelEntry> wallSamples = new List<labelEntry>();
            float step = 0.3f;
            int virtualWidthReducer = 1;
            int virtualHeightReducer = 1;

            int wallIndikator = -1; // 0 for Sidewall, 1 for floor/bottom, 2 for front/backwall
            float normalWallPoint = 0;
       
            int maxStepVirtualWidth = 0;
            int maxStepVirtualHeight = 0;
            foreach (String label in knn.labels)
            {
                labelEntry newEntry = new labelEntry();
                newEntry.label = label;
                newEntry.pointList = new List<Point3D>();
                wallSamples.Add(newEntry);
            }


            foreach (RoomPlane wall in wallList)
            {

                Point3D point = new Point3D(wall.width, wall.heigth, wall.depth);
                Bitmap bitmap = null;

                if (wall.name == "frontWall")
                {
                    Console.WriteLine("Frontwall found");
                    if (wall.plane.Normal.X != 0)
                    {
                        maxStepVirtualWidth = (int) Math.Ceiling(depth / step);
                        maxStepVirtualHeight = (int) Math.Ceiling(height / step);
                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        Console.WriteLine("SideWall");
                        wallIndikator = 0;
                        if (!(wall.plane.Normal.X > 0))
                        {
                            normalWallPoint = width;
                        }

                        //virtualWidthResetter = wall.depth;
                        //isSidewall = true;
                    }
                    else if (wall.plane.Normal.Y != 0)
                    {
                        maxStepVirtualWidth = (int) Math.Ceiling(width / step);
                        maxStepVirtualHeight = (int)Math.Ceiling(depth / step);
                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        Console.WriteLine("Floor/Bottom");
                        wallIndikator = 1;
                        if (!(wall.plane.Normal.X > 0))
                        {
                            normalWallPoint = height;
                        }

                        //virtualWidthResetter = wall.width;
                    }
                    else if (wall.plane.Normal.Z != 0)
                    {
                        maxStepVirtualWidth = (int)Math.Ceiling(width / step);
                        maxStepVirtualHeight = (int)Math.Ceiling(height / step);
                        bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
                        Console.WriteLine("Front/Backwall");
                        wallIndikator = 2;
                        if (!(wall.plane.Normal.Z > 0))
                        {
                            normalWallPoint = depth;
                        }

                        Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
                        Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

                        //virtualWidthResetter = wall.width;
                    }



                    for (int stepCounterVirtualHeight = 0; stepCounterVirtualHeight < maxStepVirtualHeight; stepCounterVirtualHeight++)
                    {
                        float virtualHightReduce = (virtualHeightReducer*step);

                        Point3D newPoint = new Point3D(wall.width, wall.heigth, wall.depth);

                        for (int stepCounterVirtualWidth = 0; stepCounterVirtualWidth < maxStepVirtualWidth; stepCounterVirtualWidth++)
                        {

                            if (wallIndikator == 0)
                            {
                                newPoint.X = normalWallPoint;
                                newPoint.Y = Math.Round(wall.heigth - virtualHightReduce,4);
                                if (newPoint.Y < 0) newPoint.Y = 0;
                                newPoint.Z = Math.Round(wall.depth - (virtualWidthReducer * step),4);
                                if (newPoint.Z < 0) newPoint.Z = 0; 
                            }
                            else if (wallIndikator == 1)
                            {

                                newPoint.X = Math.Round(wall.width - (virtualWidthReducer * step),4);
                                if (newPoint.X < 0) newPoint.X = 0;
                                newPoint.Y = normalWallPoint;
                                newPoint.Z = Math.Round(wall.depth - virtualHightReduce,4);
                                if (newPoint.Z < 0) newPoint.Z = 0; 

                            }
                            else if (wallIndikator == 2)
                            {
                                newPoint.X = Math.Round((wall.width - (virtualWidthReducer * step)),4);
                                if (newPoint.X < 0) newPoint.X = 0;
                                newPoint.Y = Math.Round((wall.heigth - virtualHightReduce),4);
                                if (newPoint.Y < 0) newPoint.Y = 0;
                                newPoint.Z = normalWallPoint;

                                Console.WriteLine(newPoint);
                            }
                            else
                            {

                            }
                            virtualWidthReducer++;
                            KNNSample sample = new KNNSample(newPoint);
                            sample = knn.classify(sample);
                            Color c = knn.deviceColorLookup(sample.sampleDeviceName);
                            bitmap.SetPixel(stepCounterVirtualWidth, stepCounterVirtualHeight, c);
                            Console.Write(".");
                        }
                        virtualWidthReducer = 1;
                        virtualHeightReducer++;
                       
                    }

                    wall.deviceAreas = bitmap;
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + wall.name + ".bmp");
                }
            }
        }
                //if (wall.plane.Normal.X != 0)
                //{
                //    maxStepVirtualWidth = depth / step;
                //    maxStepVirtualHeight = height / step;
                //    Bitmap bitmap = new Bitmap((int)Math.Ceiling(depth / step), (int)Math.Ceiling(height / step));

                //    for (int stepCounterVirtualHeight = 0; stepCounterVirtualHeight < maxStepVirtualHeight; stepCounterVirtualHeight++)
                //    {
                //        Console.WriteLine("in Y");
                //        float virtualHeightReduce = (step * virtualHeightReducer)/2;
                //        point.Z = wall.depth;
                //        for (int stepCounterVirtualWidth = 0; stepCounterVirtualWidth < maxStepVirtualWidth; stepCounterVirtualWidth++)
                //        {
                //            Console.WriteLine("in Z");
                //            point = new Point3D(wall.width, wall.heigth - virtualHeightReduce, wall.depth - reducerZ * step);
                //            reducerZ++;

                //            KNNSample sample = new KNNSample(point);
                //            sample = knn.classify(sample);

                //            foreach (labelEntry entry in wallSamples)
                //            {
                //                if (entry.label.ToLower().Equals(sample.sampleDeviceName.ToLower()))
                //                {
                //                    entry.pointList.Add(point);
                //                }
                //            }
                //        }
                //        reducerZ = 1;
                //        virtualHeightReducer++;
                //    }
                    
                //}
                //if (wall.plane.Normal.Y != 0)
                //{
                //    maxStepOuter = width / step;
                //    maxStepInner = depth / step;
                //    Bitmap bitmap = new Bitmap((int)Math.Ceiling(width / step), (int)Math.Ceiling(depth / step));
                //    Console.WriteLine("Y");
                //    Point3D point = new Point3D(wall.width, wall.heigth, wall.depth);
                //    while (0 <= point.X && point.X <= wall.width)
                //    for (int stepCounterOuter = 0; stepCounterOuter < maxStepOuter; stepCounterOuter++)
                //    {
                //        Console.WriteLine("in X");
                //        float Xreduce = step * reducerX;
                //        point.Z = wall.depth;
                //        while (0 <= point.Z && point.Z <= wall.depth)
                //        for (int stepCounterInner = 0; stepCounterInner < maxStepInner; stepCounterInner++)
                //        {
                //            Console.WriteLine("in Z");
                //            point = new Point3D(wall.width - Xreduce, wall.heigth, wall.depth - reducerZ * step);
                //            reducerZ++;

                //            KNNSample sample = new KNNSample(point);
                //            sample = knn.classify(sample);

                //            foreach (labelEntry entry in wallSamples)
                //            {
                //                if (entry.label.ToLower().Equals(sample.sampleDeviceName.ToLower()))
                //                {
                //                    entry.pointList.Add(point);
                //                }
                //            }
                //        }
                        
                //        reducerZ = 1;
                //        reducerX++;
                //    }
                //}
                //if (wall.plane.Normal.Z != 0)
                //{
                //    maxStepOuter = width / step;
                //    maxStepInner = height / step;
                //    Bitmap bitmap = new Bitmap((int)Math.Ceiling(width/step), (int)Math.Ceiling(height/step));
                //    Console.WriteLine("z");
                //    Point3D point = new Point3D(wall.width, wall.heigth, wall.depth);
                //    for (int stepCounterOuter = 0; stepCounterOuter < maxStepOuter; stepCounterOuter++)
                //    {
                //        Console.WriteLine("in Y");
                //        float Yreduce = step * reducerY;
                //        point.X = wall.width;
                //        for (int stepCounterInner = 0; stepCounterInner < maxStepInner; stepCounterInner++)
                //        {
                //            Console.WriteLine("in X");
                //            point = new Point3D(wall.width - reducerX * step, wall.heigth - Yreduce, wall.depth);
                //            reducerZ++;

                //            KNNSample sample = new KNNSample(point);
                //            sample = knn.classify(sample);

                //            foreach (labelEntry entry in wallSamples)
                //            {
                //                if (entry.label.ToLower().Equals(sample.sampleDeviceName.ToLower()))
                //                {
                //                    entry.pointList.Add(point);
                //                }
                //            }
                //        }
                //        reducerX = 1;
                //        reducerY++;
                //    }
                //}
    

        
    }


}
