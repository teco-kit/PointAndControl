using IGS.Server.Devices;
using IGS.Server.IGS;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IGS.Helperclasses
{
    static class XMLSkeletonJointRecords
    {

        private struct SortMarker
        {
            public string deviceIdentifier { get; set; }
            public string deviceID { get; set; }
            public List<XmlNode> nodes { get; set; }
        }
       
        
        public static void sortSelectsByDevice(String path)
        {
            
            String p = AppDomain.CurrentDomain.BaseDirectory + "\\" + path + ".xml";
            List<String> stringList = new List<string>();
            List<SortMarker> deviceMarkers = new List<SortMarker>();
            
            XmlDocument docConfig = new XmlDocument();
            bool selectAdded = false;
            if (File.Exists(p))
            {
                docConfig.Load(p);
            }

            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            foreach (XmlNode select in rootNode.ChildNodes)
            {
                String deviceIdentifier = select.Attributes[3].Value;
                String deviceID = select.Attributes[2].Value;
                

                foreach (SortMarker marker in deviceMarkers)
                {
                    if (marker.deviceIdentifier.Equals(deviceIdentifier))
                    {
                        marker.nodes.Add(select);
                        selectAdded = true;
                    }
                }

                if (selectAdded == false)
                {
                    SortMarker marker = new SortMarker();
                    marker.deviceIdentifier = deviceIdentifier;
                    marker.deviceID = deviceID;
                    marker.nodes = new List<XmlNode>();
                    marker.nodes.Add(select);
                    deviceMarkers.Add(marker);
                }
            }


            String pathSorted = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothedSortedByDevice.xml";

            if (File.Exists(pathSorted))
            {
                docConfig.Load(pathSorted);
            }

            rootNode = docConfig.SelectSingleNode("/data");

            foreach (SortMarker marker in deviceMarkers)
            {
                
                XmlElement xmlDevice = docConfig.CreateElement("device");
                xmlDevice.SetAttribute("id", marker.deviceID);
                xmlDevice.SetAttribute("name", marker.deviceIdentifier);
                foreach (XmlNode node in marker.nodes)
                {
                    xmlDevice.AppendChild(node);
                }
                rootNode.AppendChild(xmlDevice);
            }

        }

        public static void writeClassifiedDeviceToLastSelect(Device dev)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml";

            XmlDocument docConfig = new XmlDocument();

            if (File.Exists(path))
            {
                docConfig.Load(path);
            }

            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNode lastSelect = rootNode.LastChild;

            lastSelect.Attributes[2].Value = dev.Id;
            lastSelect.Attributes[3].Value = dev.Name;

            docConfig.Save(path);

            path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml";

            //add device to configuration XML
            docConfig = new XmlDocument();


            if (File.Exists(path))
            {
                docConfig.Load(path);
            }

            rootNode = docConfig.SelectSingleNode("/data");
            lastSelect = rootNode.LastChild;
            lastSelect.Attributes[2].Value = dev.Id;
            lastSelect.Attributes[3].Value = dev.Name;
            docConfig.Save(path);
        }

        public static void deleteLastUserSkeletonSelected()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml";

            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();

            if (File.Exists(path))
            {
                docConfig.Load(path);
            }

            XmlNode rootNode = docConfig.SelectSingleNode("/data");
            int selected = int.Parse(rootNode.Attributes[0].Value);
            rootNode.Attributes[0].Value = (selected - 1).ToString();
            rootNode.RemoveChild(rootNode.LastChild);
            docConfig.Save(path);

            path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml";

            //add device to configuration XML
            docConfig = new XmlDocument();

            if (File.Exists(path))
            {
                docConfig.Load(path);
            }

            rootNode = docConfig.SelectSingleNode("/data");

            rootNode.RemoveChild(rootNode.LastChild);
            selected = int.Parse(rootNode.Attributes[0].Value);
            rootNode.Attributes[0].Value = (selected - 1).ToString();
            docConfig.Save(path);

        }

        public static bool writeUserjointsPerSelectSmoothed(int id, List<Body[]> bodiesList)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml";

            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();


            docConfig.Load(path);



            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            int select = int.Parse(rootNode.Attributes[0].Value);
            XmlElement xmlSelect = docConfig.CreateElement("select");
            int skeletonCounter = 0;
            xmlSelect.SetAttribute("time", DateTime.Now.ToString("HH:mm:ss"));
            xmlSelect.SetAttribute("date", DateTime.Now.ToShortDateString());
            xmlSelect.SetAttribute("devID", "");
            xmlSelect.SetAttribute("devName", "");

            foreach (Body[] bodies in bodiesList)
            {
                foreach (Body body in bodies)
                {
                    if ((int)body.TrackingId == id)
                    {
                        XmlElement xmlSkeleton = docConfig.CreateElement("skeleton");
                        foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
                        {
                            XmlElement xmlJoint = docConfig.CreateElement("joint");
                            xmlJoint.SetAttribute("type", jointType.ToString());

                            xmlJoint.SetAttribute("X", body.Joints[jointType].Position.X.ToString());
                            xmlJoint.SetAttribute("Y", body.Joints[jointType].Position.Y.ToString());
                            xmlJoint.SetAttribute("Z", body.Joints[jointType].Position.Z.ToString());
                            xmlSkeleton.AppendChild(xmlJoint);
                        }
                        xmlSelect.AppendChild(xmlSkeleton);
                        skeletonCounter++;
                        break;

                    }
                }
            }

            xmlSelect.SetAttribute("skelCount", skeletonCounter.ToString());
            rootNode.AppendChild(xmlSelect);
            rootNode.Attributes[0].Value = (select++).ToString();
            docConfig.Save(path);

            return true;
        }
        public static void deleteLastUserSkeletonFromLogXML(Device dev)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFile.xml";

            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();

            if (File.Exists(path))
            {
                docConfig.Load(path);
            }

            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            foreach (XmlNode device in rootNode.ChildNodes)
            {
                if (device.Attributes[0].Value == dev.Id)
                {
                    device.RemoveChild(device.LastChild);
                    docConfig.Save(path);
                    return;
                }
            }

        }

        public static void writeUserJointsPerSelectClick(Body b)
        {
            if (b == null)
            {
                Console.Out.WriteLine("No Body found, cannot write to xml");
                return;
            }
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml";

            //add device to configuration XML
            XmlDocument docConfig = new XmlDocument();


            docConfig.Load(path);



            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            int select = int.Parse(rootNode.Attributes[0].Value);



            XmlElement xmlSelect = docConfig.CreateElement("select");
            XmlElement xmlSkeleton = docConfig.CreateElement("skeleton");
            xmlSelect.SetAttribute("time", DateTime.Now.ToString("HH:mm:ss"));
            xmlSelect.SetAttribute("date", DateTime.Now.ToShortDateString());
            xmlSelect.SetAttribute("devID", "");
            xmlSelect.SetAttribute("devName", "");


            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                XmlElement xmlJoint = docConfig.CreateElement("joint");
                xmlJoint.SetAttribute("type", jointType.ToString());

                xmlJoint.SetAttribute("X", b.Joints[jointType].Position.X.ToString());
                xmlJoint.SetAttribute("Y", b.Joints[jointType].Position.Y.ToString());
                xmlJoint.SetAttribute("Z", b.Joints[jointType].Position.Z.ToString());
                xmlSkeleton.AppendChild(xmlJoint);

            }
            xmlSelect.AppendChild(xmlSkeleton);
            rootNode.AppendChild(xmlSelect);
            rootNode.Attributes[0].Value = (select+1).ToString();

            docConfig.Save(path);

        }

    }
}
