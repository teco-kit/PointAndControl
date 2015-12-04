using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using IGS.Server.Location;
using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.IO;
using System.Xml;

namespace IGS.Server.IGS
{
    class CollisionMethod : ICoreMethods
    {

        public Locator locator { get; set; }

        /// <summary>
        ///     Stores Dataholder to be able to acces its data or functions.\n
        /// </summary>
        public DataHolder Data { get; set; }
        /// <summary>
        ///     Stores UserTracker to be able to acces its data or functions.\n
        /// </summary>
        public UserTracker Tracker { get; set; }
        /// <summary>
        ///     Stores CoordTransform to be able to acces its data or functions.\n
        /// </summary>
        public CoordTransform Transformer { get; set; }
    
        public CollisionMethod(DataHolder data, UserTracker tracker, CoordTransform transformer)
        {
            this.Data = data;
            this.Tracker = tracker;
            this.Transformer = transformer;

            this.locator = new Locator();
        }

        public List<Device> chooseDevice(User usr, Boolean headBase = false)
        {
            return usr != null ? CollisionDetection.PointSelection(Data.Devices, Transformer.transformJointCoords(Tracker.GetCoordinates(usr.SkeletonId, headBase))) : null;
        }

        public List<DeviceWithLocation> getDevicesInView(User usr)
        {
            if (usr == null)
                return null;

            // [0] is the head and [1] is the hand/wrist
            Point3D[] usrCoords = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(usr.SkeletonId, true));
            //Point3D[] usrCoords = Transformer.transformJointCoords(Tracker.GetCoordinates(usr.SkeletonId, true));
            
            // 90° is everything in front of the user
            // List<Device> devices = CollisionDetection.ConeSelection(Data.Devices, usrCoords, 30);

            // plane of SmartPhone in the hand of the user, used for projection
            Plane3D phonePlane = new Plane3D(usrCoords[1], Point3D.Subtract(usrCoords[1], usrCoords[0]));
            // this results in user perspective rendering
            // Point3D pointOfView = usrCoords[0];
            // or find virtual camera point in front of the smartphone (120mm constant for Nexus 4)
            Point3D pointOfView = phonePlane.origin - (0.12 / phonePlane.normal.Length * phonePlane.normal);

            // reference vector pointing to the ceiling, projected onto plane
            Ray3D screenUp = new Ray3D(pointOfView, Point3D.Add(phonePlane.origin, new Vector3D(0, 0.1, 0)));
            Vector3D upProjection = Point3D.Subtract(phonePlane.rayIntersection(screenUp), phonePlane.origin);

            List<DeviceWithLocation> devLocs = new List<DeviceWithLocation>();

            // compute position
            foreach (Device dev in Data.Devices)
            {
                DeviceWithLocation devLoc = new DeviceWithLocation();
                devLoc.device = dev;
                
                Ray3D devCenter = new Ray3D(pointOfView, dev.getCenterOfGravity());
                Vector3D devProjection = Point3D.Subtract(phonePlane.rayIntersection(devCenter), phonePlane.origin);
                
                // distance from center
                devLoc.radius = devProjection.Length;

                if (Double.IsNaN(devLoc.radius))
                    continue;

                // angle to up direction on plane
                devLoc.angle = Vector3D.AngleBetween(upProjection, devProjection);
                // adjust sign based on comparison between cross product and plane normal
                devLoc.angle *= (Vector3D.DotProduct(Vector3D.CrossProduct(upProjection, devProjection), phonePlane.normal) > 0) ? 1 : -1;

                if (Double.IsNaN(devLoc.angle))
                    continue;

                devLocs.Add(devLoc);
            }

            devLocs.Sort();

            return devLocs;
        }

        public class DeviceWithLocation : IComparable<DeviceWithLocation>
        {
            /// <summary>
            /// reference to the device
            /// </summary>
            public Device device { get; set; }

            /// <summary>
            /// radial distance to the center of the reference plane
            /// </summary>
            public double radius { get; set; }

            /// <summary>
            /// angle to up-vector on reference plane from -180 to +180
            /// </summary>
            public double angle { get; set; }

            public int CompareTo(DeviceWithLocation compareTo)
            {
                return this.radius.CompareTo(compareTo.radius);
            }
        }

        public String train(Device dev)
        {
            if (dev == null) return "Gerät nicht gefunden";

            Console.Out.WriteLine("CurrentList length:" + dev.skelPositions.Count);

            //Line3D.updateWeight(lines);

            //calculate new Position
            if (dev.skelPositions.Count < Locator.MIN_NUMBER_OF_VECTORS)
                return "Mehr Vektoren für die Berechnung benötigt";

            Point3D position = locator.getDeviceLocation(dev.skelPositions);

            // vectors were used for calculation, clear list
            dev.skelPositions.Clear();

            if (position.Equals(new Point3D(Double.NaN, Double.NaN, Double.NaN)))
            {
                //error: advise user to try again
                return "Berechnungsfehler. Bitte erneut versuchen.";
            }

            //change position of device in dataHolder
            List<Ball> balls = new List<Ball>();
            balls.Add(new Ball(position, 0.3f));
            dev.Form = balls;

            //add new location to xml
            //TODO: disabled for testing, should be handled in dataHolder
            //String result = xmlChangeDeviceLocation(tempDevice, position);

            return "Gerät " + dev.Name + " wurde neu plaziert";
        }

        public int getMinVectorsPerDevice()
        {
            return Locator.MIN_NUMBER_OF_VECTORS;
        }

        /// <summary>
        ///     Set new Device Position in configuraion.xml.\n
        ///     <param name="device">device to be changed</param>
        ///     <param name="position">new position of device</param>
        ///     <returns>Returns "" on success, otherwise error message</returns>
        /// </summary>
        private String xmlChangeDeviceLocation(Device device, Vector3D position)
        {
            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
            XmlNode rootNode = docConfig.SelectSingleNode("/config/deviceConfiguration");

            //try to find existing device
            XmlNode deviceNode = null;
            foreach (XmlNode XmlDevice in rootNode.ChildNodes)
            {
                if (XmlDevice.ChildNodes[1].InnerText.Equals(device.Id))
                {
                    deviceNode = XmlDevice;
                }
            }

            if (deviceNode == null)
            {
                return "Error: Device not found in configuration xml";
            }

            //get existing form
            XmlNode oldFormNode = deviceNode.ChildNodes.Item(2);


            //create new form, only 1 ball is used -> old ball is removed
            XmlElement formNode = docConfig.CreateElement("form");
            formNode.SetAttribute("count", "1");

            XmlElement ball = docConfig.CreateElement("ball");
            ball.SetAttribute("radius", "0,4");
            ball.SetAttribute("centerX", position.X.ToString());
            ball.SetAttribute("centerY", position.Y.ToString());
            ball.SetAttribute("centerZ", position.Z.ToString());

            formNode.AppendChild(ball);

            //replace old with new form
            deviceNode.ReplaceChild(formNode, oldFormNode);


            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
            return "";
        }
    }
}
