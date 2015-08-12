using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using System.Xml;

namespace IGS.Server.Location
{
    public class Locator
    {
        public const int MIN_NUMBER_OF_VECTORS = 3;

        private static Line3D[] gLines;

        /// <summary>
        ///     Constructor.\n
        ///     Needs Dataholder, UserTracker and CoordTransform from IGS
        ///     <param name="data">Dataholder</param>
        ///     <param name="tracker">UserTracker</param>
        ///     <param name="transformer">CoordTransform</param>
        /// </summary>
        public Locator(DataHolder data, UserTracker tracker, CoordTransform transformer)
        {
            Data = data;
            Tracker = tracker;
            Transformer = transformer;
        }

        /// <summary>
        ///     A new Device is created.\n
        ///     If possible the device is added to the running program. \n\n
        ///     
        ///     This function is not used anymore.
        ///     <param name="cmd"> cmd from IGS. Format shall be: createNewDevice$<type>$<name>$<address>$<port>  </param>
        ///     <returns>Returns message that is displayed at user device</returns>
        /// </summary>
        public String CreateNewDevice(String cmd)
        {
            String retStr = "";

            //split up cmd string that has the form createNewDevice$<type>$<name>$<address>$<port> 
            String[] commands = cmd.Split(new string[] { "$" }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length < 5)
            {
                return "Please fill in all forms.";
            }
            String type = commands[1]; //start with index 1 since 0 is createNewDevice command string
            String name = commands[2];
            String address = commands[3];
            String port = commands[4];

            double result;
            if (!Double.TryParse(port, out result))
            {
                return "Port needs to be numeric value!";
            }


            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
            XmlNode rootNode = docConfig.SelectSingleNode("/config/deviceConfiguration");
            

            //Create Device node
            XmlElement device = docConfig.CreateElement("device");
            device.SetAttribute("type", type);
            rootNode.AppendChild(device);

            XmlNodeList deviceNodes = docConfig.SelectNodes("/config/deviceConfiguration/device");

            //add attributes
            XmlElement xmlName = docConfig.CreateElement("name");
            xmlName.InnerText = name;
            deviceNodes[deviceNodes.Count - 1].AppendChild(xmlName);

            XmlElement xmlId = docConfig.CreateElement("id");
            int count = 1;
            for (int i = 0; i < Data.Devices.Count; i++)
            {
                String[] devId = Data.Devices[i].Id.Split('_');
                if (devId[0] == type)
                    count++;
            }
            xmlId.InnerText = type + "_" + count;
            deviceNodes[deviceNodes.Count - 1].AppendChild(xmlId);

            XmlElement xmlForm = docConfig.CreateElement("form");
            xmlForm.SetAttribute("count", "0");
            deviceNodes[deviceNodes.Count - 1].AppendChild(xmlForm);

            XmlElement xmlAddress = docConfig.CreateElement("address");
            xmlAddress.InnerText = address;
            deviceNodes[deviceNodes.Count - 1].AppendChild(xmlAddress);

            XmlElement xmlPort = docConfig.CreateElement("port");
            xmlPort.InnerText = port;
            deviceNodes[deviceNodes.Count - 1].AppendChild(xmlPort);

            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");

            //add device to DataHolder Data
            Type typeObject = Type.GetType("IGS.Server.Devices." + type);
            if (typeObject != null){
                try{
                    object instance = Activator.CreateInstance(typeObject, name, xmlId.InnerText, new List<Ball>(),
                                                       address, port);
                    Data.Devices.Add((Device)instance);
                }catch(Exception e){
                    Console.Out.WriteLine(e.ToString());
                }
                    
                    
                retStr = "Device added to deviceConfiguration.xml and devices list";
            }
            else
            {
                retStr = "Device added to deviceConfiguration.xml but not to devices list";
            }

            String logEntry = "HinzugefügtesGerät - " + " Typ: " + type + " Name: " + name + " Id: " + xmlId.InnerText + " Adresse: " + address + " Port: " + port + " Resultat: " + retStr;
            //Igs.writeInLog(logEntry);
            return retStr;
        }

        /// <summary>
        ///     Based on current kinect data, a new position for the device is created.\n
        ///     <param name="devId">device to be changed</param>
        ///     <param name="wlanAdr">wlan address of user that performes change</param>
        ///     <returns>Returns success message, more vectors needed message, or error message according to progress</returns>
        /// </summary>
        public String ChangeDeviceLocation(String devId, String wlanAdr)
        {
            //receive Device from DataHolder Data
            Device tempDevice = Data.getDeviceByID(devId);
            if(tempDevice == null) return "Device with ID ["+devId+"] not found";
            //get pointing vector of user
            User tempUser = Data.GetUserByIp(wlanAdr);
            if (tempUser == null) return "User with ID ["+wlanAdr+"] not found";


            int skeleton = tempUser.SkeletonId;
            if (skeleton <= 0) return "User with ID [" + wlanAdr + "] not tracked";

            Vector3D[] vectors = Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId));

            tempDevice.PositionVectors.Add(vectors);

            //log current body (evaluation)
            //writeToXmlFile(tempUser, tempDevice);


            Console.Out.WriteLine("CurrentList length:" + tempDevice.PositionVectors.Count);
            //set new Position
            return setDeviceLocation(tempDevice);
        }

        public String ChangeDeviceLocation(String devId, List<Vector3D[]> vectorsList)
        {
            //receive Device from DataHolder Data
            Device tempDevice = Data.GetDeviceByName(devId);
            if (tempDevice == null) return "Device with ID [" + devId + "] not found";

            // process collected vectors
            tempDevice.PositionVectors = vectorsList;

            Console.Out.WriteLine("CurrentList length:" + tempDevice.PositionVectors.Count);
            //set new Position
            return setDeviceLocation(tempDevice);
        }


        public String setDeviceLocation(Device dev)
        {
            if (dev == null) return "Gerät nicht gefunden";

            Console.Out.WriteLine("CurrentList length:" + dev.PositionVectors.Count);

            //set new Position
            if (dev.PositionVectors.Count >= MIN_NUMBER_OF_VECTORS) // if enough vectors in list to calculate Position
            {
                List<Line3D> lines = new List<Line3D>();

                foreach (Vector3D[] v in dev.PositionVectors)
                {
                    Vector3D rightDir = Vector3D.Subtract(v[3], v[2]);
                    Vector3D rightWrist = v[3];

                    Line3D line = new Line3D(rightWrist, rightDir);

                    lines.Add(line);
                }
                //Line3D.updateWeight(lines);

                Vector3D position = Locator.cobylaCentralPoint(lines.ToArray());

                // vectors were used for calculation, clear list
                dev.PositionVectors.Clear();

                if (position.Equals(new Vector3D(Double.NaN, Double.NaN, Double.NaN)))
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

            return "Mehr Vektoren für die Berechnung benötigt";

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
            //XmlNode deviceNode = docConfig.SelectSingleNode("/deviceConfiguration/device[@type='" +  + "']");

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

        /// <summary>
        ///     Calculates distance between line and point
        ///     <param name="line">line</param>
        ///     <param name="point">point</param>
        ///     <returns>Returns distance</returns>
        /// </summary>
        private static double distanceBetweenLineAndPoint(Line3D line, Vector3D point)
        {
            //shortes distance from point to line: d = |(point - base) x dir| / |dir|
            return Vector3D.Divide(Vector3D.CrossProduct(Vector3D.Subtract(point, line.Base), line.Direction), line.Direction.Length).Length;
        }

        /// <summary>
        ///     Calculates distance between line and point
        ///     <param name="line">line</param>
        ///     <param name="x">point.x</param>
        ///     <param name="y">point.y</param>
        ///     <param name="z">point.z</param>
        ///     <returns>Returns distance</returns>
        /// </summary>
        private static double distanceBetweenLineAndPoint(Line3D line, Double x, Double y, Double z)
        {
            return Vector3D.Divide(Vector3D.CrossProduct(Vector3D.Subtract(new Vector3D(x, y, z), line.Base), line.Direction), line.Direction.Length).Length;
        }

        /// <summary>
        ///     Finds central point of all lines with COBYLA
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D cobylaCentralPoint(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobyla, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobyla(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2); //* gLines[i].Weight;
            }
            f = sum;

            //implicit requirement con[] >= 0 for all constrains

            con[0] = 30 - Math.Abs(x[0]);
            con[1] = 30 - Math.Abs(x[1]);
            con[2] = 30 - Math.Abs(x[2]);
        }

        /// <summary>
        ///     Finds central point of all lines with COBYLA using weight
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D cobylaCentralPointWithWeight(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };

            var status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 0, 3500, Console.Out);

            if (status == CobylaExitStatus.MaxIterationsReached)
                status = Cobyla.FindMinimum(calfunCobylaWeight, 3, 3, xyz, 0.5, 1.0e-6, 1, 10000, Console.Out);

            if (status == CobylaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static void calfunCobylaWeight(int n, int m, double[] x, out double f, double[] con)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2) * gLines[i].Weight;
            }
            f = sum;

            //implicit requirement con[] >= 0 for all constrains

            con[0] = 30 - Math.Abs(x[0]);
            con[1] = 30 - Math.Abs(x[1]);
            con[2] = 30 - Math.Abs(x[2]);
        }

        /// <summary>
        ///     Finds central point of all lines with BOBYQA
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D bobyqaCentralPoint(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqa, 3, xyz, lBounds, uBounds, -1, -1, -1, 0, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqa(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2); //* gLines[i].Weight;
            }
            return sum;
        }

        /// <summary>
        ///     Finds central point of all lines with BOBYQA using weight
        ///     <param name="list">Array of lines</param>
        ///     <returns>Returns central Point or Vector3D(NaN,NaN,NaN) on error</returns>
        /// </summary>
        private static Vector3D bobyqaCentralPointWithWeight(Line3D[] lines)
        {
            gLines = lines;

            var xyz = new[] { 0.0, 0.0, 0.0 };
            var lBounds = new[] { -30.0, -30.0, -30.0 };
            var uBounds = new[] { 30.0, 30.0, 30.0 };

            var status = Bobyqa.FindMinimum(calfunBobyqaWeight, 3, xyz, lBounds, uBounds, -1, -1, -1, 1, 10000, Console.Out);

            if (status == BobyqaExitStatus.Normal)
                return new Vector3D(xyz[0], xyz[1], xyz[2]);
            else
                return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }

        private static double calfunBobyqaWeight(int n, double[] x)
        {
            //Double[] terms = new Double[gLines.Length];
            Double sum = 0;
            for (int i = 0; i < gLines.Length; i++)
            {
                sum += Math.Pow(distanceBetweenLineAndPoint(gLines[i], x[0], x[1], x[2]), 2) * gLines[i].Weight;
            }
            return sum;
        }

        /// <summary>
        ///     Updates the weight of all Lines in list
        ///     <param name="list">list of lines</param>
        /// </summary>
        private static void updateWeight(List<Line3D> list)
        {
            DateTime currentTime = DateTime.Now;
            //Console.Out.WriteLine("currentTime:"+currentTime.ToString());

            List<Line3D> noWeight = new List<Line3D>();

            foreach (Line3D line in list)
            {
                if (currentTime.Subtract(line.creationTime).TotalMinutes <= 15)
                { line.Weight = 1; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 30)
                { line.Weight = 0.95; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalMinutes <= 45)
                { line.Weight = 0.9; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 1)
                { line.Weight = 0.85; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 2)
                { line.Weight = 0.8; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 6)
                { line.Weight = 0.75; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalHours <= 12)
                { line.Weight = 0.7; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 1)
                { line.Weight = 0.65; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 3)
                { line.Weight = 0.6; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 7)
                { line.Weight = 0.55; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 30)
                { line.Weight = 0.5; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 90)
                { line.Weight = 0.4; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 180)
                { line.Weight = 0.3; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 270)
                { line.Weight = 0.2; continue; }
                else if (currentTime.Subtract(line.creationTime).TotalDays <= 360)
                { line.Weight = 0.1; continue; }
                else
                {
                    line.Weight = 0.0;
                    noWeight.Add(line);
                    continue;
                }
            }

            //remove all lines from list that do not have a weight
            foreach (Line3D line in noWeight)
            {
                list.Remove(line);
            }

        }

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
        public  CoordTransform Transformer { get; set; }
    }
}
