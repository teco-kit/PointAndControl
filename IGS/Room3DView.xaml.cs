using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using IGS.Server.Devices;
using System.Windows.Media.Animation;
using Microsoft.Kinect;
using IGS.Server.IGS;

namespace IGS
{
    /// <summary>
    /// This class is a view for the 3D representation of the room and its devicces
    /// as well as the users with activated gesture control. Also it does the building and 
    /// and positioning of the 3D elements.
    /// </summary>
    public partial class Room3DView : Window
    {
        /// <summary>
        /// the transformator used for calculating the positions of the 3D models
        /// </summary>
        public CoordTransform transformator { get; set; }
        
        /// <summary>
        /// the list of all devices
        /// </summary>
        private List<Device> deviceList;
        // MVP 1 = room
        // MVP 2 = balls
        // MVP 3 = kinect
        // MVP 4 = skeletons
        private ModelVisual3D kinect;

        /// <summary>
        /// indicator list which body is initialized.
        /// </summary>
        private List<bool> boneListInitList { get; set; }
        /// <summary>
        /// List of IDs of the bodys for each user with active gesture control
        /// </summary>
        public List<ulong> IDList { get; set; }
    
        public List<bool> IDListNullSpaces { get; set; }
        /// <summary>
        /// the complete body for each user
        /// </summary>
        public List<ModelVisual3D> skelList { get; set; }
        /// <summary>
        /// the list for all ball
        /// </summary>
        private List<List<HelixToolkit.Wpf.SphereVisual3D>> ballListsList { get; set; }
        private List<List<HelixToolkit.Wpf.PipeVisual3D>> boneListsList { get; set; }
        /// <summary>
        /// indicator which body was actualized last
        /// </summary>
        private ulong lastSkeletonAktualized { get; set; }
        /// <summary>
        /// a list with a ray for each user where this user is pointing
        /// </summary>
        public List<PipeVisual3D> skelRayList { get; set; }
        /// <summary>
        /// an array how often a body was aktualized
        /// </summary>
        private int[] aktualizations { get; set; }
        /// <summary>
        /// the model of the room to display
        /// </summary>
        ModelVisual3D room = new ModelVisual3D();


        /// <summary>
        /// constructor for the 3D view of the room. 
        /// Initializes all list and models
        /// </summary>
        /// <param name="list">The locally stored device list</param>
        /// <param name="transformator">The transformator used for transforming the coordinates from camera to world coordinates</param>
        public Room3DView(List<Device> list, CoordTransform transformator)
        {


            this.deviceList = list;
            this.transformator = transformator;

            skelList = new List<ModelVisual3D>();
            boneListInitList = new List<Boolean>();
            IDList = new List<ulong>();
            ballListsList = new List<List<SphereVisual3D>>();
            boneListsList = new List<List<PipeVisual3D>>();
            aktualizations = new int[6];
            IDListNullSpaces = new List<bool>();
            skelRayList = new List<PipeVisual3D>();
            for (int i = 0; i < 6; i++)
            {
                skelList.Add(new ModelVisual3D());
                IDList.Add(0);
                IDListNullSpaces.Add(true);
                boneListInitList.Add(false);
                ballListsList.Add(initBalls());
                aktualizations[i] = 0;
            }
            lastSkeletonAktualized = 0;

            kinect = new ModelVisual3D();

            InitializeComponent();
            FillRoom();
        }

        /// <summary>
        /// Creating a triangle with three given points and a calculated normal 
        /// to create the room. 
        /// </summary>
        /// <param name="p0">first point of the triangle</param>
        /// <param name="p1">second point of the triangle</param>
        /// <param name="p2">third point of the triangle</param>
        /// <returns>a trianglemodel for the room</returns>
        private ModelVisual3D creatTriangleModelRoom(Point3D p0, Point3D p1, Point3D p2)
        {
            ModelVisual3D group = new ModelVisual3D();
            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Vector3D normal = new Vector3D();

            triangleMesh.Positions.Add(p0);
            triangleMesh.Positions.Add(p1);
            triangleMesh.Positions.Add(p2);

            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);

            normal = calcNormal(p0, p1, p2);

            
            triangleMesh.Normals.Add(-normal);
            triangleMesh.Normals.Add(-normal);
            triangleMesh.Normals.Add(-normal);

            Material mat = new DiffuseMaterial(new SolidColorBrush(Colors.AliceBlue));
            GeometryModel3D model = new GeometryModel3D(triangleMesh, mat);

            group.Content = model;

            return group;

        }


        /// <summary>
        /// this method creates a cube(room) with a specified width, heights and depth 
        /// </summary>
        /// <param name="width">the width of the room</param>
        /// <param name="height">the height of the room</param>
        /// <param name="depth">the depth of the room</param>
        public void createRoom(float width, float height, float depth)
        {

            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(width, 0, 0);
            Point3D p2 = new Point3D(width, 0, depth);
            Point3D p3 = new Point3D(0, 0, depth);
            Point3D p4 = new Point3D(0, height, 0);
            Point3D p5 = new Point3D(width, height, 0);
            Point3D p6 = new Point3D(width, height, depth);
            Point3D p7 = new Point3D(0, height, depth);

            //front
            room.Children.Add(creatTriangleModelRoom(p3, p2, p6));
            room.Children.Add(creatTriangleModelRoom(p3, p6, p7));
            //right
            room.Children.Add(creatTriangleModelRoom(p2, p1, p5));
            room.Children.Add(creatTriangleModelRoom(p2, p5, p6));
            //back
            room.Children.Add(creatTriangleModelRoom(p1, p0, p4));
            room.Children.Add(creatTriangleModelRoom(p1, p4, p5));
            //left
            room.Children.Add(creatTriangleModelRoom(p0, p3, p7));
            room.Children.Add(creatTriangleModelRoom(p0, p7, p4));
            //top
            room.Children.Add(creatTriangleModelRoom(p7, p6, p5));
            room.Children.Add(creatTriangleModelRoom(p7, p5, p4));
            //bottom
            room.Children.Add(creatTriangleModelRoom(p2, p3, p0));
            room.Children.Add(creatTriangleModelRoom(p2, p0, p1));

            this.mainViewport.Children.Add(room);

        }


        /// <summary>
        /// calculates the normal with the cross product of the provided thee points
        /// </summary>
        /// <param name="p0">fist point</param>
        /// <param name="p1">second point</param>
        /// <param name="p2">third point</param>
        /// <returns></returns>
        private Vector3D calcNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector3D crossProd = Vector3D.CrossProduct(v0, v1);
            return crossProd;
        }

        /// <summary>
        /// Sets the camera to view the 3D scene with the coordinates and view vector specified
        /// in the belonging textboxes
        /// </summary>
        /// <param name="sender">the object which triggered the event</param>
        /// <param name="e">the RoutedEventArgs</param>
        private void SetCamera_Click(object sender, RoutedEventArgs e)
        {
            PerspectiveCamera camera = (PerspectiveCamera)mainViewport.Camera;
            Point3D position = new Point3D(
                Convert.ToDouble(cameraPositionXTextBox.Text),
                Convert.ToDouble(cameraPositionYTextBox.Text),
                Convert.ToDouble(cameraPositionZTextBox.Text));
            Vector3D lookDirection = new Vector3D(
                Convert.ToDouble(lookAtXTextBox.Text),
                Convert.ToDouble(lookAtYTextBox.Text),
                Convert.ToDouble(lookAtZTextBox.Text));
            camera.Position = position;
            camera.LookDirection = lookDirection;
        }

        /// <summary>
        /// fills the room with the sphere representations of all devices.
        /// </summary>
        private void FillRoom()
        {
            if (deviceList != null)
            {
                ModelVisual3D model = new ModelVisual3D();
                for (int i = 0; i < deviceList.Count; i++)
                {
                    for (int j = 0; j < deviceList[i].Form.Count; j++)
                    {
                        Point3D center = new Point3D();
                        Vector3D vec = deviceList[i].Form[j].Centre;
                        float rad = deviceList[i].Form[j].Radius;

                        HelixToolkit.Wpf.SphereVisual3D sphere = new HelixToolkit.Wpf.SphereVisual3D();

                        center.X = vec.X;
                        center.Y = vec.Y;
                        center.Z = vec.Z;

                        Material mat = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

                        sphere.Center = center;
                        sphere.Material = mat;
                        sphere.Radius = rad;
                        sphere.Visible = true;
                        sphere.PhiDiv = 13;
                        sphere.ThetaDiv = 13;

                        model.Children.Add(sphere);
                    }
                }
                mainViewport.Children.Add(model);
            }
        }


        /// <summary>
        /// Sets the 3D representation of the kinect camera in the 3D representation of the room
        /// </summary>
        /// <param name="devKinect">the representation of the kinect camera of the IGS to get its information of size and 
        ///                         and position</param>
        public void SetKinectCamera(IGS.Server.Devices.devKinect devKinect)
        {
            Point3D center = new Point3D();


            HelixToolkit.Wpf.BoxVisual3D addKinect = new HelixToolkit.Wpf.BoxVisual3D();
            RotateTransform3D trans = new RotateTransform3D();
            AxisAngleRotation3D rotation = new AxisAngleRotation3D();

            center.X = devKinect.ball.Centre.X;
            center.Y = devKinect.ball.Centre.Y;
            center.Z = devKinect.ball.Centre.Z;

            addKinect.Center = center;
            addKinect.Length = 0.38;
            addKinect.Width = 0.156;
            addKinect.Height = 0.123;

            rotation.Axis = (new Vector3D(0, 1, 0));
            rotation.Angle = devKinect.roomOrientation;

            trans.Rotation = rotation;
            trans.CenterX = center.X;
            trans.CenterY = center.Y;
            trans.CenterZ = center.Z;
            addKinect.Transform = trans;

            this.kinect = addKinect;
            mainViewport.Children.Remove(kinect);
            mainViewport.Children.Add(kinect);
        }

        ///// <summary>
        ///// clears the viewport of all children
        ///// </summary>
        //private void ClearViewport()
        //{
        //    ModelVisual3D m;
        //    for (int i = mainViewport.Children.Count - 1; i >= 0; i--)
        //    {
        //        m = (ModelVisual3D)mainViewport.Children[i];
        //        if (m.Content is DirectionalLight == false)
        //            mainViewport.Children.Remove(m);
        //    }
        //}

        /// <summary>
        /// this method decides which (if any) body model will be replaced and then actulizes 
        /// or replaces the body with its more actual representation.
        /// </summary>
        /// <param name="body">the body information provided by the kinect camera</param>

        public void createBody(Body body)
        {
            int IDPlace = 0;
            bool IDfound = false;
            for (int i = 0; i < IDList.Count; i++)
            {
                if (IDList[i] == body.TrackingId)
                {
                    IDfound = true;
                    aktualizations[i]++;
                    IDPlace = i;
                    break;
                }
            }
            if (IDfound == false)
            {
                for (int i = 0; i < IDListNullSpaces.Count; i++)
                {
                    if (IDListNullSpaces[i] == true)
                    {
                        IDListNullSpaces[i] = false;
                        IDList[i] = body.TrackingId;
                        aktualizations[i] = 1;
                        IDfound = true;
                        IDPlace = i;
                        break;
                    }
                }
            }

            if (IDfound == false)
            {
                int minActualizations = aktualizations.Min();
                int minActInd = aktualizations.ToList().IndexOf(minActualizations);

                aktualizations[minActInd] = 1;
                IDList[minActInd] = body.TrackingId;
                IDfound = true;
                IDPlace = minActInd;
            }


            Point3D head = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.Head].Position.X, body.Joints[JointType.Head].Position.Y, body.Joints[JointType.Head].Position.Z));
            Point3D right_hand = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.HandRight].Position.X, body.Joints[JointType.HandRight].Position.Y, body.Joints[JointType.HandRight].Position.Z));
            Point3D right_wrist = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.WristRight].Position.X, body.Joints[JointType.WristRight].Position.Y, body.Joints[JointType.WristRight].Position.Z));
            Point3D right_elbow = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.ElbowRight].Position.X, body.Joints[JointType.ElbowRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Z));
            Point3D right_shoulder = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.ShoulderRight].Position.Y, body.Joints[JointType.ShoulderRight].Position.Z));
            Point3D left_hand = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.HandLeft].Position.X, body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.HandLeft].Position.Z));
            Point3D left_wrist = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.WristLeft].Position.X, body.Joints[JointType.WristLeft].Position.Y, body.Joints[JointType.WristLeft].Position.Z));
            Point3D left_elbow = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.ElbowLeft].Position.X, body.Joints[JointType.ElbowLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Z));
            Point3D left_shoulder = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.ShoulderLeft].Position.Y, body.Joints[JointType.ShoulderLeft].Position.Z));
            Point3D right_foot = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.FootRight].Position.X, body.Joints[JointType.FootRight].Position.Y, body.Joints[JointType.FootRight].Position.Z));
            Point3D right_ankle = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.AnkleRight].Position.X, body.Joints[JointType.AnkleRight].Position.Y, body.Joints[JointType.AnkleRight].Position.Z));
            Point3D right_knee = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.KneeRight].Position.X, body.Joints[JointType.KneeRight].Position.Y, body.Joints[JointType.KneeRight].Position.Z));
            Point3D right_hip = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.HipRight].Position.X, body.Joints[JointType.HipRight].Position.Y, body.Joints[JointType.HipRight].Position.Z));
            Point3D left_foot = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.FootLeft].Position.X, body.Joints[JointType.FootLeft].Position.Y, body.Joints[JointType.FootLeft].Position.Z));
            Point3D left_ankle = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.AnkleLeft].Position.X, body.Joints[JointType.AnkleLeft].Position.Y, body.Joints[JointType.AnkleLeft].Position.Z));
            Point3D left_knee = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.KneeLeft].Position.X, body.Joints[JointType.KneeLeft].Position.Y, body.Joints[JointType.KneeLeft].Position.Z));
            Point3D left_hip = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.HipLeft].Position.X, body.Joints[JointType.HipLeft].Position.Y, body.Joints[JointType.HipLeft].Position.Z));
            Point3D center_hip = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.SpineBase].Position.X, body.Joints[JointType.SpineBase].Position.Y, body.Joints[JointType.SpineBase].Position.Z));
            Point3D spine = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.SpineMid].Position.X, body.Joints[JointType.SpineMid].Position.Y, body.Joints[JointType.SpineMid].Position.Z));
            Point3D center_shoulder = transformator.TransformPoint3D(new Point3D(body.Joints[JointType.SpineShoulder].Position.X, body.Joints[JointType.SpineShoulder].Position.Y, body.Joints[JointType.SpineShoulder].Position.Z));


            List<Point3D> midSec = new List<Point3D>();

            midSec.Add(head);
            midSec.Add(center_shoulder);
            midSec.Add(spine);
            midSec.Add(center_hip);


            List<Point3D> pList = new List<Point3D>();

            pList.Add(right_hand);
            pList.Add(right_wrist);
            pList.Add(right_elbow);
            pList.Add(right_shoulder);
            pList.Add(center_shoulder);
            pList.Add(left_shoulder);
            pList.Add(left_elbow);
            pList.Add(left_wrist);
            pList.Add(left_hand);
            pList.Add(head);
            pList.Add(spine);
            pList.Add(right_foot);
            pList.Add(right_ankle);
            pList.Add(right_knee);
            pList.Add(right_hip);
            pList.Add(center_hip);
            pList.Add(left_hip);
            pList.Add(left_knee);
            pList.Add(left_ankle);
            pList.Add(left_foot);





            removeModels(skelList[IDPlace]);
            
            if( boneListInitList[IDPlace] == false)
            {
                initBodyBones(IDPlace, pList, midSec);
            }
            transformBallList(IDPlace, pList, midSec);
            makeBodyRay(IDPlace, right_elbow, right_wrist);
            replaceBodyBones(IDPlace, pList, midSec);
        }

        /// <summary>
        /// initializes for a body its joints
        /// </summary>
        /// <returns>returns a list with initialized joint spheres</returns>
        private List<SphereVisual3D> initBalls()
        {
            List<HelixToolkit.Wpf.SphereVisual3D> sphereList = new List<SphereVisual3D>();
            double pointsizes = 0.05;

            for (int i = 0; i < 24; i++)
            {
                SphereVisual3D sphere = new SphereVisual3D();
                Point3D point = new Point3D(-1, -1, -1);
                Material mat = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

                sphere.PhiDiv = 12;
                sphere.ThetaDiv = 12;
                sphere.Center = point;
                sphere.Material = mat;
                sphere.Radius = pointsizes;

                sphereList.Add(sphere);
            }

            return sphereList;
        }
        /// <summary>
        /// Creates a pipe (bone) between two joints
        /// </summary>
        /// <param name="firstJoint">the first joint(end) of  the bone </param>
        /// <param name="lastJoint">the second joint(end) of the bone</param>
        /// <returns>a list with initialized bones</returns>
        private PipeVisual3D createBones(Point3D firstJoint, Point3D lastJoint)
        {
            PipeVisual3D pipe = new PipeVisual3D();
            Material mat = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

            pipe.Diameter = 0.07f;
            pipe.InnerDiameter = 0.00f;
            pipe.ThetaDiv = 3;
            pipe.Point1 = firstJoint;
            pipe.Point2 = lastJoint;
            pipe.Material = mat;
            return (pipe);

        }

        /// <summary>
        /// removes all children of a provided model
        /// </summary>
        /// <param name="model">the model the children should be remoed from</param>
        private void removeModels(ModelVisual3D model)
        {
            int counter = model.Children.Count;
            ModelVisual3D m = new ModelVisual3D();
            for (int i = 0; i < counter; i++)
            {
                m = (ModelVisual3D)model.Children[counter - i - 1];
                model.Children.Remove(m);
            }
        }


        /// <summary>
        /// transforms all joint spheres of a body to their new positions
        /// </summary>
        /// <param name="bodyNr">the number of the body which should be be placed</param>
        /// <param name="pList"></param>
        private void transformBallList(int bodyNr, List<Point3D> pList, List<Point3D> midsec)
        {
            TranslateTransform3D tmp = new TranslateTransform3D();

            for (int i = 0; i < pList.Count; i++)
             {
                    
                    tmp.OffsetX = pList[i].X - ballListsList[bodyNr][i].Center.X;
                    tmp.OffsetX = pList[i].Y - ballListsList[bodyNr][i].Center.Y;
                    tmp.OffsetX = pList[i].Z - ballListsList[bodyNr][i].Center.Z;

                    ballListsList[bodyNr][i].Transform = tmp;

                    //skelList[bodyNr].Children.Remove(ballListsList[bodyNr][i]);
                    //skelList[bodyNr].Children.Add(ballListsList[bodyNr][i]);
              }
            for (int i = 0; i < midsec.Count; i++)
            {
                tmp.OffsetX = midsec[i].X - ballListsList[bodyNr][pList.Count + i].Center.X;
                tmp.OffsetX = midsec[i].Y - ballListsList[bodyNr][pList.Count + i].Center.Y;
                tmp.OffsetX = midsec[i].Z - ballListsList[bodyNr][pList.Count + i].Center.Z;

                ballListsList[bodyNr][pList.Count + i].Transform = tmp;
            }
        }

        /// <summary>
        /// initializes the bones for a body with the provided number, lists for the midsection and other joints provided
        /// by the kinect camera
        /// </summary>
        /// <param name="bodyNr">the number of the body which will be initialized</param>
        /// <param name="pList">the list of joints without the midsection</param>
        /// <param name="midSection">the list of midsection joints</param>
        private void initBodyBones(int bodyNr, List<Point3D> pList, List<Point3D> midSection)
        {
            boneListsList[bodyNr] = new List<PipeVisual3D>();

            //from right to center;
            for (int i = 0; i < 4; i++)
            {
                boneListsList[bodyNr].Add(createBones(pList[i], pList[i + 1]));
            }
            for (int i = 4; i < 8; i++)
            {
                boneListsList[bodyNr].Add(createBones(pList[i], pList[i + 1]));
            }

            //head to center hip

            for (int i = 0; i < midSection.Count - 1; i++)
            {
                boneListsList[bodyNr].Add(createBones(midSection[i], midSection[i + 1]));
            }
            //from right foot to center
            for (int i = 11; i < 15; i++)
            {
                boneListsList[bodyNr].Add(createBones(pList[i], pList[i + 1]));
            }
            //from left foot to center
            for (int i = 15; i < 19; i++)
            {
                boneListsList[bodyNr].Add(createBones(pList[i], pList[i + 1]));
            }

            skelRayList[bodyNr] = new PipeVisual3D();
            skelRayList[bodyNr].Diameter = 0.07f;
            skelRayList[bodyNr].InnerDiameter = 0.00f;
            skelRayList[bodyNr].ThetaDiv = 3;
            boneListInitList[bodyNr] = true;
        }

        /// <summary>
        /// replaces the bones for a body with the provided number and joint list (midsection and other joints seperated)
        /// </summary>
        /// <param name="bodyNr">the number of the body which bones will be replaced</param>
        /// <param name="pList">the joints of the body without the midsection </param>
        /// <param name="midSection">the joints of the midsection without the other joints</param>
        private void replaceBodyBones(int bodyNr, List<Point3D> pList, List<Point3D> midSection)
        {

            for (int i = 0; i < 4; i++)
            {
                boneListsList[bodyNr][i].Point1 = pList[i];
                boneListsList[bodyNr][i].Point2 = pList[i + 1];

            }
            for (int i = 4; i < 8; i++)
            {
                boneListsList[bodyNr][i].Point1 = pList[i];
                boneListsList[bodyNr][i].Point2 = pList[i + 1];

            }

            int counter = 8;
            //head to center hip
            for (int i = 0; i < midSection.Count - 1; i++)
            {
                boneListsList[bodyNr][counter].Point1 = midSection[i];
                boneListsList[bodyNr][counter].Point2 = midSection[i + 1];
                counter++;
            }


            //from right foot to center
            for (int i = 11; i < 15; i++)
            {
                boneListsList[bodyNr][i].Point1 = pList[i];
                boneListsList[bodyNr][i].Point2 = pList[i + 1];

            }
            //from left foot to center
            for (int i = 15; i < 19; i++)
            {
                boneListsList[bodyNr][i].Point1 = pList[i];
                boneListsList[bodyNr][i].Point2 = pList[i + 1];

            }
            mainViewport.Children.Remove(skelList[bodyNr]);
            mainViewport.Children.Add(skelList[bodyNr]);
        }

        /// <summary>
        /// calculates a ray to indicate where a body points. The vector 
        /// is calculated with the right elbow and right wrist
        /// </summary>
        /// <param name="bodyNr">the number of the body the ray should be calculated for </param>
        /// <param name="rightElbow">the point of the right elbow of the body</param>
        /// <param name="rightWrist">the point of the right wrist of the body </param>
        private void makeBodyRay(int bodyNr, Point3D rightElbow, Point3D rightWrist)
        {
            skelRayList[bodyNr].Point1 = rightElbow;
            Vector3D vec = new Vector3D((rightWrist.X - rightElbow.X), (rightWrist.Y - rightElbow.Y), (rightWrist.Z - rightElbow.Z));
            skelRayList[bodyNr].Point2 = Point3D.Add(rightElbow, Vector3D.Multiply(10, vec));
            mainViewport.Children.Remove(skelRayList[bodyNr]);
            mainViewport.Children.Add(skelRayList[bodyNr]);
        }



    }
}