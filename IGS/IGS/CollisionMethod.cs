using IGS.ComponentHandling;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using IGS.Server.Location;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
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

        private EventLogger logger { get; set; }
    
        public CollisionMethod(DataHolder data, UserTracker tracker, CoordTransform transformer, EventLogger eLogger)
        {
            this.Data = data;
            this.Tracker = tracker;
            this.Transformer = transformer;

            this.locator = new Locator();
            this.logger = eLogger;
        }

        public List<Device> chooseDevice(User usr)
        {
            return usr != null ? CollisionDetection.Calculate(Data.Devices, Transformer.transformJointCoords(Tracker.GetCoordinates(usr.SkeletonId))) : null;
        }

        public String train(Device dev)
        {
            if (dev == null) return Properties.Resources.SpecifiedDeviceNotFound;

            Console.Out.WriteLine("CurrentList length:" + dev.skelPositions.Count);

            //Line3D.updateWeight(lines);

            //calculate new Position
            if (dev.skelPositions.Count < Locator.MIN_NUMBER_OF_VECTORS)
                return Properties.Resources.MoreVectorsRequired;

            Point3D position = locator.getDeviceLocation(dev.skelPositions);

            // vectors were used for calculation, clear list
            dev.skelPositions.Clear();

            if (position.Equals(new Point3D(Double.NaN, Double.NaN, Double.NaN)))
            {
                //error: advise user to try again
                return Properties.Resources.CalculationError;
            }

            //change position of device in dataHolder

            Data.changeDeviceCoordinates(dev.Id, "0,3", position);

            return String.Format(Properties.Resources.DevCoordsReplaced, dev.Id);
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
