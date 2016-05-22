using Microsoft.Kinect;
using Newtonsoft.Json;
using PointAndControl.Kinect;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PointAndControl.Webserver
{
    //These Classes and its content is based on the Project kinectv2-webserver by Pete D
    //Link: https://github.com/peted70/kinectv2-webserver (21.05.2016)

    public class JointEx
    {
        public Joint Joint { get; set; }
        public ColorSpacePoint ColorSpacePos { get; set; }
    }

    public class ColorFrameData
    {
        public byte[] Data { get; set; }
        public ColorImageFormat Format { get; set; }
    }
    class KinectImagePublisher
    {
        public bool serverRunning { get; private set; }
        KinectSensor kinectSensor { get; set; }
        MultiSourceFrameReader reader { get; set; }
        WebSocketServer imageServer { get; set; }
        ColorFrameData colorData { get; set; }
        byte[] encodedBytes { get; set; }
        Body[] bodies;
        List<Body> trackedBodies { get; set; }
        List<TrackedSkeleton> trackedSkeletons { get; set; }
        List<Dictionary<JointType, JointEx>> mappedJoints { get; set; }
        CameraSpacePoint[] cameraTempPoint { get; set; }
        ColorSpacePoint[] colorTempPoint { get; set; }
        const int NumJoints = 25;
        ColorSpacePoint[] colorTempPoints { get; set; }
        List<ColorSpacePoint[]> bodyTransferData { get; set; }
        uint frameSize { get; set; }

        public KinectImagePublisher(KinectSensor sensor, List<TrackedSkeleton> trackedSkeletons)
        {
            mappedJoints = new List<Dictionary<JointType, JointEx>>();
            colorData = new ColorFrameData();
            cameraTempPoint = new CameraSpacePoint[1];
            colorTempPoint = new ColorSpacePoint[1];
            colorTempPoints = new ColorSpacePoint[NumJoints];
            bodyTransferData = new List<ColorSpacePoint[]>();
            kinectSensor = sensor;
            trackedBodies = new List<Body>();
            this.trackedSkeletons = trackedSkeletons;

            var description = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            frameSize = description.BytesPerPixel * description.LengthInPixels;
            colorData.Data = new byte[frameSize];
            colorData.Format = ColorImageFormat.Bgra;

            reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Color);

            
        }



        private void msfr_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (e.FrameReference == null)
                return;
            var multiFrame = e.FrameReference.AcquireFrame();
            if (multiFrame == null)
                return;

            bool colorRead = false;
            FrameDescription fd = null;
            if (multiFrame.ColorFrameReference != null)
            {
                using (var cf = multiFrame.ColorFrameReference.AcquireFrame())
                {
                    fd = cf.ColorFrameSource.FrameDescription;
                    cf.CopyConvertedFrameDataToArray(colorData.Data, colorData.Format);
                    colorRead = true;
                }
            }
            bool bodyRead = false;
            if (multiFrame.BodyFrameReference != null)
            {
                using (var bf = multiFrame.BodyFrameReference.AcquireFrame())
                {
                    bf.GetAndRefreshBodyData(bodies);
                    bodyRead = true;
                }
            }

            byte[] data = null;
            if (colorRead == true)
            {
                data = SendColorData(colorData, fd);
            }
            string bodyData = null;
            if (bodyRead == true)
            {
                bodyData = SerialiseBodyData();
            }

            var sessions = imageServer.GetAllSessions();
            if (sessions.Count() < 1)
                return;

            foreach (var session in sessions)
            {
                if (data != null)
                {
                    session.Send(data, 0, data.Length);
                }
                if (bodyData != null)
                {
                    session.Send(bodyData);
                }
            }
        }

        private string SerialiseBodyData()
        {
            trackedBodies.Clear();

            foreach(Body b in bodies)
            {
                foreach(TrackedSkeleton t in trackedSkeletons)
                {
                    if ((int)b.TrackingId == t.Id)
                    {
                        trackedBodies.Add(b);
                        break;
                    }
                }
            }

            if (trackedBodies.Count() < 1)
                return null;

            bodyTransferData.Clear();
            foreach (var body in trackedBodies)
            {
                var list = body.Joints.Select(j => j.Value).Select(p => p.Position).ToArray();
                kinectSensor.CoordinateMapper.MapCameraPointsToColorSpace(list, colorTempPoints);
                bodyTransferData.Add(colorTempPoints);
            }

            var str = JsonConvert.SerializeObject(bodyTransferData);
            return str;
        }

        private byte[] SendColorData(ColorFrameData data, FrameDescription fd)
        {
            if (data == null)
                return null;

            var dpiX = 96.0;
            var dpiY = 96.0;
            var pixelFormat = PixelFormats.Bgra32;
            var bytesPerPixel = (pixelFormat.BitsPerPixel) / 8;
            var stride = bytesPerPixel * fd.Width;

            var bitmap = BitmapSource.Create(fd.Width, fd.Height, dpiX, dpiY,
                                             pixelFormat, null, data.Data, (int)stride);
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var ms = new MemoryStream())
            using (var br = new BinaryReader(ms))
            {
                encoder.Save(ms);
                ms.Flush();
                ms.Position = 0;
                encodedBytes = br.ReadBytes((int)ms.Length);
            }

            return encodedBytes;
        }

        public void start()
        {
            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
            }

            imageServer = new WebSocketServer();

            var config = new ServerConfig();
            config.Name = "kinect";
            config.Port = 2012;
            config.MaxRequestLength = (int)frameSize;

            if (!imageServer.Setup(config)) //Setup with listening port 
            {
                Console.WriteLine("Failed to setup image server!");
                return;
            }


            if (!imageServer.Start())
            {
                Console.WriteLine("Failed to start image server!");
                return;
            }
            serverRunning = true;
            Console.WriteLine("Image Server Started");

            while (serverRunning)
            {

            }

            imageServer.Stop();
            Console.WriteLine("The image server was stopped!");
        }

        public void stop()
        {
            serverRunning = false;
        }
    }
}
