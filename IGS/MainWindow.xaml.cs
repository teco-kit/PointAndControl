using IGS;
using IGS.Helperclasses;
using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using IGS.Server.WebServer;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using System.ComponentModel;
using System.Xml.Linq;
using IGS.KNN;
using IGS.Kinect;




/// <summary>
///     Interaktionslogik für MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    ///     Width of output drawing
    /// </summary>
    private const float RenderWidth = 640.0f;

    /// <summary>
    ///     Height of our output drawing
    /// </summary>
    private const float RenderHeight = 480.0f;

    /// <summary>
    ///     Thickness of drawn joint lines
    /// </summary>
    private const double JointThickness = 3;

    /// <summary>
    ///     Thickness of body center ellipse
    /// </summary>
    private const double BodyCenterThickness = 10;

    /// <summary>
    ///     Thickness of clip edge rectangles
    /// </summary>
    private const double ClipBoundsThickness = 10;

    /// <summary>
    ///     Brush used to draw skeleton center point
    /// </summary>
    private readonly Brush _centerPointBrush = Brushes.Blue;

    /// <summary>
    ///     Pen used for drawing bones that are currently inferred
    /// </summary>
    private readonly Pen _inferredBonePen = new Pen(Brushes.Gray, 1);

    /// <summary>
    ///     Brush used for drawing joints that are currently inferred
    /// </summary>
    private readonly Brush _inferredJointBrush = Brushes.Yellow;


    private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
    /// <summary>
    ///     preperations for drawing the image and hands
    /// </summary>
    private byte[] pixels = null;
    private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
    private const double HandSize = 30;
    /// <summary>
    /// Width of display (depth space)
    /// </summary>
    private int displayWidth;

    /// <summary>
    /// Height of display (depth space)
    /// </summary>
    private int displayHeight;
    /// <summary>
    ///     Pen used for drawing bones that are currently tracked
    /// </summary>
    private readonly Pen _trackedBonePen = new Pen(Brushes.Green, 6);

    /// <summary>
    ///     Brush used for drawing joints that are currently tracked
    /// </summary>
    private readonly Brush _trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

    private String roomPath = AppDomain.CurrentDomain.BaseDirectory + "\\3DroomSize.txt";

    /// <summary>
    /// Brush used for drawing hands that are currently tracked as closed
    /// </summary>
    private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

    /// <summary>
    /// Brush used for drawing hands that are currently tracked as opened
    /// </summary>
    private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

    /// <summary>
    /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
    /// </summary>
    private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

    private Igs _igs;
    private WriteableBitmap bitmap = null;
    /// <summary>
    ///     Drawing group for skeleton rendering output
    /// </summary>
    private DrawingGroup _drawingGroup;

    /// <summary>
    ///     Drawing image that we will display
    /// </summary>
    private DrawingImage _imageSource;
    /// <summary>
    /// Drawing image only of the skeleton components
    /// </summary>
    private DrawingImage imageSourceSkeleton;
    private String _input;
    /// <summary>
    /// If activated, the 3D view of the room
    /// </summary>
    private Room3DView _3Dview;
    /// <summary>
    /// the RGB view of the kinect cameras view
    /// </summary>
    private RGB_SkelpointView _RGBView;
    /// <summary>
    ///     Active Kinect sensor
    /// </summary>
    private KinectSensor _sensor;

    /// <summary>
    /// A MultiSourceFrameReader for reading the RGB and body data of the kinect
    /// </summary>
    private MultiSourceFrameReader multiFrameReader = null;

    /// <summary>
    /// the coordinateMapper to map the coordinates get by the camera to different view spaces
    /// </summary>
    private CoordinateMapper coordinateMapper = null;

    /// <summary>
    /// indicator if the 3D view of the room is active.
    /// </summary>
    private bool _3dviewIsAktive { get; set; }
    /// <summary>
    /// indicator if a skeleton is build or not
    /// </summary>
    private bool _ifSkeletonisBuild { get; set; }

    private float roomWidth { get; set; }
    private float roomDepth { get; set; }
    private float roomHeight { get; set; }
    /// <summary>
    /// the ImageSource for the colorimage 
    /// </summary>
    public ImageSource ImageSourceColor
    {
        get
        {
            return this.bitmap;
        }
    }
    /// <summary>
    /// the ImageSource for the image of the skeletonPositions
    /// </summary>
    public ImageSource ImageSourceSkeleton
    {
        get
        {
            return this.imageSourceSkeleton;
        }
    }

    /// <summary>
    /// Initializes the MainWindow components
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

    }

    /// <summary>
    ///     Draws indicators to show which edges are clipping skeleton data
    /// </summary>
    /// <param name="body">skeleton to draw clipping information for</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    private static void RenderClippedEdges(Body body, DrawingContext drawingContext)
    {
        if (body.ClippedEdges.HasFlag(FrameEdges.Bottom))
        {
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
        }

        if (body.ClippedEdges.HasFlag(FrameEdges.Top))
        {
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                new Rect(0, 0, RenderWidth, ClipBoundsThickness));
        }

        if (body.ClippedEdges.HasFlag(FrameEdges.Left))
        {
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                new Rect(0, 0, ClipBoundsThickness, RenderHeight));
        }

        if (body.ClippedEdges.HasFlag(FrameEdges.Right))
        {
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
        }


    }

    /// <summary>
    ///     Execute startup tasks
    /// </summary>
    /// <param name="sender">object sending the event</param>
    /// <param name="e">event arguments</param>
    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        // Create the drawing group we'll use for drawing
        _drawingGroup = new DrawingGroup();

        // Create an image source that we can use in our image control
        _imageSource = new DrawingImage(_drawingGroup);
        DataContext = this;
        // Display the drawing using our image control

        _3dviewIsAktive = false;
        _ifSkeletonisBuild = false;

        xmlFilesControl();

        _igs = Initializer.InitializeIgs();
        fillFieldsGUI();

        this.coordinateMapper = _igs.Tracker.Sensor.CoordinateMapper;
        _igs.devInit = true;

        _sensor = _igs.Tracker.Sensor;
        if (_sensor != null)
        {
            this.multiFrameReader = _igs.Tracker.Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            this.multiFrameReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;
        }

        String[] roomText = XMLComponentHandler.readRoomComponents();
        roomWidth = float.Parse(roomText[0]);
        roomDepth = float.Parse(roomText[2]);
       
        
        FrameDescription ColorframeDescription = _igs.Tracker.Sensor.ColorFrameSource.FrameDescription;
        


    
        // allocate space to put the pixels being received
        this.pixels = new byte[ColorframeDescription.Width * ColorframeDescription.Height * this.bytesPerPixel];

        // create the bitmap to display
        this.bitmap = new WriteableBitmap(ColorframeDescription.Width, ColorframeDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        FrameDescription depthFrameDescription = _igs.Tracker.Sensor.DepthFrameSource.FrameDescription;
        this.displayWidth = depthFrameDescription.Width;
        this.displayHeight = depthFrameDescription.Width;

        this.imageSourceSkeleton = new DrawingImage(this._drawingGroup);

        

      
    }

    /// <summary>
    ///     Execute shutdown tasks
    /// </summary>
    /// <param name="sender">object sending the event</param>
    /// <param name="mouseButtonEventArgs"></param>
    private void WindowClosing(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        _igs.Tracker.ShutDown();
        Environment.Exit(1);
    }
    /// <summary>
    /// This reads an arriving MultiSourceFrameArrived event and uses the data to draw the 
    /// color and the skeleton image and also calls the Room3DView to visualize the skeletons in 3D
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
    {

        List<ModelVisual3D> models = new List<ModelVisual3D>();
        List<ulong> skelIDs = new List<ulong>();

        ColorFrame frameColor = e.FrameReference.AcquireFrame().ColorFrameReference.AcquireFrame();
        BodyFrame bodyFrame = e.FrameReference.AcquireFrame().BodyFrameReference.AcquireFrame();

        if (frameColor != null)
        {
            // ColorFrame is IDisposable
            using (frameColor)
            {


                FrameDescription frameDescriptionColor = frameColor.FrameDescription;

                // update status unless last message is sticky for a whil

                // verify data and write the new color frame data to the display bitmap
                if ((frameDescriptionColor.Width == this.bitmap.PixelWidth) && (frameDescriptionColor.Height == this.bitmap.PixelHeight))
                {
                    if (frameColor.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        frameColor.CopyRawFrameDataToArray(this.pixels);
                    }
                    else
                    {
                        frameColor.CopyConvertedFrameDataToArray(this.pixels, ColorImageFormat.Bgra);
                    }

                    this.bitmap.WritePixels(
                    new Int32Rect(0, 0, frameDescriptionColor.Width, frameDescriptionColor.Height),
                    this.pixels,
                    frameDescriptionColor.Width * this.bytesPerPixel, 0);
                    if (_RGBView != null)
                    {
                        _RGBView.ColorImage.Source = this.bitmap;
                    }

                }
            }
        }
        Body[] bodies = new Body[0];

        if (bodyFrame != null)
        {
            using (bodyFrame)
            {
                bodies = new Body[bodyFrame.BodyCount];

                using (DrawingContext dc = this._drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    // dc.DrawImage(
                    // BitmapSource.Create(colorFrameDescription.Width, colorFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null,
                    //                    pixels, stride), new Rect(0.0, 0.0, RenderWidth, RenderHeight));


                    bodyFrame.GetAndRefreshBodyData(bodies);
                    if (bodies.Length != 0)
                    {
                        foreach (Body body in bodies)
                        {
                           
                            foreach (TrackedSkeleton ts in _igs.Tracker.Bodies)
                            {
                                if ((int)body.TrackingId == ts.Id)
                                {
                                    RenderClippedEdges(body, dc);

                                    if (body.IsTracked)
                                    {
                                        //this.DrawClippedEdges(body, dc);

                                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                                        // convert the joint points to depth (display) space
                                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                                        foreach (JointType jointType in joints.Keys)
                                        {
                                            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(joints[jointType].Position);
                                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                                        }

                                        this.DrawBody(joints, jointPoints, dc);

                                        this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                                        this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                                        if (_RGBView != null)
                                        {
                                            _RGBView.BodyImage.Source = imageSourceSkeleton;
                                        }
                                        if (_3Dview != null)
                                        {
                                            _3Dview.createBody(body);
                                            skelIDs.Add(body.TrackingId);
                                        }
                                    }
                                    this._drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                }

                            }
                        }

                    }

                    if (_3Dview != null)
                    {
                        // check if trackedskeletons of counter == shown skeletons in 3D
                        int[] notFound = new int[6];
                        bool foundID = false;

                        for (int j = 0; j < _3Dview.IDList.Count; j++)
                        {

                            for (int i = 0; i < _igs.Tracker.Bodies.Count; i++)
                            {
                                if (_3Dview.IDList[j] == _igs.Tracker.Bodies[i].Id)
                                {
                                    foundID = true;
                                    break;
                                }
                            }

                            if (foundID == false)
                            {
                                _3Dview.mainViewport.Children.Remove(_3Dview.skelList[j]);
                                _3Dview.mainViewport.Children.Remove(_3Dview.skelRayList[j]);
                                _3Dview.IDList[j] = -1;
                                _3Dview.IDListNullSpaces[j] = true;
                            }
                            foundID = false;
                        }
                    }

                }
                // prevent drawing outside of our render area
                _drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
    }




    /// <summary>
    ///     Draws a skeleton's bones and joints
    /// </summary>
    /// <param name="body">skeleton to draw</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    /// <param name="depth">depth information</param>
    private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
    {
        // Draw the bones

        // Torso
        this.DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, drawingContext);

        // Right Arm    
        this.DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, drawingContext);

        // Left Arm
        this.DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, drawingContext);

        // Right Leg
        this.DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, drawingContext);

        // Left Leg
        this.DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, drawingContext);
        this.DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, drawingContext);

        // Draw the joints
        foreach (JointType jointType in joints.Keys)
        {
            Brush drawBrush = null;

            TrackingState trackingState = joints[jointType].TrackingState;

            if (trackingState == TrackingState.Tracked)
            {
                drawBrush = this._trackedJointBrush;

            }
            else if (trackingState == TrackingState.Inferred)
            {
                drawBrush = this._inferredJointBrush;

            }

            if (drawBrush != null)
            {
                drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);

            }
        }
    }

    /// <summary>
    ///     Maps a SkeletonPoint to lie within our render space and converts to Point
    /// </summary>
    /// <param name="bodyPoint">point to map</param>
    /// <returns>mapped point</returns>
    //private Point SkeletonPointToScreen(Body bodyPoint)
    //{ 
    //    // Convert point to depth space.  
    //    // We are not using depth directly, but we do want the points in our 640x480 output resolution.
    //    DepthImagePoint depthPoint = CoordinateMapper.MapSkeletonPointToDepthPoint(bodyPoint,
    //                                                                                      DepthImageFormat
    //                                                                                          .Resolution640x480Fps30);
    //    ColorImagePoint color = _sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, depthPoint, ColorImageFormat.RawBayerResolution640x480Fps30);


    //    return new Point(color.X, color.Y);
    //}

    /// <summary>
    ///     Draws a bone line between two joints
    /// </summary>
    /// <param name="body">skeleton to draw bones from</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    /// <param name="jointType0">joint to start drawing from</param>
    /// <param name="jointType1">joint to end drawing at</param>
    /// <param name="depth">depth information</param>
    private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext)
    {
        Joint joint0 = joints[jointType0];
        Joint joint1 = joints[jointType1];

        // If we can't find either of these joints, exit
        if (joint0.TrackingState == TrackingState.NotTracked ||
            joint1.TrackingState == TrackingState.NotTracked)
        {
            return;
        }

        // Don't draw if both points are inferred
        if (joint0.TrackingState == TrackingState.Inferred &&
            joint1.TrackingState == TrackingState.Inferred)
        {
            return;
        }

        // We assume all drawn bones are inferred unless BOTH joints are tracked
        Pen drawPen = this.inferredBonePen;
        if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
        {
            drawPen = this._trackedBonePen;
        }

        drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
    }



    /// <summary>
    /// The method triggered by a click on the change plugwise adress button.
    /// Reads the host, port, and path in the belonging text boxes of the mainwindow and writes them 
    /// into the xml file and propagates them to all plugwises in the IGS.
    /// </summary>
    /// <param name="sender">The object triggered the event</param>
    /// <param name="e">The RoutetEventArgs</param>
    private void Change_Plugwise_Adress_Button_Click(object sender, RoutedEventArgs e)
    {

        String[] parameter = new String[3];

        parameter[0] = Plugwise_host.Text;
        parameter[1] = Plugwise_port.Text;
        parameter[2] = Plugwise_path.Text;

        writePlugwiseAdressesToXMLandDevices(parameter);

    }
    /// <summary>
    /// The method triggered by a click on the 3DView button.
    /// It opens a new window with a 3D view to vizualize the Room, its devices(by their coordinate balls) and users with
    /// active gesture control.
    /// </summary>
    /// <param name="sender">The object who triggered the event</param>
    /// <param name="e">The RoutedEventArgs</param>
    private void _3DViewButton_Click(object sender, RoutedEventArgs e)
    {

        _3Dview = new Room3DView(_igs.classification.knnClassifier.samples, _igs.classification.knnClassifier.devicesRepresentation, _igs.Transformer);
        _3Dview.SetKinectCamera(_igs.IGSKinect);
        _3Dview.ClipToBounds = false;
        _3Dview.mainViewport.Effect = null;
        String[] roomText = XMLComponentHandler.readRoomComponents();
        _3Dview.createRoom(float.Parse(roomText[0]), float.Parse(roomText[1]), float.Parse(roomText[2]));
        _3Dview.Show();
    }

    /// <summary>
    /// The method which is triggered by the kinect replace button.
    /// It reads the information specified in the textboxes of the kinect fields X,Y,Z,Tdeg and Hdeg.
    /// Those will be written in the newPosition array then in the config.xml and to the devKinect object 
    /// and also the CoordinateTransformer will be updated to have the right position.
    /// Additionally the old and new place will be printed to the console.
    /// </summary>
    /// <param name="sender">The object which triggered the event</param>
    /// <param name="e">The RoutedEventArgs</param>
    private void Kinect_Replace_Button_Click(object sender, RoutedEventArgs e)
    {
        String oldPlace = "";
        String newPlace = "";
        String logEntry = "";
        String[] newPosition = new String[5];

        String x = Kinect_X.Text;
        newPosition[0] = x;
        String y = Kinect_Y.Text;
        newPosition[1] = y;
        String z = Kinect_Z.Text;
        newPosition[2] = z;
        String Tdeg = Kinect_TiltAngle_Textbox.Text;
        newPosition[3] = Tdeg;
        String Hdeg = Kinect_Roomorientation.Text;
        newPosition[4] = Hdeg;

        oldPlace = "Old Koords: " +
            "X: " + _igs.IGSKinect.ball.Centre.X + " " +
            "Y: " + _igs.IGSKinect.ball.Centre.Y + " " +
            "Z: " + _igs.IGSKinect.ball.Centre.Z + " " +
            "H°: " + _igs.IGSKinect.roomOrientation;

        newPlace = "New Koords: " +
            "X: " + x + " " +
            "Y: " + y + " " +
            "Z: " + z + " " +
            "H°: " + Hdeg;

        float i_X = float.Parse(x);
        float i_Y = float.Parse(y);
        float i_Z = float.Parse(z);
        double orientation = double.Parse(Hdeg);

        XMLComponentHandler.saveKinectPosition(newPosition);
        Vector3D newCenter = new Vector3D();

        newCenter.X = i_X;
        newCenter.Y = i_Y;
        newCenter.Z = i_Z;


        _igs.IGSKinect.roomOrientation = orientation;
        _igs.IGSKinect.ball.Centre = newCenter;

        if (_igs.Tracker.Sensor != null)
        {
            _igs.Transformer.calculateRotationMatrix(0, _igs.IGSKinect.roomOrientation);
        }
        _igs.Transformer.transVector = newCenter;

        if (_3Dview != null)
        {
            _3Dview.SetKinectCamera(_igs.IGSKinect);
        }
        logEntry = "Kinectplace changed from: " + oldPlace + " to: " + newPlace;
    }
    /// <summary>
    /// The method triggered by the creat room button.
    /// On click it reads the information in the textboxes belong to the width, depth, heigth of the room and
    /// writes them in the roomData array.
    /// After that the roompart of the config.xml will be updated and the new room will be changed if the 
    /// 3D view is aktive.
    /// </summary>
    /// <param name="sender">the object which triggered the event</param>
    /// <param name="e">The RoutedEventArgs</param>
    private void CreateRoom_Button_Click(object sender, RoutedEventArgs e)
    {
        float width = 0;
        float depth = 0;
        float height = 0;
        String[] roomData = new String[3];

        roomData[0] = Room_Width.Text;
        roomData[2] = Room_Depth.Text;
        roomData[1] = Room_Height.Text;
        width = float.Parse(Room_Width.Text);
        depth = float.Parse(Room_Depth.Text);
        height = float.Parse(Room_Height.Text);
        roomWidth = width;
        roomDepth = depth;
        XMLComponentHandler.saveRoomPosition(roomData);
        _igs.classification.collector.calcRoomModel.setRoomMeasures(width, depth, height);
        if (_3Dview != null)
        {
            _3Dview.createRoom(width, depth, height);
           
        }

    }

    /// <summary>
    /// Draws for each body a sphere around their hand indicating if the hand is closed, opened or has a pointing finger(lasso)
    /// This method is provided by the Microsoft examples.
    /// </summary>
    /// <param name="handState">The state of the hand(close, opened, lasso)</param>
    /// <param name="handPosition">The position of the hand</param>
    /// <param name="drawingContext">The drawingcontext</param>
    private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
    {
        switch (handState)
        {
            case HandState.Closed:
                drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                break;

            case HandState.Open:
                drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                break;

            case HandState.Lasso:
                drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                break;
        }
    }


    /// <summary>
    /// This method is triggered by the Open RGB Window Button.
    /// It opens a new Window which shows the rgb view of the kinect with overlaying body joints.
    /// </summary>
    /// <param name="sender">The object which triggered the event</param>
    /// <param name="e">The RoutedEventArgs</param>
    private void RGB_Body_WIndow_Button_Click(object sender, RoutedEventArgs e)
    {
        // create the bitmap to display

        _RGBView = new RGB_SkelpointView();

        _RGBView.Show();
    }
    /// <summary>
    /// Builds the regular adresses for the plugwises with the passed components
    /// </summary>
    /// <param name="components">The components of the plugwise adressin ascending order: Host, Port, Path</param>
    /// <returns></returns>
    private String buildPlugwiseString(String[] components)
    {
        String PlugWiseAdress = "http://" + components[0] + ":" + components[1] + "/" + components[2] + "/";

        return PlugWiseAdress;
    }


    /// <summary>
    /// This method wraps the building of the plugwise adress, the distribution to all plugwises and 
    /// the update function for the plugwise components in the config.xml.
    /// </summary>
    /// <param name="components">The components of the plugwise adress: Host, Port, Path</param>
    private void writePlugwiseAdressesToXMLandDevices(String[] components)
    {
        String adress = buildPlugwiseString(components);
        XMLComponentHandler.writePlugwiseAdresstoXML(components);

        _igs.Data.Change_PlugWise_Adress(adress);
    }
    /// <summary>
    /// This method fills all textboxes in the MainWindow with their parameters stored in the config.xml
    /// </summary>
    private void fillFieldsGUI()
    {
        String[] PlugComps = XMLComponentHandler.readPlugwiseComponents();

        Plugwise_host.Text = PlugComps[0];
        Plugwise_port.Text = PlugComps[1];
        Plugwise_path.Text = PlugComps[2];

        String[] KinectComps = XMLComponentHandler.readKinectComponents();
        Kinect_X.Text = KinectComps[0];
        Kinect_Y.Text = KinectComps[1];
        Kinect_Z.Text = KinectComps[2];
        Kinect_TiltAngle_Textbox.Text = KinectComps[3];
        Kinect_Roomorientation.Text = KinectComps[4];

        String[] RoomComps = XMLComponentHandler.readRoomComponents();
        Room_Width.Text = RoomComps[0];
        Room_Height.Text = RoomComps[1];
        Room_Depth.Text = RoomComps[2];
    }

    /// <summary>
    /// The method triggered by a click on the create device button on the MainWindow.
    /// The information of the textboxes device type, device name, device adress and device port will be 
    /// read, written into the parameters array in the order read and the device will then be add by the belonging 
    /// method of the IGS.
    /// </summary>
    /// <param name="sender">The object triggered the event</param>
    /// <param name="e">The MouseButtonEventArgs</param>
    private void CreateDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        String[] parameter = new String[4];

        parameter[0] = DeviceType.Text;
        parameter[1] = DeviceName.Text;
        parameter[2] = DeviceAdress.Text;
        parameter[3] = DevicePort.Text;

        _igs.AddDevice(parameter);
    }

    private void trainBatch_Button_Click(object sender, RoutedEventArgs e)
    {
        _igs.classification.retrainClassifier(_igs.classification.knnClassifier.pendingSamples);
        XMLComponentHandler.writeLogEntry("Batch training executed manually");
    }


    private void WCBMP_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_igs.classification.knnClassifier.samples.Count != 0)
        {
            _igs.classification.collector.calcRoomModel.calculateDeviceAreas(_igs.classification.knnClassifier);
            XMLComponentHandler.writeLogEntry("Devices BMP calculated manually");
        }
    }

    private void xmlFilesControl()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml"))
        {
            Initializer.createXMLFile();
        }

        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml"))
        {
            Initializer.createWallProjectionSampleXMLFile();
        }
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\samples.xml"))
        {
            Initializer.createSampleXMLFIle();
        }

        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml"))
        {
            Initializer.createWallProjectionAndPositionSampleXMLFile();
        }
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml"))
        {
            Initializer.createLogFilePerSelect();
        }
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml"))
        {
            Initializer.createLogFilePerSelectSmoothed();
        }

        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\program_log.xml"))
        {
            Initializer.createGeneralLogFile();
        }
    }

    public List<Vector3D[]> createRandomList()
    {
        List<Vector3D[]> l = new List<Vector3D[]>();
        Random r = new Random();
        for (int i = 0; i < 4; i++)
        {
            Vector3D[] vecs = new Vector3D[4];
            for (int j = 0; j < 4; j++)
            {
                vecs[j] = new Vector3D(r.Next(10), r.Next(10), r.Next(10));
            }
            l.Add(vecs);
        }

        return l;
    }

    private void CreateTestFiles_Button_Click(object sender, RoutedEventArgs e)
    {
        _igs.classification.extractAndCreateLists(_igs.Transformer);
    
    }

    
    private void CrossVal_Button_Click(object sender, RoutedEventArgs e)
    {
        Crossvalidator crossval = new Crossvalidator(_igs.classification, _igs.Data, _igs.Transformer);
        crossval.crossValidateClassifier();
        crossval.crossValidateCollision();
    }

   
}