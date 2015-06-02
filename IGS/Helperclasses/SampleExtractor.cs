using IGS.Kinect;
using IGS.KNN;
using IGS.Server.IGS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            public Vector3D[] joints;
        }
        List<Vector3D[]> arr { get; set; }
        public List<rawSample> rawSamplesPerSelectSmoothed { get; set; }
        public List<rawSample> rawSamplesPerSelect { get; set; }
 
        SkeletonJointFilter jointFilter { get; set; }

        public SampleExtractor(CoordTransform transform)
        {
            rawSamplesPerSelectSmoothed = new List<rawSample>();
            jointFilter = new MedianJointFilter();
            rawSamplesPerSelect = new List<rawSample>();
            arr = new List<Vector3D[]>();

            readSkeletonsPerSelectFromXMLAndCreateRawSamples(transform);

            readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(transform);
       } 


        public void writeNormalSamplesFromRawSamples(String DirectoryPath, List<rawSample> sampleList, String Filename)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + DirectoryPath))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + DirectoryPath);
            }
            XMLComponentHandler.testAndCreateSampleXML("NormalSamples" + "\\" + DirectoryPath + "\\" + Filename);

            foreach (rawSample rs in sampleList)
            {
                Vector3D direction = Vector3D.Subtract(rs.joints[3], rs.joints[2]);
                XMLComponentHandler.writeSampleToXML(rs.joints[2], direction, rs.label, "NormalSamples" + "\\" + DirectoryPath + "\\" + Filename);
            }
        }


        public List<WallProjectionSample> calculateAndWriteWallProjectionSamples(SampleCollector collector, String DirectoryPath, List<rawSample> sampleList, String Filename)
        {
            List<WallProjectionSample> wallProjectionSamples = new List<WallProjectionSample>();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                WallProjectionSample sample = collector.calculateWallProjectionSample(rawSample.joints, rawSample.label);
                if (!sample.sampleDeviceName.Equals("nullSample"))
                {
                    wallProjectionSamples.Add(sample);
                }
            }

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + DirectoryPath))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + DirectoryPath);
            }
            XMLComponentHandler.testAndCreateSampleXML("WallProjectionSamples" + "\\" + DirectoryPath + "\\" + Filename);
            foreach (WallProjectionSample wps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionSampleToXML(wps, "WallProjectionSamples" + "\\" + DirectoryPath + "\\" + Filename);
            }

            return wallProjectionSamples;
        }

        public List<WallProjectionSample> calculateWallProjectionSamples(SampleCollector collector, List<rawSample> sampleList)
        {
            List<WallProjectionSample> wpsList = new List<WallProjectionSample>();

            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                WallProjectionSample sample = collector.calculateWallProjectionSample(rawSample.joints, rawSample.label);
                if (!sample.sampleDeviceName.Equals("nullSample"))
                {
                   wpsList.Add(sample);
                }
            }
            return wpsList;

        }

        public List<WallProjectionAndPositionSample> calculateAndWriteWallProjectionAndPositionSamples(SampleCollector collector, String DirectoryPath, List<rawSample> sampleList, String Filename)
        {
            List<WallProjectionAndPositionSample> wallProjectionSamplesAndPositionSamples = new List<WallProjectionAndPositionSample>();
            WallProjectionSample tmpSample = new WallProjectionSample();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                tmpSample = collector.calculateWallProjectionSample(rawSample.joints, rawSample.label);

                WallProjectionAndPositionSample sample = new WallProjectionAndPositionSample(tmpSample, new Point3D(rawSample.joints[2].X, rawSample.joints[2].Y, rawSample.joints[2].Z), rawSample.label);

                if (!sample.sampleDeviceName.Equals("NullSample"))
                {
                    wallProjectionSamplesAndPositionSamples.Add(sample);
                }
                
            }

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples" + "\\" + DirectoryPath))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples"+"\\" + DirectoryPath);
            }
            XMLComponentHandler.testAndCreateSampleXML("WallProjectionAndPositionSamples"+"\\" + DirectoryPath + "\\" + Filename);
            foreach (WallProjectionAndPositionSample wpps in wallProjectionSamplesAndPositionSamples)
            {
                XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(wpps, "WallProjectionAndPositionSamples" + "\\" + DirectoryPath + "\\" + Filename);
            }

            return wallProjectionSamplesAndPositionSamples;
        }


        public void readSkeletonsPerSelectFromXMLAndCreateRawSamples(CoordTransform transformer)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml";

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNodeList selects = rootNode.ChildNodes;

            bool foundWristRight = false;
            bool foundShoulderRight = false;
            bool foundWristLeft = false;
            bool foundShoulderLeft = false;


            foreach (XmlNode select in selects)
            {
                String deviceName = select.Attributes[3].Value;

                Vector3D WristRight = new Vector3D();
                Vector3D ShoulderRight = new Vector3D();
                Vector3D WristLeft = new Vector3D();
                Vector3D ShoulderLeft = new Vector3D();
                Vector3D[] smoothed = new Vector3D[4];

                foreach (XmlNode joint in select.FirstChild)
                {
                    if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristRight"))
                    {
                        WristRight.X = Double.Parse(joint.Attributes[1].Value);
                        WristRight.Y = Double.Parse(joint.Attributes[2].Value);
                        WristRight.Z = Double.Parse(joint.Attributes[3].Value);
                        foundWristRight = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderRight"))
                    {
                        ShoulderRight.X = Double.Parse(joint.Attributes[1].Value);
                        ShoulderRight.Y = Double.Parse(joint.Attributes[2].Value);
                        ShoulderRight.Z = Double.Parse(joint.Attributes[3].Value);
                        foundShoulderRight = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristLeft"))
                    {
                        WristLeft.X = Double.Parse(joint.Attributes[1].Value);
                        WristLeft.Y = Double.Parse(joint.Attributes[2].Value);
                        WristLeft.Z = Double.Parse(joint.Attributes[3].Value);
                        foundWristLeft = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderLeft"))
                    {
                        ShoulderLeft.X = Double.Parse(joint.Attributes[1].Value);
                        ShoulderLeft.Y = Double.Parse(joint.Attributes[2].Value);
                        ShoulderLeft.Z = Double.Parse(joint.Attributes[3].Value);
                        foundShoulderLeft = true;
                    }
                    if (foundWristRight == true && foundShoulderRight == true && foundWristLeft == true && foundShoulderLeft)
                    {
                        
                        foundWristRight = false;
                        foundShoulderRight = false;
                        foundWristLeft = false;
                        foundShoulderLeft = false;

                        Vector3D[] tmpVecs = new Vector3D[] {
                                ShoulderLeft, WristLeft, ShoulderRight, WristRight
                            };
                        Vector3D[] vecs = transformer.transformJointCoords(tmpVecs);
                        
                        
                        rawSample sample = new rawSample();
                        sample.joints = vecs;
                        sample.label = deviceName;
                        rawSamplesPerSelect.Add(sample);
                        break;
                    }
                }
            }
        }



        public void readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(CoordTransform transformer)
        {
            List<Vector3D[]> selectVectorsToSmooth = new List<Vector3D[]>();
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml";

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNodeList selects = rootNode.ChildNodes;

            int i = 0;
            
            bool foundWristRight = false;
            bool foundShoulderRight = false;
            bool foundWristLeft = false;
            bool foundShoulderLeft = false;

            foreach (XmlNode select in selects)
            {
                i++;
                Vector3D[] smoothed = new Vector3D[4];
                String deviceName = select.Attributes[3].Value;
                
                foreach (XmlNode skeleton in select.ChildNodes)
                {
                    
                    Vector3D WristRight = new Vector3D();
                    Vector3D ShoulderRight = new Vector3D();
                    Vector3D WristLeft = new Vector3D();
                    Vector3D ShoulderLeft = new Vector3D();
                    

                    foreach (XmlNode joint in skeleton.ChildNodes)
                    {
                        if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristRight"))
                        {
                            WristRight.X = Double.Parse(joint.Attributes[1].Value);
                            WristRight.Y = Double.Parse(joint.Attributes[2].Value);
                            WristRight.Z = Double.Parse(joint.Attributes[3].Value);
                            foundWristRight = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderRight"))
                        {
                            ShoulderRight.X = Double.Parse(joint.Attributes[1].Value);
                            ShoulderRight.Y = Double.Parse(joint.Attributes[2].Value);
                            ShoulderRight.Z = Double.Parse(joint.Attributes[3].Value);
                            foundShoulderRight = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristLeft"))
                        {
                            WristLeft.X = Double.Parse(joint.Attributes[1].Value);
                            WristLeft.Y = Double.Parse(joint.Attributes[2].Value);
                            WristLeft.Z = Double.Parse(joint.Attributes[3].Value);
                            foundWristLeft = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderLeft"))
                        {
                            ShoulderLeft.X = Double.Parse(joint.Attributes[1].Value);
                            ShoulderLeft.Y = Double.Parse(joint.Attributes[2].Value);
                            ShoulderLeft.Z = Double.Parse(joint.Attributes[3].Value);
                            foundShoulderLeft = true;
                        }
                        if (foundWristRight == true && foundShoulderRight == true && foundWristLeft == true && foundShoulderLeft)
                        {

                            
                            foundWristRight = false;
                            foundShoulderRight = false;
                            foundWristLeft = false;
                            foundShoulderLeft = false;

                            Vector3D[] tmpVecs = new Vector3D[] {
                                ShoulderLeft, WristLeft, ShoulderRight, WristRight
                            };
                           
                            selectVectorsToSmooth.Add(tmpVecs);

                            break;
                        }
                    }
                }

                smoothed = jointFilter.jointFilter(selectVectorsToSmooth);
                selectVectorsToSmooth.Clear();
                smoothed = transformer.transformJointCoords(smoothed);
                rawSample sample = new rawSample();
                sample.joints = smoothed;
                sample.label = deviceName;

                rawSamplesPerSelectSmoothed.Add(sample);
            }
        }

        public List<List<SampleExtractor.rawSample>> rawSampleOnlinePartsMerger(List<List<rawSample>> onlineParts)
        {
            List<List< rawSample>> mergedParts = new List<List<rawSample>>();

            for (int k = 0; k < onlineParts.Count; k++ )
            {
                List<rawSample> newList = new List<rawSample>();
                mergedParts.Add(newList);
            }

            for (int i = 0; i < onlineParts.Count; i++)
            {
                for (int j = i; j < onlineParts.Count; j++)
                {
                    foreach (rawSample rs in onlineParts[i])
                    {
                        mergedParts[j].Add(rs);
                    }
                }
            }

            return mergedParts;
        }

        public void readSkeletonsPerSelectFromXMLAndCreateRawSamples(CoordTransform transformer, String targetName, String targetFolder)
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\" + targetFolder + "\\" + targetName + ".xml";

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNodeList selects = rootNode.ChildNodes;

            bool foundWristRight = false;
            bool foundShoulderRight = false;
            bool foundWristLeft = false;
            bool foundShoulderLeft = false;


            foreach (XmlNode select in selects)
            {
                String deviceName = select.Attributes[3].Value;

                Vector3D WristRight = new Vector3D();
                Vector3D ShoulderRight = new Vector3D();
                Vector3D WristLeft = new Vector3D();
                Vector3D ShoulderLeft = new Vector3D();
                Vector3D[] smoothed = new Vector3D[4];

                foreach (XmlNode joint in select.FirstChild)
                {
                    if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristRight"))
                    {
                        WristRight.X = Double.Parse(joint.Attributes[1].Value);
                        WristRight.Y = Double.Parse(joint.Attributes[2].Value);
                        WristRight.Z = Double.Parse(joint.Attributes[3].Value);
                        foundWristRight = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderRight"))
                    {
                        ShoulderRight.X = Double.Parse(joint.Attributes[1].Value);
                        ShoulderRight.Y = Double.Parse(joint.Attributes[2].Value);
                        ShoulderRight.Z = Double.Parse(joint.Attributes[3].Value);
                        foundShoulderRight = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristLeft"))
                    {
                        WristLeft.X = Double.Parse(joint.Attributes[1].Value);
                        WristLeft.Y = Double.Parse(joint.Attributes[2].Value);
                        WristLeft.Z = Double.Parse(joint.Attributes[3].Value);
                        foundWristLeft = true;
                    }
                    else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderLeft"))
                    {
                        ShoulderLeft.X = Double.Parse(joint.Attributes[1].Value);
                        ShoulderLeft.Y = Double.Parse(joint.Attributes[2].Value);
                        ShoulderLeft.Z = Double.Parse(joint.Attributes[3].Value);
                        foundShoulderLeft = true;
                    }
                    if (foundWristRight == true && foundShoulderRight == true && foundWristLeft == true && foundShoulderLeft)
                    {

                        foundWristRight = false;
                        foundShoulderRight = false;
                        foundWristLeft = false;
                        foundShoulderLeft = false;

                        Vector3D[] tmpVecs = new Vector3D[] {
                                ShoulderLeft, WristLeft, ShoulderRight, WristRight
                            };
                        Vector3D[] vecs = transformer.transformJointCoords(tmpVecs);


                        rawSample sample = new rawSample();
                        sample.joints = vecs;
                        sample.label = deviceName;
                        rawSamplesPerSelect.Add(sample);
                        break;
                    }
                }
            }
        }



        public void readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(CoordTransform transformer, String targetName, String targetFolder)
        {
            List<Vector3D[]> selectVectorsToSmooth = new List<Vector3D[]>();
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\" + targetFolder + "\\" + targetName + ".xml";

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNodeList selects = rootNode.ChildNodes;

            int i = 0;

            bool foundWristRight = false;
            bool foundShoulderRight = false;
            bool foundWristLeft = false;
            bool foundShoulderLeft = false;

            foreach (XmlNode select in selects)
            {
                i++;
                Vector3D[] smoothed = new Vector3D[4];
                String deviceName = select.Attributes[3].Value;

                foreach (XmlNode skeleton in select.ChildNodes)
                {

                    Vector3D WristRight = new Vector3D();
                    Vector3D ShoulderRight = new Vector3D();
                    Vector3D WristLeft = new Vector3D();
                    Vector3D ShoulderLeft = new Vector3D();


                    foreach (XmlNode joint in skeleton.ChildNodes)
                    {
                        if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristRight"))
                        {
                            WristRight.X = Double.Parse(joint.Attributes[1].Value);
                            WristRight.Y = Double.Parse(joint.Attributes[2].Value);
                            WristRight.Z = Double.Parse(joint.Attributes[3].Value);
                            foundWristRight = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderRight"))
                        {
                            ShoulderRight.X = Double.Parse(joint.Attributes[1].Value);
                            ShoulderRight.Y = Double.Parse(joint.Attributes[2].Value);
                            ShoulderRight.Z = Double.Parse(joint.Attributes[3].Value);
                            foundShoulderRight = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("WristLeft"))
                        {
                            WristLeft.X = Double.Parse(joint.Attributes[1].Value);
                            WristLeft.Y = Double.Parse(joint.Attributes[2].Value);
                            WristLeft.Z = Double.Parse(joint.Attributes[3].Value);
                            foundWristLeft = true;
                        }
                        else if (joint.Attributes[0].Name.ToString().Equals("type") && joint.Attributes[0].Value.ToString().Equals("ShoulderLeft"))
                        {
                            ShoulderLeft.X = Double.Parse(joint.Attributes[1].Value);
                            ShoulderLeft.Y = Double.Parse(joint.Attributes[2].Value);
                            ShoulderLeft.Z = Double.Parse(joint.Attributes[3].Value);
                            foundShoulderLeft = true;
                        }
                        if (foundWristRight == true && foundShoulderRight == true && foundWristLeft == true && foundShoulderLeft)
                        {


                            foundWristRight = false;
                            foundShoulderRight = false;
                            foundWristLeft = false;
                            foundShoulderLeft = false;

                            Vector3D[] tmpVecs = new Vector3D[] {
                                ShoulderLeft, WristLeft, ShoulderRight, WristRight
                            };

                            selectVectorsToSmooth.Add(tmpVecs);

                            break;
                        }
                    }
                }

                smoothed = jointFilter.jointFilter(selectVectorsToSmooth);
                selectVectorsToSmooth.Clear();
                smoothed = transformer.transformJointCoords(smoothed);
                rawSample sample = new rawSample();
                sample.joints = smoothed;
                sample.label = deviceName;

                rawSamplesPerSelectSmoothed.Add(sample);
            }
        }

    }
}
