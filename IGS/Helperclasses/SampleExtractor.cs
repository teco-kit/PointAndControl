using IGS.Kinect;
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
            public Vector3D positionShoulder;
            public Vector3D positionWrist;
            public Vector3D direction;
        }

        public List<rawSample> rawSamplesPerSelectSmoothed { get; set; }
        public List<rawSample> rawSamplesPerSelect { get; set; }

        SkeletonJointFilter jointFilter { get; set; }

        public SampleExtractor()
        {
            rawSamplesPerSelectSmoothed = new List<rawSample>();
            jointFilter = new MedianJointFilter();
            rawSamplesPerSelect = new List<rawSample>();
            readSkeletonsPerSelectFromXMLAndCreateRawSamples();
            readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples();
           
        }


        public void writeNormalSamplesFromRawSamples(String filePath, List<rawSample> sampleList)
        {
            XMLComponentHandler.testAndCreateSampleXML(filePath);

            foreach (rawSample rs in sampleList)
            {
                XMLComponentHandler.writeSampleToXML(rs.positionShoulder, rs.direction, rs.label, filePath);
            }
        }


        public List<WallProjectionSample> calculateAndWriteWallProjectionSamples(SampleCollector collector, String filePath, List<rawSample> sampleList)
        {
            List<WallProjectionSample> wallProjectionSamples = new List<WallProjectionSample>();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                WallProjectionSample sample = collector.calculateWallProjectionSample(rawSample.direction, rawSample.positionShoulder, rawSample.label);
                wallProjectionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML(filePath);
            foreach (WallProjectionSample wps in wallProjectionSamples)
            {
                XMLComponentHandler.writeWallProjectionSampleToXML(wps, filePath);
            }

            return wallProjectionSamples;
        }

        public List<WallProjectionAndPositionSample> calculateAndWriteWallProjectionAndPositionSamples(SampleCollector collector, String filePath, List<rawSample> sampleList)
        {
            List<WallProjectionAndPositionSample> wallProjectionSamplesAndPositionSamples = new List<WallProjectionAndPositionSample>();
            WallProjectionSample tmpSample = new WallProjectionSample();
            foreach (SampleExtractor.rawSample rawSample in sampleList)
            {
                tmpSample = collector.calculateWallProjectionSample(rawSample.direction, rawSample.positionShoulder, rawSample.label);

                WallProjectionAndPositionSample sample = new WallProjectionAndPositionSample(tmpSample, new Point3D(rawSample.positionShoulder.X, rawSample.positionShoulder.Y, rawSample.positionShoulder.Z), rawSample.label);

                wallProjectionSamplesAndPositionSamples.Add(sample);
            }
            XMLComponentHandler.testAndCreateSampleXML(filePath);
            foreach (WallProjectionAndPositionSample wpps in wallProjectionSamplesAndPositionSamples)
            {
                XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(wpps, filePath);
            }

            return wallProjectionSamplesAndPositionSamples;
        }


        public void readSkeletonsPerSelectFromXMLAndCreateRawSamples()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml";

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(path);
            XmlNode rootNode = docConfig.SelectSingleNode("/data");

            XmlNodeList selects = rootNode.ChildNodes;

           

            bool foundDirectionPoint = false;
            bool foundUpPoint = false;


            foreach (XmlNode select in selects)
            {
                String deviceName = select.Attributes[3].Value;
                Vector3D upPoint = new Vector3D();
                Vector3D directionPoint = new Vector3D();
                Vector3D direction = new Vector3D();
                foreach (XmlNode joint in select.FirstChild)
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
                                    upPoint.X = Double.Parse(joint.Attributes[1].Value);
                                    upPoint.Y = Double.Parse(joint.Attributes[2].Value);
                                    upPoint.Z = Double.Parse(joint.Attributes[3].Value);
                                    foundUpPoint = true;
                                }
                    if (foundDirectionPoint == true && foundUpPoint == true)
                    {
                        foundDirectionPoint = false;
                        foundUpPoint = false;
                        direction.X = directionPoint.X - upPoint.X;
                        direction.Y = directionPoint.Y - upPoint.Y;
                        direction.Z = directionPoint.Z - upPoint.Z;


                        rawSample sample = new rawSample();
                        sample.label = deviceName;
                        sample.direction = direction;
                        sample.positionShoulder = upPoint;
                        sample.positionWrist = directionPoint;
                        rawSamplesPerSelect.Add(sample);
                        break;
                    }
                }
            }
        }



        public void readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples()
        {
            List<Vector3D[]> selectVectorsToSmooth = new List<Vector3D[]>();
            String path = AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml";

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
                Vector3D WristRight = new Vector3D();
                Vector3D ShoulderRight = new Vector3D();
                Vector3D WristLeft = new Vector3D();
                Vector3D ShoulderLeft = new Vector3D();
                Vector3D[] smoothed = new Vector3D[4];
                Vector3D position = new Vector3D();
                Vector3D directionPoint = new Vector3D();
                Vector3D direction = new Vector3D();
           
                String deviceName = select.Attributes[3].Value;
                foreach (XmlNode skeleton in select.ChildNodes)
                {
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

                            Vector3D[] vecs = new Vector3D[] {
                                ShoulderLeft, WristLeft, ShoulderRight, WristRight
                            };

                            selectVectorsToSmooth.Add(vecs);

                            break;
                        }
                    }
                }

                smoothed = jointFilter.jointFilter(selectVectorsToSmooth);

                position = smoothed[2];
                directionPoint = smoothed[3];

                direction.X = directionPoint.X - position.X;
                direction.Y = directionPoint.Y - position.Y;
                direction.Z = directionPoint.Z - position.Z;

                rawSample sample = new rawSample();
                sample.direction = direction;
                sample.positionShoulder = position;
                sample.positionWrist = directionPoint;
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

        

    }
}
