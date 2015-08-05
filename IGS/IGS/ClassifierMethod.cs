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
using Microsoft.Kinect;

namespace IGS.Server.IGS
{
    class ClassifierMethod : ICoreMethods
    {
        public ClassificationHandler classificationHandler { get; set; }
        public DataHolder data { get; set; }

        public UserTracker tracker { get; set; }
        public CoordTransform transformer { get; set; }


        public ClassifierMethod(ClassificationHandler handler, UserTracker tracker, DataHolder data, CoordTransform transform)
        {
            this.classificationHandler = handler;
            this.data = data;
            this.tracker = tracker;
            this.transformer = transform;
        }

        public String train(Device dev)
        {
            String s = "";

            //TODO: validate that this function still works as intended

            //Vector3D[] vecs = transformer.transformJointCoords(tracker.getMedianFilteredCoordinates(usr.SkeletonId));
            foreach (Vector3D[] vecs in dev.PositionVectors)
            {
                s += classificationHandler.calculateWallProjectionSampleAndLearn(vecs, dev.Id);
                XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(dev);
            }

            return s;

        }

        public List<Device> chooseDevice(User usr)
        {
            List<Device> dev = new List<Device>();
            Vector3D[] vecs = transformer.transformJointCoords(tracker.getMedianFilteredCoordinates(usr.SkeletonId));
            if (usr != null)
            {

                WallProjectionSample sample = classificationHandler.sCalculator.calculateSample(vecs, "");

                sample = classificationHandler.classify(sample);

                Console.WriteLine("Classified: " + sample.sampledeviceIdentifier);
                XMLComponentHandler.writeLogEntry("Device classified to" + sample.sampledeviceIdentifier);
                Body body = tracker.GetBodyById(usr.SkeletonId);
                //XMLSkeletonJointRecords.writeUserJointsToXmlFile(tempUser, Data.GetDeviceByName(sample.sampledeviceIdentifier), body);
                //XMLComponentHandler.writeUserJointsPerSelectClick(body);

                String decapsulate = "";
                Device device = null;

                foreach (Device d in data.Devices)
                {
                    decapsulate = d.Id.Replace("_", "");
                    if (sample.sampledeviceIdentifier.ToLower() == decapsulate.ToLower())
                    {
                        device = d;
                    }
                }
                sample.sampledeviceIdentifier = device.Id;



                if (sample != null)
                {
                    foreach (Device d in data.Devices)
                    {
                        if (d.Id.ToLower() == sample.sampledeviceIdentifier.ToLower())
                        {

                            Point3D p = new Point3D(vecs[2].X, vecs[2].Y, vecs[2].Z);

                            dev.Add(d);

                            return dev;
                        }
                    }
                }
            }
            return dev;
        }

        public int getMinVectorsPerDevice()
        {
            return 10;
        }

    }
}
