using IGS.Helperclasses;
using IGS.Classifier;
using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.Server.IGS
{
    class ClassifierMethod : ICoreMethods
    {
        public  ClassificationHandler classificationHandler { get; set; }
        public DataHolder data { get; set; }

        public UserTracker tracker { get; set; }
        public CoordTransform transformer { get; set; }


        public ClassifierMethod(ClassificationHandler handler, UserTracker tracker, DataHolder data, CoordTransform transform)
        {
            this.classificationHandler = handler;
            this.data = data;
            this.tracker = tracker;
            this.transformer = transformer;
        }

        public String train( String wlanAdr ,String devID)
        {

            Device dev = data.GetDeviceByName(devID);
            User tmp = data.GetUserByIp(wlanAdr);
            Vector3D[] vecs = transformer.transformJointCoords(tracker.getMedianFilteredCoordinates(tmp.SkeletonId));
            String s = classificationHandler.calculateWallProjectionSampleAndLearn(vecs, dev.Id);
            XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(dev);

            return s;

        }
        
        public List<Device> chooseDevice(String wlanAdr)
        {
            
            
            List<Device> dev = new List<Device>();
            User tempUser = data.GetUserByIp(wlanAdr);
            Vector3D[] vecs = transformer.transformJointCoords(tracker.getMedianFilteredCoordinates(tempUser.SkeletonId));
     
            //Vector3D[] vecs = Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId));
            if (tempUser != null)
            {
                
                WallProjectionSample sample = classificationHandler.sCalculator.calculateSample(vecs, "");


               
                sample = classificationHandler.classify(sample);

                Console.WriteLine("Classified: " + sample.sampledeviceIdentifier);
                XMLComponentHandler.writeLogEntry("Device classified to" + sample.sampledeviceIdentifier);
                //Body body = Tracker.GetBodyById(tempUser.SkeletonId);
                //XMLSkeletonJointRecords.writeUserJointsToXmlFile(tempUser, Data.GetDeviceByName(sample.sampledeviceIdentifier), body);
                //XMLComponentHandler.writeUserJointsPerSelectClick(body);
               
                Device device = data.GetDeviceByName(sample.sampledeviceIdentifier);
                sample.sampledeviceIdentifier = device.Name;



                if (sample != null)
                {
                    foreach (Device d in data.Devices)
                    {
                        if (d.Name.ToLower() == sample.sampledeviceIdentifier.ToLower())
                        {
                            XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                            Point3D p = new Point3D(vecs[2].X, vecs[2].Y, vecs[2].Z);
                            XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                            XMLComponentHandler.writeSampleToXML(vecs, sample.sampledeviceIdentifier);
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
