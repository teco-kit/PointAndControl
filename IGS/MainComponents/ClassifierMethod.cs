﻿using PointAndControl.Classifier;
using PointAndControl.ComponentHandling;
using PointAndControl.Devices;
using PointAndControl.Helperclasses;
using PointAndControl.Kinect;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace PointAndControl.MainComponents
{
    class ClassifierMethod : ICoreMethods
    {
        public ClassificationHandler classificationHandler { get; set; }
        public DataHolder data { get; set; }

        public UserTracker tracker { get; set; }
        public CoordTransform transformer { get; set; }

        private EventLogger logger { get; set; }

        public ClassifierMethod(ClassificationHandler handler, UserTracker tracker, DataHolder data, CoordTransform transform, EventLogger eLogger)
        {
            this.classificationHandler = new ClassificationHandler(transform, data);
            this.data = data;
            this.tracker = tracker;
            this.transformer = transform;
            logger = eLogger;
        }

        public String train(Device dev)
        {
            String s = "";

            foreach (Point3D[] vecs in dev.skelPositions)
            {
                s += classificationHandler.calculateWallProjectionSampleAndLearn(vecs, dev.Id);
            }

            return String.Format(Properties.Resources.AdditionalDevCoords, dev.Name);

        }

        public List<Device> chooseDevice(User usr)
        {
            List<Device> dev = new List<Device>();
            Point3D[] vecs = transformer.transformJointCoords(tracker.getMedianFilteredCoordinates(usr.SkeletonId));

            if (usr != null)
            {

                WallProjectionSample sample = classificationHandler.sCalculator.calculateSample(vecs, "");

                // execute the classification
                sample = classificationHandler.classify(sample);

                Console.WriteLine("Classified: " + sample.sampledeviceIdentifier);
                logger.enqueueEntry("Device classified to" + sample.sampledeviceIdentifier);
                //Body body = tracker.GetBodyById(usr.SkeletonId);
                //XMLSkeletonJointRecords.writeUserJointsToXmlFile(tempUser, Data.GetDeviceByName(sample.sampledeviceIdentifier), body);
                //XMLComponentHandler.writeUserJointsPerSelectClick(body);


                
                //ANSWER: NUML Returns the Label of the Classification in all uppercases and without extra like "_" (Partitioner of DeviceType_Number) - Decapsulate needed for Comparison of DeviceID and Returned Label    
                String decapsulate = "";

                foreach (Device d in data.getCompleteDeviceList())
                {
                    decapsulate = decapsulateID(d.Id);

                    if (sample.sampledeviceIdentifier.ToLower() == decapsulate.ToLower())
                    {
                        dev.Add(d);
                        break;
                    }
                }
            }

            return dev;
        }

        public int getMinVectorsPerDevice()
        {
            return 10;
        }

        private String decapsulateID(String id)
        {
            return id.Replace("_", "");
        }

    }
}
