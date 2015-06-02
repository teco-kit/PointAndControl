using IGS.Helperclasses;
using IGS.KNN;
using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.IGS
{
    class ClassifierMethod : CoreMethods
    {
        public  ClassificationHandler classificationHandler { get; set; }
        public DataHolder data { get; set; }


        public ClassifierMethod(ClassificationHandler handler, DataHolder Data)
        {
            classificationHandler = handler;
            this.data = Data;
        }

        public override void train(List<Vector3D[]> vectorList, Device dev, String value)
        {

            classificationHandler.calculateWallProjectionSampleAndLearn(vectorList, dev.Name);

        }
        
        public override List<Device> chooseDevice(String wlanAdr, CoordTransform Transformer, UserTracker Tracker)
        {
            
            
            List<Device> dev = new List<Device>();
            User tempUser = data.GetUserByIp(wlanAdr);
            Vector3D[] vecs = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(tempUser.SkeletonId));
     
            //Vector3D[] vecs = Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId));
            if (tempUser != null)
            {
                
                WallProjectionSample sample = classificationHandler.collector.calculateWallProjectionSample(vecs, "");


               
                sample = classificationHandler.classify(sample);

                Console.WriteLine("Classified: " + sample.sampleDeviceName);
                XMLComponentHandler.writeLogEntry("Device classified to" + sample.sampleDeviceName);
                //Body body = Tracker.GetBodyById(tempUser.SkeletonId);
                //XMLSkeletonJointRecords.writeUserJointsToXmlFile(tempUser, Data.GetDeviceByName(sample.sampleDeviceName), body);
                //XMLComponentHandler.writeUserJointsPerSelectClick(body);
                classificationHandler.deviceClassificationCount++;

                Device device = data.GetDeviceByName(sample.sampleDeviceName);
                sample.sampleDeviceName = device.Name;



                if (sample != null)
                {
                    foreach (Device d in data.Devices)
                    {
                        if (d.Name.ToLower() == sample.sampleDeviceName.ToLower())
                        {
                            XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                            Point3D p = new Point3D(vecs[2].X, vecs[2].Y, vecs[2].Z);
                            XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                            XMLComponentHandler.writeSampleToXML(vecs, sample.sampleDeviceName);
                            XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(d);
                            dev.Add(d);
                            //tempUser.lastChosenDeviceID = d.Id;
                            //tempUser.lastClassDevSample = sample;
                            //tempUser.deviceIDChecked = false;
                            return dev;
                        }
                    }
                }
            }
            return dev;
        }

    }
}
