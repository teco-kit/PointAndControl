using IGS.Classifier;
using IGS.Server.Devices;
using IGS.Server.IGS;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;


namespace IGS.Helperclasses
{
    static class XMLComponentHandler
    {

        public static void writeWallProjectionAndPositionSampleToXML(WallProjectionAndPositionSample sample)
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml");
            XmlNode node = docConfig.SelectSingleNode("/devices");

            foreach (XmlNode deviceNode in node.ChildNodes)
            {

                if (deviceNode.FirstChild.InnerText == sample.sampledeviceIdentifier && deviceNode.FirstChild.Name == "deviceIdentifier")
                {
                    if (deviceNode.ChildNodes[1].Name == "samples")
                    {
                        
                        XmlElement xmlSample = docConfig.CreateElement("sample");
                        XmlElement samplePos = docConfig.CreateElement("wallPosition");
                        XmlElement posX = docConfig.CreateElement("X");
                        posX.InnerText = sample.wallPositionX.ToString();
                        samplePos.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                        posY.InnerText = sample.wallPositionY.ToString();
                        samplePos.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                        posZ.InnerText = sample.wallPositionZ.ToString();
                        samplePos.AppendChild(posZ);
                        xmlSample.AppendChild(samplePos);

                        XmlElement writepersonPosition = docConfig.CreateElement("personPosition");
                        XmlElement writeppX = docConfig.CreateElement("X");
                        writeppX.InnerText = sample.personPositionX.ToString();
                        writepersonPosition.AppendChild(writeppX);
                        XmlElement writeppY = docConfig.CreateElement("Y");
                        writeppY.InnerText = sample.personPositionY.ToString();
                        writepersonPosition.AppendChild(writeppY);
                        XmlElement writeppZ = docConfig.CreateElement("Z");
                        writeppZ.InnerText = sample.personPositionZ.ToString();
                        writepersonPosition.AppendChild(writeppZ);
                        xmlSample.AppendChild(writepersonPosition);
                        //xmlPosition.SetAttribute("X:", sample.x.ToString());
                        //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                        //xmlPosition.SetAttribute("Z:", sample.z.ToString());
                        deviceNode.ChildNodes[1].AppendChild(xmlSample);
                        
                        docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml");
                        return;
                    }
                    
                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement deviceIdentifier = docConfig.CreateElement("deviceIdentifier");
            deviceIdentifier.InnerText = sample.sampledeviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samples");
            XmlElement newSample = docConfig.CreateElement("sample");
            XmlElement samplePosition = docConfig.CreateElement("wallPosition");
            XmlElement X = docConfig.CreateElement("X");
            X.InnerText = sample.wallPositionX.ToString();
            samplePosition.AppendChild(X);
            XmlElement Y = docConfig.CreateElement("Y");
            Y.InnerText = sample.wallPositionY.ToString();
            samplePosition.AppendChild(Y);
            XmlElement Z = docConfig.CreateElement("Z");
            Z.InnerText = sample.wallPositionZ.ToString();
            samplePosition.AppendChild(Z);
            XmlElement personPosition = docConfig.CreateElement("personPosition");
            XmlElement ppX = docConfig.CreateElement("X");
            ppX.InnerText = sample.personPositionX.ToString();
            personPosition.AppendChild(ppX);
            XmlElement ppY = docConfig.CreateElement("Y");
            ppY.InnerText = sample.personPositionY.ToString();
            personPosition.AppendChild(ppY);
            XmlElement ppZ = docConfig.CreateElement("Z");
            ppZ.InnerText = sample.personPositionZ.ToString();
            personPosition.AppendChild(ppZ);


            newSample.AppendChild(samplePosition);
            newSample.AppendChild(personPosition);
            samplePositions.AppendChild(newSample);
            newDev.AppendChild(deviceIdentifier);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml");
            return;
        }


        public static void writeWallProjectionAndPositionSampleToXML(WallProjectionAndPositionSample sample, String fileName)
        {


            String p = AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName + ".xml";


            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(p);
            XmlNode node = docConfig.SelectSingleNode("/devices");

            foreach (XmlNode deviceNode in node.ChildNodes)
            {

                if (deviceNode.FirstChild.InnerText == sample.sampledeviceIdentifier && deviceNode.FirstChild.Name == "deviceIdentifier")
                {
                    if (deviceNode.ChildNodes[1].Name == "samples")
                    {

                        XmlElement xmlSample = docConfig.CreateElement("sample");
                        XmlElement samplePos = docConfig.CreateElement("wallPosition");
                        XmlElement posX = docConfig.CreateElement("X");
                        posX.InnerText = sample.wallPositionX.ToString();
                        samplePos.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                        posY.InnerText = sample.wallPositionY.ToString();
                        samplePos.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                        posZ.InnerText = sample.wallPositionZ.ToString();
                        samplePos.AppendChild(posZ);
                        xmlSample.AppendChild(samplePos);

                        XmlElement writepersonPosition = docConfig.CreateElement("personPosition");
                        XmlElement writeppX = docConfig.CreateElement("X");
                        writeppX.InnerText = sample.personPositionX.ToString();
                        writepersonPosition.AppendChild(writeppX);
                        XmlElement writeppY = docConfig.CreateElement("Y");
                        writeppY.InnerText = sample.personPositionY.ToString();
                        writepersonPosition.AppendChild(writeppY);
                        XmlElement writeppZ = docConfig.CreateElement("Z");
                        writeppZ.InnerText = sample.personPositionZ.ToString();
                        writepersonPosition.AppendChild(writeppZ);
                        xmlSample.AppendChild(writepersonPosition);
                        deviceNode.ChildNodes[1].AppendChild(xmlSample);

                        docConfig.Save(p);
                        return;
                    }

                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement deviceIdentifier = docConfig.CreateElement("deviceIdentifier");
            deviceIdentifier.InnerText = sample.sampledeviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samples");
            XmlElement newSample = docConfig.CreateElement("sample");
            XmlElement samplePosition = docConfig.CreateElement("wallPosition");
            XmlElement X = docConfig.CreateElement("X");
            X.InnerText = sample.wallPositionX.ToString();
            samplePosition.AppendChild(X);
            XmlElement Y = docConfig.CreateElement("Y");
            Y.InnerText = sample.wallPositionY.ToString();
            samplePosition.AppendChild(Y);
            XmlElement Z = docConfig.CreateElement("Z");
            Z.InnerText = sample.wallPositionZ.ToString();
            samplePosition.AppendChild(Z);
            XmlElement personPosition = docConfig.CreateElement("personPosition");
            XmlElement ppX = docConfig.CreateElement("X");
            ppX.InnerText = sample.personPositionX.ToString();
            personPosition.AppendChild(ppX);
            XmlElement ppY = docConfig.CreateElement("Y");
            ppY.InnerText = sample.personPositionY.ToString();
            personPosition.AppendChild(ppY);
            XmlElement ppZ = docConfig.CreateElement("Z");
            ppZ.InnerText = sample.personPositionZ.ToString();
            personPosition.AppendChild(ppZ);



            newSample.AppendChild(samplePosition);
            newSample.AppendChild(personPosition);
            samplePositions.AppendChild(newSample);
            newDev.AppendChild(deviceIdentifier);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(p);
            return;
        }


       
        public static void writeWallProjectionSampleToXML(WallProjectionSample sample)
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
            XmlNode node = docConfig.SelectSingleNode("/devices");

            foreach (XmlNode deviceNode in node.ChildNodes)
            {

                if (deviceNode.FirstChild.InnerText == sample.sampledeviceIdentifier && deviceNode.FirstChild.Name == "deviceIdentifier")
                {
                    if (deviceNode.ChildNodes[1].Name == "samplePositions")
                    {
                        XmlElement xmlPosition = docConfig.CreateElement("position");
                        XmlElement posX = docConfig.CreateElement("X");
                        posX.InnerText = sample.x.ToString();
                        xmlPosition.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                        posY.InnerText = sample.y.ToString();
                        xmlPosition.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                        posZ.InnerText = sample.z.ToString();
                        xmlPosition.AppendChild(posZ);
                        //xmlPosition.SetAttribute("X:", sample.x.ToString());
                        //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                        //xmlPosition.SetAttribute("Z:", sample.z.ToString());
                        deviceNode.ChildNodes[1].AppendChild(xmlPosition);
                        docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
                        return;
                    }
                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement deviceIdentifier = docConfig.CreateElement("deviceIdentifier");
            deviceIdentifier.InnerText = sample.sampledeviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samplePositions");
            XmlElement position = docConfig.CreateElement("position");
            XmlElement X = docConfig.CreateElement("X");
            X.InnerText = sample.x.ToString();
            position.AppendChild(X);
            XmlElement Y = docConfig.CreateElement("Y");
            Y.InnerText = sample.y.ToString();
            position.AppendChild(Y);
            XmlElement Z = docConfig.CreateElement("Z");
            Z.InnerText = sample.z.ToString();
            position.AppendChild(Z);

            samplePositions.AppendChild(position);
            newDev.AppendChild(deviceIdentifier);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
            return;
        }


        public static void writeWallProjectionSampleToConfig(WallProjectionSample sample)
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
            XmlNode node = docConfig.SelectSingleNode("/config");

            foreach (XmlNode deviceNode in node.ChildNodes)
            {

                if (deviceNode.FirstChild.InnerText == sample.sampledeviceIdentifier && deviceNode.FirstChild.Name == "deviceIdentifier")
                {
                    if (deviceNode.ChildNodes[1].Name == "samplePositions")
                    {
                        XmlElement xmlPosition = docConfig.CreateElement("position");
                        XmlElement posX = docConfig.CreateElement("X");
                        posX.InnerText = sample.x.ToString();
                        xmlPosition.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                        posY.InnerText = sample.y.ToString();
                        xmlPosition.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                        posZ.InnerText = sample.z.ToString();
                        xmlPosition.AppendChild(posZ);
                        //xmlPosition.SetAttribute("X:", sample.x.ToString());
                        //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                        //xmlPosition.SetAttribute("Z:", sample.z.ToString());
                        deviceNode.ChildNodes[1].AppendChild(xmlPosition);
                        docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
                        return;
                    }
                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement deviceIdentifier = docConfig.CreateElement("deviceIdentifier");
            deviceIdentifier.InnerText = sample.sampledeviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samplePositions");
            XmlElement position = docConfig.CreateElement("position");
            XmlElement X = docConfig.CreateElement("X");
            X.InnerText = sample.x.ToString();
            position.AppendChild(X);
            XmlElement Y = docConfig.CreateElement("Y");
            Y.InnerText = sample.y.ToString();
            position.AppendChild(Y);
            XmlElement Z = docConfig.CreateElement("Z");
            Z.InnerText = sample.z.ToString();
            position.AppendChild(Z);

            samplePositions.AppendChild(position);
            newDev.AppendChild(deviceIdentifier);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
            return;
        }
        public static void writeWallProjectionSampleToXML(WallProjectionSample sample, String fileName)
        {
            String p = AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName + ".xml";
                      
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(p);
            XmlNode node = docConfig.SelectSingleNode("/devices");

            foreach (XmlNode deviceNode in node.ChildNodes)
            {

                if (deviceNode.FirstChild.InnerText == sample.sampledeviceIdentifier && deviceNode.FirstChild.Name == "deviceIdentifier")
                {
                    if (deviceNode.ChildNodes[1].Name == "samplePositions")
                    {
                        XmlElement xmlPosition = docConfig.CreateElement("position");
                        XmlElement posX = docConfig.CreateElement("X");
                        posX.InnerText = sample.x.ToString();
                        xmlPosition.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                        posY.InnerText = sample.y.ToString();
                        xmlPosition.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                        posZ.InnerText = sample.z.ToString();
                        xmlPosition.AppendChild(posZ);
                        //xmlPosition.SetAttribute("X:", sample.x.ToString());
                        //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                        //xmlPosition.SetAttribute("Z:", sample.z.ToString());
                        deviceNode.ChildNodes[1].AppendChild(xmlPosition);
                        docConfig.Save(p);
                        return;
                    }
                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement deviceIdentifier = docConfig.CreateElement("deviceIdentifier");
            deviceIdentifier.InnerText = sample.sampledeviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samplePositions");
            XmlElement position = docConfig.CreateElement("position");
            XmlElement X = docConfig.CreateElement("X");
            X.InnerText = sample.x.ToString();
            position.AppendChild(X);
            XmlElement Y = docConfig.CreateElement("Y");
            Y.InnerText = sample.y.ToString();
            position.AppendChild(Y);
            XmlElement Z = docConfig.CreateElement("Z");
            Z.InnerText = sample.z.ToString();
            position.AppendChild(Z);

            samplePositions.AppendChild(position);
            newDev.AppendChild(deviceIdentifier);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(p);
            return;
        }

        public static List<WallProjectionAndPositionSample> readWallProjectionAndPositionSamplesFromXML()
        {
            List<WallProjectionAndPositionSample> sampleList = new List<WallProjectionAndPositionSample>();

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml");

            XmlNodeList devices = docConfig.SelectSingleNode("/devices").ChildNodes;

            foreach (XmlNode device in devices)
            {
                String knndeviceIdentifier = "";
                foreach (XmlNode prop in device)
                {
                    if (prop.Name.Equals("deviceIdentifier"))
                    {
                        knndeviceIdentifier = prop.InnerText;
                    }
                    else if (prop.Name.Equals("samples"))
                    {
                        foreach (XmlNode sample in prop.ChildNodes)
                        {
                            WallProjectionAndPositionSample s = new WallProjectionAndPositionSample(new Point3D(
                                double.Parse(sample.FirstChild.ChildNodes[0].InnerText),
                                double.Parse(sample.FirstChild.ChildNodes[1].InnerText),
                                double.Parse(sample.FirstChild.ChildNodes[2].InnerText)), 

                            new Point3D(
                                double.Parse(sample.ChildNodes[1].ChildNodes[0].InnerText),
                                double.Parse(sample.ChildNodes[1].ChildNodes[1].InnerText),
                                double.Parse(sample.ChildNodes[1].ChildNodes[2].InnerText)
                            ), knndeviceIdentifier);
                            
                            sampleList.Add(s);
                        }
                    }
                }

            }

            return sampleList;
        }

        public static List<WallProjectionSample> readWallProjectionSamplesFromXML()
        {
            List<WallProjectionSample> sampleList = new List<WallProjectionSample>();

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
           
            XmlNodeList devices = docConfig.SelectSingleNode("/devices").ChildNodes;

            foreach (XmlNode sample in devices)
            {
                String knndeviceIdentifier = "";
                foreach (XmlNode prop in sample)
                {
                    if (prop.Name.Equals("deviceIdentifier"))
                    {
                        knndeviceIdentifier = prop.InnerText;
                    }
                    else if (prop.Name.Equals("samplePositions"))
                    {
                        foreach (XmlNode position in prop.ChildNodes)
                        {
                            WallProjectionSample s = new WallProjectionSample(new Point3D(
                                double.Parse(position.ChildNodes[0].InnerText),
                                double.Parse(position.ChildNodes[1].InnerText),
                                double.Parse(position.ChildNodes[2].InnerText)), knndeviceIdentifier);

                            sampleList.Add(s);
                        }
                    }
                }
                
            }

            return sampleList;
        }

        public static void writeSampleToXML(Point3D[] positions, String deviceIdentifier)
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\samples.xml");
            XmlNode node = docConfig.SelectSingleNode("/devices");
            Vector3D dir = Point3D.Subtract(positions[3], positions[2]);
            foreach (XmlNode deviceNode in node.ChildNodes)
            {
                if (deviceNode.FirstChild.InnerText == deviceIdentifier 
                    && deviceNode.FirstChild.Name == "deviceIdentifier"
                    && deviceNode.ChildNodes[1].Name == "samples")
                {
                   
                        XmlElement sample = docConfig.CreateElement("sample");
                        XmlNode xmlSamples = deviceNode.ChildNodes[1];
                       
                        XmlElement xmlPosition = docConfig.CreateElement("position");
                        XmlElement posX = docConfig.CreateElement("X");
                                   posX.InnerText = positions[2].X.ToString();
                                   xmlPosition.AppendChild(posX);
                        XmlElement posY = docConfig.CreateElement("Y");
                                   posY.InnerText = positions[2].Y.ToString();
                                   xmlPosition.AppendChild(posY);
                        XmlElement posZ = docConfig.CreateElement("Z");
                                   posZ.InnerText = positions[2].Z.ToString();
                        xmlPosition.AppendChild(posZ);
                        sample.AppendChild(xmlPosition);
                                
                               
                        XmlElement xmlDirection = docConfig.CreateElement("direction");
                        XmlElement dirX = docConfig.CreateElement("X");
                                   dirX.InnerText = dir.X.ToString();
                                   xmlDirection.AppendChild(dirX);
                        XmlElement dirY = docConfig.CreateElement("Y");
                                   dirY.InnerText = dir.Y.ToString();
                                   xmlDirection.AppendChild(dirY);
                        XmlElement dirZ = docConfig.CreateElement("Z");
                                   dirZ.InnerText = dir.Z.ToString();
                        xmlDirection.AppendChild(dirZ);
                        sample.AppendChild(xmlDirection);
                                
                            

                        
                        deviceNode.ChildNodes[1].AppendChild(sample);

                        docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\samples.xml");
                        return;
                        //xmlPosition.SetAttribute("X:", sample.x.ToString());
                        //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                        //xmlPosition.SetAttribute("Z:", sample.z.ToString());
                    
                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement devName = docConfig.CreateElement("deviceIdentifier");
            devName.InnerText = deviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samples");
            XmlElement newSample = docConfig.CreateElement("sample");


            XmlElement position = docConfig.CreateElement("position");
            XmlElement pX = docConfig.CreateElement("X");
            pX.InnerText = positions[2].X.ToString();
            position.AppendChild(pX);
            XmlElement pY = docConfig.CreateElement("Y");
            pY.InnerText = positions[2].Y.ToString();
            position.AppendChild(pY);
            XmlElement pZ = docConfig.CreateElement("Z");
            pZ.InnerText = positions[2].Z.ToString();
            position.AppendChild(pZ);
            newSample.AppendChild(position);


            XmlElement direction = docConfig.CreateElement("direction");
            XmlElement dX = docConfig.CreateElement("X");
            dX.InnerText = dir.X.ToString();
            direction.AppendChild(dX);
            XmlElement dY = docConfig.CreateElement("Y");
            dY.InnerText = dir.Y.ToString();
            direction.AppendChild(dY);
            XmlElement dZ = docConfig.CreateElement("Z");
            dZ.InnerText = dir.Z.ToString();
            direction.AppendChild(dZ);
            newSample.AppendChild(direction);

            samplePositions.AppendChild(newSample);
            newDev.AppendChild(devName);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\samples.xml");
            return;
        }

        public static void writeSampleToXML(Point3D upPoint, Vector3D direction, String deviceIdentifier, String path)
        {
            XmlDocument docConfig = new XmlDocument();
            
            docConfig.Load(AppDomain.CurrentDomain.BaseDirectory + "\\" + path + ".xml");
            XmlNode node = docConfig.SelectSingleNode("/devices");
            
            foreach (XmlNode deviceNode in node.ChildNodes)
            {
                if (deviceNode.FirstChild.InnerText == deviceIdentifier
                    && deviceNode.FirstChild.Name == "identifier"
                    && deviceNode.ChildNodes[1].Name == "samples")
                {

                    XmlElement sample = docConfig.CreateElement("sample");
                    XmlNode xmlSamples = deviceNode.ChildNodes[1];

                    XmlElement xmlPosition = docConfig.CreateElement("position");
                    XmlElement posX = docConfig.CreateElement("X");
                    posX.InnerText = upPoint.X.ToString();
                    xmlPosition.AppendChild(posX);
                    XmlElement posY = docConfig.CreateElement("Y");
                    posY.InnerText = upPoint.Y.ToString();
                    xmlPosition.AppendChild(posY);
                    XmlElement posZ = docConfig.CreateElement("Z");
                    posZ.InnerText = upPoint.Z.ToString();
                    xmlPosition.AppendChild(posZ);
                    sample.AppendChild(xmlPosition);


                    XmlElement xmlDirection = docConfig.CreateElement("direction");
                    XmlElement dirX = docConfig.CreateElement("X");
                    dirX.InnerText = direction.X.ToString();
                    xmlDirection.AppendChild(dirX);
                    XmlElement dirY = docConfig.CreateElement("Y");
                    dirY.InnerText = direction.Y.ToString();
                    xmlDirection.AppendChild(dirY);
                    XmlElement dirZ = docConfig.CreateElement("Z");
                    dirZ.InnerText = direction.Z.ToString();
                    xmlDirection.AppendChild(dirZ);
                    sample.AppendChild(xmlDirection);




                    deviceNode.ChildNodes[1].AppendChild(sample);

                    docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\" + path + ".xml");
                    return;
                    //xmlPosition.SetAttribute("X:", sample.x.ToString());
                    //xmlPosition.SetAttribute("Y:", sample.y.ToString());
                    //xmlPosition.SetAttribute("Z:", sample.z.ToString());

                }
            }

            XmlElement newDev = docConfig.CreateElement("device");
            XmlElement devName = docConfig.CreateElement("deviceIdentifier");
            devName.InnerText = deviceIdentifier;
            XmlElement samplePositions = docConfig.CreateElement("samples");
            XmlElement newSample = docConfig.CreateElement("sample");


            XmlElement position = docConfig.CreateElement("position");
            XmlElement pX = docConfig.CreateElement("X");
            pX.InnerText = upPoint.X.ToString();
            position.AppendChild(pX);
            XmlElement pY = docConfig.CreateElement("Y");
            pY.InnerText = upPoint.Y.ToString();
            position.AppendChild(pY);
            XmlElement pZ = docConfig.CreateElement("Z");
            pZ.InnerText = upPoint.Z.ToString();
            position.AppendChild(pZ);
            newSample.AppendChild(position);


            XmlElement nDirection = docConfig.CreateElement("direction");
            XmlElement dX = docConfig.CreateElement("X");
            dX.InnerText = direction.X.ToString();
            nDirection.AppendChild(dX);
            XmlElement dY = docConfig.CreateElement("Y");
            dY.InnerText = direction.Y.ToString();
            nDirection.AppendChild(dY);
            XmlElement dZ = docConfig.CreateElement("Z");
            dZ.InnerText = direction.Z.ToString();
            nDirection.AppendChild(dZ);
            newSample.AppendChild(nDirection);

            samplePositions.AppendChild(newSample);
            newDev.AppendChild(devName);
            newDev.AppendChild(samplePositions);

            node.AppendChild(newDev);
            docConfig.Save(AppDomain.CurrentDomain.BaseDirectory + "\\"+ path + ".xml");
            return;
        }
      
        public static void writeLogEntry(String entry)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\program_log.xml";
            XmlDocument docCOnfig = new XmlDocument();
            docCOnfig.Load(path);

            XmlNode logNode = docCOnfig.SelectSingleNode("/log");

            XmlElement xmlLogEntry = docCOnfig.CreateElement("entry");
            xmlLogEntry.SetAttribute("time", DateTime.Now.ToString("HH:mm:ss.fff"));
            xmlLogEntry.SetAttribute("date", DateTime.Now.ToShortDateString());
            XmlElement xmlEntryString = docCOnfig.CreateElement("msg");
            xmlEntryString.InnerText = entry;

            xmlLogEntry.AppendChild(xmlEntryString);
            logNode.AppendChild(xmlLogEntry);
            docCOnfig.Save(path);

        }  

    }
}
