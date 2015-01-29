using IGS.KNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;

namespace IGS.Helperclasses
{
    public class SampleExtractor
    {
        public struct rawSample
        {
            public String label;
            public Vector3D position;
            public Vector3D direction;
        }

        public List<rawSample> sampleList { get; set; }
        public SampleExtractor(String rawDataFileName)
        {
            List<rawSample> tmpSampleList = getRawSamplesOfSkeletondata(rawDataFileName);
            if (tmpSampleList != null)
            {
                sampleList = tmpSampleList;
            }
        }

        private List<rawSample> getRawSamplesOfSkeletondata(String fileName)
        {

            //check for every file if exists
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + fileName + ".xml"))
            {
                Console.WriteLine("No such filename");
                return null; ;
            }
            String path = AppDomain.CurrentDomain.BaseDirectory + fileName + ".xml";
            
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            //get the data node (topmost node)
            XmlNode nodeList = docConfig.SelectSingleNode("/data");
            //get the device nodes
            XmlNodeList dataNodes = nodeList.ChildNodes;
            
        
            bool foundDirectionPoint = false;
            bool foundUpPoint = false;
            List<rawSample> rawSampleList = new List<rawSample>();


            // iterate over every devices
            foreach (XmlNode device in dataNodes)
            {
                //get label
                String actualLabel = device.Attributes[1].Value.ToString();

                foreach (XmlNode skeletonNode in device.ChildNodes)
                {
                    Vector3D position = new Vector3D();
                    Vector3D directionPoint = new Vector3D();
                    Vector3D direction = new Vector3D();

                    foreach (XmlNode joint in skeletonNode.ChildNodes)
                    {
                        

                        if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristRight"))
                        {
                            directionPoint.X = Double.Parse(joint.Attributes[1].Value);
                            directionPoint.Y = Double.Parse(joint.Attributes[2].Value);
                            directionPoint.Z = Double.Parse(joint.Attributes[3].Value);
                            foundDirectionPoint = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderRight"))
                        {
                            position.X = Double.Parse(joint.Attributes[1].Value);
                            position.Y = Double.Parse(joint.Attributes[2].Value);
                            position.Z = Double.Parse(joint.Attributes[3].Value);
                            foundUpPoint = true;
                        }
                        if (foundDirectionPoint == true && foundUpPoint == true)
                        {
                            foundDirectionPoint = false;
                            foundUpPoint = false;
                            direction.X = directionPoint.X - position.X;
                            direction.Y = directionPoint.Y - position.Y;
                            direction.Z = directionPoint.Z - position.Z;


                            rawSample sample = new rawSample();
                            sample.label = actualLabel;
                            sample.direction = direction;
                            sample.position = position;
                            rawSampleList.Add(sample);
                            break;
                        }
                    }
                }



            }
            Console.WriteLine("File read to end:" + fileName);
            return rawSampleList;
        }

        public void calculateAndWriteWallProjectionSamples(SampleCollector collector)
        {
            List<WallProjectionSample> wallProjectionSamples = new List<WallProjectionSample>();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                WallProjectionSample sample = collector.calculateSample(rawSample.direction, rawSample.position, rawSample.label);
                wallProjectionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML("rawSampleWallProjection");
            foreach (WallProjectionSample wps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionSampleToXML(wps, "rawSampleWallProjection");
            }
        }

        public void calculateAndWriteWallProjectionAndPositionSamples(SampleCollector collector)
        {
            List<WallProjectionAndPositionSample> wallProjectionSamples = new List<WallProjectionAndPositionSample>();
            WallProjectionSample tmpSample = new WallProjectionSample();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                tmpSample = collector.calculateSample(rawSample.direction, rawSample.position, rawSample.label);

                WallProjectionAndPositionSample sample = new WallProjectionAndPositionSample(tmpSample, new Point3D(rawSample.position.X, rawSample.position.Y, rawSample.position.Z), rawSample.label);
                wallProjectionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML("rawSampleWallProjectionAndPosition");
            foreach (WallProjectionAndPositionSample wpps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(wpps, "rawSampleWallProjectionAndPosition");
            }
        }


        public void calculateAndWriteWallProjectionSamples(List<SampleExtractor.rawSample> rawSampleList, String filePath, SampleCollector collector)
        {
            List<WallProjectionSample> wallProjectionSamples = new List<WallProjectionSample>();
            foreach (SampleExtractor.rawSample rawSample in rawSampleList)
            {
                WallProjectionSample sample = collector.calculateSample(rawSample.direction, rawSample.position, rawSample.label);
                wallProjectionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML(filePath);
            foreach (WallProjectionSample wps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionSampleToXML(wps, filePath);
            }
        }

        public void calculateAndWriteWallProjectionAndPositionSamples(List<SampleExtractor.rawSample> rawSampleList, String filePath, SampleCollector collector)
        {
            List<WallProjectionAndPositionSample> wallProjectionSamples = new List<WallProjectionAndPositionSample>();
            WallProjectionSample tmpSample = new WallProjectionSample();
            foreach (SampleExtractor.rawSample rawSample in rawSampleList)
            {
                tmpSample = collector.calculateSample(rawSample.direction, rawSample.position, rawSample.label);

                WallProjectionAndPositionSample sample = new WallProjectionAndPositionSample(tmpSample, new Point3D(rawSample.position.X, rawSample.position.Y, rawSample.position.Z), rawSample.label);
                wallProjectionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML(filePath);
            foreach (WallProjectionAndPositionSample wpps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(wpps, filePath);
            }
        }
    }
}
