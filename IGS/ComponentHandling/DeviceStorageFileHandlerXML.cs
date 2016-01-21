using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGS.Server.Devices;
using System.Xml;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.IO;
using System.Xml.Linq;

namespace IGS.ComponentHandling
{
    public class DeviceStorageFileHandlerXML
    {
        readonly string DEVICE_SAVE_PATH = AppDomain.CurrentDomain.BaseDirectory + "\\devices.xml";

        public void addDevice(Device dev)
        {
            String[] getType = dev.GetType().ToString().Split('.');
            String[] getID = dev.Id.ToString().Split('_');

            String[] complete = new string[] { (getType[getType.Count() - 1]), dev.Name, dev.address, dev.port };

            addDevice(complete, int.Parse(getID[getID.Count() - 1]));
            
        }

        public void addDevice(string[] parameter, int id_number)
        {

            if (!File.Exists(DEVICE_SAVE_PATH))
            {
                XElement cleanNode = new XElement("devices");
                cleanNode.Save(DEVICE_SAVE_PATH);
            }

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(DEVICE_SAVE_PATH);
            XmlNode rootNode = docConfig.SelectSingleNode("/devices");

            // creates an device node
            XmlElement device = docConfig.CreateElement("device");
            device.SetAttribute("type", parameter[0]);
            rootNode.AppendChild(device);

            XmlNodeList deviceNodes = docConfig.SelectNodes("config/deviceConfiguration/device");

            XmlElement name = docConfig.CreateElement("name");
            name.InnerText = parameter[1];
            deviceNodes[deviceNodes.Count - 1].AppendChild(name);

            XmlElement id = docConfig.CreateElement("id");

            id.InnerText = parameter[0] + "_" + id_number;
            deviceNodes[deviceNodes.Count - 1].AppendChild(id);

            XmlElement form = docConfig.CreateElement("form");
            form.SetAttribute("count", "0");
            deviceNodes[deviceNodes.Count - 1].AppendChild(form);

            XmlElement address = docConfig.CreateElement("address");
            address.InnerText = parameter[2];
            deviceNodes[deviceNodes.Count - 1].AppendChild(address);

            XmlElement port = docConfig.CreateElement("port");
            port.InnerText = parameter[3];
            deviceNodes[deviceNodes.Count - 1].AppendChild(port);

            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");

            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
        }


        /// <summary>
        /// This methods writes for a specific existing device a new ball (position and radius)
        /// in its XML entry.
        /// </summary>
        /// <param name="devId">the id of the device, the new ball will be added to</param>
        /// <param name="radius">the radius of the ball</param>
        /// <param name="ball">the ball of the device</param>
        /// <returns>returns the message if the write process was successful</returns>
        public string addDeviceCoord(string devId, Ball ball)
        {

            if (!File.Exists(DEVICE_SAVE_PATH))
            {
                return "No Devices Available";
            }

            String ret = Properties.Resources.NoCoordAdded; 
         
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(DEVICE_SAVE_PATH);
            XmlNodeList nodeList = docConfig.SelectNodes("/devices");

            foreach (XmlNode node in nodeList)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    if (node.ChildNodes[i].InnerText.Equals(devId))
                    {
                        XmlElement newElement = docConfig.CreateElement("ball");
                        node.ChildNodes[i + 1].Attributes[0].Value =
                            (Convert.ToInt32(node.ChildNodes[i + 1].Attributes[0].Value) + 1).ToString();

                        newElement.SetAttribute("radius", ball.Radius.ToString());
                        newElement.SetAttribute("centerX", ball.Center.X.ToString());
                        newElement.SetAttribute("centerY", ball.Center.Y.ToString());
                        newElement.SetAttribute("centerZ", ball.Center.Z.ToString());
                        node.ChildNodes[i + 1].AppendChild(newElement);

                        ret = Properties.Resources.CoordinatesAdded; 
                        break;
                    }
                }
            }
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");


            return ret;
        }

        public List<Device> readDevices()
        {
            if (!File.Exists(DEVICE_SAVE_PATH))
            {
                XElement cleanNode = new XElement("devices");
                cleanNode.Save(DEVICE_SAVE_PATH);
                return new List<Device>();
            }

            List<Device> devices = new List<Device>();
            XmlTextReader reader = new XmlTextReader(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "device")
                {
                    String type = reader.GetAttribute(0);
                    reader.ReadToDescendant("name");
                    String name = reader.ReadString();
                    reader.ReadToFollowing("id");
                    String id = reader.ReadString();
                    reader.ReadToFollowing("form");
                    String[] ballParams = new String[4];
                    List<Ball> form = new List<Ball>();
                    int count = Convert.ToInt32(reader.GetAttribute(0));
                    for (int o = 0; o < count; o++)
                    {

                        reader.ReadToFollowing("ball");
                        for (int i = 0; i < ballParams.Length; i++)
                        {
                            ballParams[i] = reader.GetAttribute(i);
                        }
                        Point3D vec = new Point3D(Convert.ToDouble(ballParams[1]), Convert.ToDouble(ballParams[2]),
                                               Convert.ToDouble(ballParams[3]));
                        float radius = (float)Convert.ToDouble(ballParams[0]);
                        Ball ball = new Ball(vec, radius);
                        form.Add(ball);
                    }
                    reader.ReadToFollowing("address");
                    String address = reader.ReadString();
                    reader.ReadToFollowing("port");
                    String port = reader.ReadString();

                    Type typeObject = Type.GetType("IGS.Server.Devices." + type);
                    try
                    {
                        object instance = Activator.CreateInstance(typeObject, name, id, form, address, port);
                        devices.Add((Device)instance);
                        Debug.WriteLine(instance.GetType().ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }

            }
            reader.Close();

            return devices;
        }
    }
}
