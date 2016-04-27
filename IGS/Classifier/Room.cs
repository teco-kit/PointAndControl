using System.Collections.Generic;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    public class Room
    {

        public double width { get; set; }
        public double depth { get; set; }
        public double height { get; set; }

        Plane3D ceiling { get; set; }
        Plane3D floor { get; set; }
        Plane3D rightWall { get; set; }
        Plane3D leftWall { get; set; }
        Plane3D frontWall { get; set; }
        Plane3D backWall { get; set; }
        public List<Plane3D> wallList { get; set; }


        public Room(double width, double height, double depth)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
            wallList = new List<Plane3D>();
            createRoomWalls();
        }

        public Room()
        {
            this.width = 0;
            this.depth = 0;
            this.height = 0;
        }
        public void createRoomWalls()
        {

            wallList.Clear();

            floor = new Plane3D(new Point3D(width, 0, depth), new Vector3D(0, 1, 0));
            wallList.Add(floor);

            rightWall = new Plane3D(new Point3D(0, height, depth), new Vector3D(1, 0, 0));
            wallList.Add(rightWall);

            backWall = new Plane3D(new Point3D(width, height, 0), new Vector3D(0, 0, 1));
            wallList.Add(backWall);

            frontWall = new Plane3D(new Point3D(width, height, depth), new Vector3D(0, 0, -1));
            wallList.Add(frontWall);

            leftWall = new Plane3D(new Point3D(width, height, depth), new Vector3D(-1, 0, 0));
            wallList.Add(leftWall);

            ceiling = new Plane3D(new Point3D(width, height, depth), new Vector3D(0, -1, 0));
            wallList.Add(ceiling);


        }





        public void setRoomMeasures(double width, double depth, double height)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;

            createRoomWalls();
        }


              //Early try to calculate nearest-neighbor-texture for faster klassifikation for selection attempts

        //    public void calculateDeviceAreas(KNNClassifier knn, DataHolder data)
        //    {

        //        //a List of Lists containing vectors for devices to retrieve the classificationboxes for 
        //        //every device

        //        float step = 0.001f;
        //        int virtualWidthReducer = 1;
        //        int virtualHeightReducer = 1;

        //        int wallIndikator = -1; // 0 for Sidewall, 1 for floor/bottom, 2 for front/backwall
        //        float normalWallPoint = 0;

        //        int maxStepVirtualWidth = 0;
        //        int maxStepVirtualHeight = 0;



        //        foreach (Plane3D wall in wallList)
        //        {

        //            Point3D point = new Point3D(wall.width, wall.heigth, wall.depth);
        //            virtualWidthReducer = 1;
        //            virtualHeightReducer = 1;
        //            wallIndikator = -1;
        //            normalWallPoint = 0;
        //            maxStepVirtualWidth = 0;
        //            maxStepVirtualHeight = 0;
        //            Bitmap bitmap = null;

        //            if (wall.Normal.X != 0)
        //            {
        //                maxStepVirtualWidth = (int)Math.Ceiling(depth / step);
        //                maxStepVirtualHeight = (int)Math.Ceiling(height / step);

        //                bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
        //                wallIndikator = 0;
        //                if (wall.Normal.X < 0)
        //                {
        //                    normalWallPoint = width;
        //                }

        //                Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
        //                Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

        //            }
        //            else if (wall.Normal.Y != 0)
        //            {
        //                maxStepVirtualWidth = (int)Math.Ceiling(width / step);
        //                maxStepVirtualHeight = (int)Math.Ceiling(depth / step);
        //                bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
        //                wallIndikator = 1;
        //                if (wall.plane.Normal.Y < 0)
        //                {
        //                    normalWallPoint = height;
        //                }

        //                Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
        //                Console.WriteLine("StepsHight:" + maxStepVirtualHeight);

        //            }
        //            else if (wall.Normal.Z != 0)
        //            {
        //                maxStepVirtualWidth = (int)Math.Ceiling(width / step);
        //                maxStepVirtualHeight = (int)Math.Ceiling(height / step);
        //                bitmap = new Bitmap(maxStepVirtualWidth, maxStepVirtualHeight);
        //                wallIndikator = 2;
        //                if (wall.Normal.Z < 0)
        //                {
        //                    normalWallPoint = depth;
        //                }

        //                Console.WriteLine("StepsWidth:" + maxStepVirtualWidth);
        //                Console.WriteLine("StepsHight:" + maxStepVirtualHeight);
        //            }


        //            for (int stepCounterVirtualHeight = 0; stepCounterVirtualHeight < maxStepVirtualHeight; stepCounterVirtualHeight++)
        //            {
        //                float virtualHightReduce = (virtualHeightReducer * step);

        //                Point3D newPoint = new Point3D(wall.width, wall.heigth, wall.depth);

        //                for (int stepCounterVirtualWidth = 0; stepCounterVirtualWidth < maxStepVirtualWidth; stepCounterVirtualWidth++)
        //                {

        //                    if (wallIndikator == 0)
        //                    {
        //                        newPoint.X = normalWallPoint;
        //                        newPoint.Y = Math.Round(wall.heigth - virtualHightReduce, 4);
        //                        if (newPoint.Y < 0) newPoint.Y = 0;
        //                        newPoint.Z = Math.Round(wall.depth - (virtualWidthReducer * step), 4);
        //                        if (newPoint.Z < 0) newPoint.Z = 0;


        //                    }
        //                    else if (wallIndikator == 1)
        //                    {

        //                        newPoint.X = Math.Round(wall.width - (virtualWidthReducer * step), 4);
        //                        if (newPoint.X < 0) newPoint.X = 0;
        //                        newPoint.Y = normalWallPoint;
        //                        newPoint.Z = Math.Round(wall.depth - virtualHightReduce, 4);
        //                        if (newPoint.Z < 0) newPoint.Z = 0;

        //                    }
        //                    else if (wallIndikator == 2)
        //                    {
        //                        newPoint.X = Math.Round((wall.width - (virtualWidthReducer * step)), 4);
        //                        if (newPoint.X < 0) newPoint.X = 0;
        //                        newPoint.Y = Math.Round((wall.heigth - virtualHightReduce), 4);
        //                        if (newPoint.Y < 0) newPoint.Y = 0;
        //                        newPoint.Z = normalWallPoint;
        //                    }
        //                    else
        //                    {

        //                    }
        //                    virtualWidthReducer++;

        //                    WallProjectionSample sample = new WallProjectionSample(newPoint);
        //                    sample = knn.classify(sample);

        //                    Color c = data.deviceColorLookupByName(sample.sampledeviceIdentifier);

        //                    bitmap.SetPixel(stepCounterVirtualWidth, stepCounterVirtualHeight, c);
        //                    Console.Write(".");
        //                }
        //                virtualWidthReducer = 1;
        //                virtualHeightReducer++;

        //            }

        //            wall.deviceAreas = bitmap;
        //            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
        //            bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + wall.name + ".bmp");
        //            Console.WriteLine("");


        //        }

        //        XMLComponentHandler.writeLogEntry("Calculated Device BMP");
        //    }
        //}


    }
}
