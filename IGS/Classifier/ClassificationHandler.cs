using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using IGS.Server.IGS;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace IGS.Classifier
{
    public class ClassificationHandler
    {

        public KNNClassifier knnClassifier { get; set; }
        
        public SampleCalculator sCalculator { get; set; }

        public SampleExtractor extractor { get; set; }
        public SampleSplitter splitter { get; set; }

        public ClassificationHandler(CoordTransform transformer)
        {
            knnClassifier = new KNNClassifier(XMLComponentHandler.readWallProjectionSamplesFromXML(), -1);
            extractor = new SampleExtractor(transformer);
        
            splitter = new SampleSplitter();
            sCalculator = new SampleCalculator(knnClassifier);
            Console.WriteLine("After collector config");
        }



        //public void onlineLearn(User u)
        //{
            
        //        knnClassifier.learnOnline(u.lastClassDevSample);

                
        //        u.deviceIDChecked = true;
               
        //        u.lastClassDevSample = null;
        //        u.lastChosenDeviceID = "";
        //}

       
        public WallProjectionSample classify(WallProjectionSample sample)
        {
            WallProjectionSample predictedSample = knnClassifier.classify(sample);

            return predictedSample;
        }

        public List<WallProjectionSample> getSamples()
        {
            return knnClassifier.samples;
        }

        public void retrainClassifier(List<WallProjectionSample> samples)
        {
            knnClassifier.learnBatch(samples);
        }

        public void retrainClassifier()
        {
            knnClassifier.trainClassifier();
        }


        //public void calculateWallDeviceAreas(DataHolder data)
        //{
        //    collector.calcRoomModel.calculateDeviceAreas(knnClassifier, data);
        //}

        public void doCrossVal(DataHolder data, CoordTransform transformer)
        {
            Crossvalidator crossval = new Crossvalidator(this, knnClassifier,data, transformer);
            crossval.crossValidateClassifier();

            //Thread.Sleep(1000);
            
            //crossval.crossValidateCollision();

            //XMLComponentHandler.writeTimesForCrossvalidation(crossval.timeForPreprocessingCollision, crossval.timeForTrainingCollsion, crossval.timeForClassifikationCollision,
            //                                               crossval.timeForPreprocessingClassification, crossval.timeForTrainingClassification, crossval.timeForClassifikationClassification);
            //crossval.crossValidateCollisionHopp();
            //timeTaking();
        }
        /// <summary>
        /// Calculates a WallProjection Sample and adds it to the Pending samples to learn.
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public bool calculateWallProjectionSampleAndLearn(List<Vector3D[]> vectorsList, String deviceName)
        {
            
            bool success = false;
           
            List<WallProjectionSample> sampleList = new List<WallProjectionSample>();
            foreach (Vector3D[] vectors in vectorsList)
            {
                WallProjectionSample sample = sCalculator.calculateWallProjectionSample(vectors, deviceName);

                if (sample.sampleDeviceName.Equals("nullSample") == false)
                {

                    XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                    Point3D p = new Point3D(vectors[2].X, vectors[2].Y, vectors[2].Z);
                    XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                    XMLComponentHandler.writeSampleToXML(vectors, sample.sampleDeviceName);

                    sampleList.Add(sample);

                    success = true;

                }

            }

            knnClassifier.learnBatch(sampleList);

            return success;
        }

        public bool calculateWallProjectionSampleAndLearn(Vector3D[] vectors, String deviceName)
        {

            bool success = false;

            
            
                WallProjectionSample sample = sCalculator.calculateWallProjectionSample(vectors, deviceName);

                if (sample.sampleDeviceName.Equals("nullSample") == false)
                {

                    XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                    Point3D p = new Point3D(vectors[2].X, vectors[2].Y, vectors[2].Z);
                    XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                    XMLComponentHandler.writeSampleToXML(vectors, sample.sampleDeviceName);

                    knnClassifier.samples.Add(sample);

                    success = true;

                }

            


            return success;
        }


        public void extractAndCreateLists(CoordTransform transform)
        {
           
            createTestLists(transform);
            createTestListsOAAOne(transform, 4);
            createTestListsOAARest(transform, 4);
            createTestSamplesOnePerDevice(transform);
        }
        public void createTestListsOAARest(CoordTransform trans, int persons)
        {



            String basePath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelect)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error");
                }
            }
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelectSmoothed)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error Smootehed");
                }
            }

            int j = 0;
            bool noMore = false;

            while (noMore == false)
            {

                if (!Directory.Exists(basePath + "\\" + j))
                {
                    noMore = true;
                    break;
                }

                    extractor.rawSamplesPerSelect.Clear();
                    extractor.rawSamplesPerSelectSmoothed.Clear();

                    extractor.readSkeletonsPerSelectFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectPartedOneRest", j.ToString());
                    extractor.readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectSmoothedPartedRest", j.ToString());




                    extractor.calculateAndWriteWallProjectionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.calculateAndWriteWallProjectionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    extractor.writeNormalSamplesFromRawSamples("\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.writeNormalSamplesFromRawSamples("\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    j++;
                }
            
        }

        public void createTestListsOAAOne(CoordTransform trans, int persons)
        {

            

            

            String basePath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelect)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error");
                }
            }
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelectSmoothed)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error Smootehed");
                }
            }

            int j = 0;
            bool noMore = false;
            while (noMore == false) {


                if (!Directory.Exists(basePath + "\\" + j))
                {
                    noMore = true;
                    break;
                }

                extractor.rawSamplesPerSelect.Clear();
                extractor.rawSamplesPerSelectSmoothed.Clear();

                extractor.readSkeletonsPerSelectFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectPartedOne", j.ToString());
                extractor.readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectSmoothedPartedOne", j.ToString());

                extractor.calculateAndWriteWallProjectionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelect, "AllOne");
                extractor.calculateAndWriteWallProjectionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");

                extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelect, "AllOne");
                extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");

                extractor.writeNormalSamplesFromRawSamples("\\" + j , extractor.rawSamplesPerSelect, "AllOne");
                extractor.writeNormalSamplesFromRawSamples("\\" + j , extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");

                j++;
            }
        }

        public void createTestLists(CoordTransform transform)
        {
          

            String basePath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelect)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error");
                }
            }
            foreach (SampleExtractor.rawSample rs in extractor.rawSamplesPerSelectSmoothed)
            {
                if (rs.label == "")
                {
                    Console.WriteLine("Label Error Smootehed");
                }
            }
           
            List<List<SampleExtractor.rawSample>> splittedFrontToBack = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedFrontToBackSmoothed = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelectSmoothed, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeft = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeftSmoothed = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelectSmoothed, 0.5);

           
           
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", splittedFrontToBack[0],"frontToBackBack");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", splittedFrontToBack[1],"frontToBackFront");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator,"" , splittedFrontToBackSmoothed[0],"frontToBackSmoothedBack");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", splittedFrontToBackSmoothed[1],"frontToBackSmoothedFront");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator,"" , splittedRightToLeft[0],"rightToLeftRight");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", splittedRightToLeft[1],"rightToLeftLeft");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", splittedRightToLeftSmoothed[0],"rightToLeftSmoothedRight");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator,"" , splittedRightToLeftSmoothed[1],"rightToLeftSmoothedLeft");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator,"" , extractor.rawSamplesPerSelect,"All");
            extractor.calculateAndWriteWallProjectionSamples(sCalculator, "", extractor.rawSamplesPerSelectSmoothed,"AllSmoothed");

         

            
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedFrontToBack[0],"frontToBackBack");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedFrontToBack[1],"frontToBackFront");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedFrontToBackSmoothed[0],"frontToBackSmoothedBack");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedFrontToBackSmoothed[1],"frontToBackSmoothedFront");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedRightToLeft[0],"rightToLeftRight");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedRightToLeft[1],"rightToLeftLeft");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedRightToLeftSmoothed[0],"rightToLeftSmoothedRight");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", splittedRightToLeftSmoothed[1],"rightToLeftSmoothedLeft");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", extractor.rawSamplesPerSelect,"All");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "", extractor.rawSamplesPerSelectSmoothed,"AllSmoothed");

           
            extractor.writeNormalSamplesFromRawSamples("", splittedFrontToBack[0], "frontToBackBack");
            extractor.writeNormalSamplesFromRawSamples("", splittedFrontToBack[1],"frontToBackFront");
            extractor.writeNormalSamplesFromRawSamples("", splittedFrontToBackSmoothed[0],"frontToBackSmoothedBack");
            extractor.writeNormalSamplesFromRawSamples("", splittedFrontToBackSmoothed[1],"frontToBackSmoothedFront");
            extractor.writeNormalSamplesFromRawSamples("", splittedRightToLeft[0],"rightToLeftRight");
            extractor.writeNormalSamplesFromRawSamples("", splittedRightToLeft[1],"rightToLeftLeft");
            extractor.writeNormalSamplesFromRawSamples("", splittedRightToLeftSmoothed[0],"rightToLeftSmoothedRight");
            extractor.writeNormalSamplesFromRawSamples("", splittedRightToLeftSmoothed[1],"rightToLeftSmoothedLeft");
            extractor.writeNormalSamplesFromRawSamples("", extractor.rawSamplesPerSelect,"All");
            extractor.writeNormalSamplesFromRawSamples("", extractor.rawSamplesPerSelectSmoothed,"AllSmoothed");



        }

        public void createTestSamplesOnePerDevice(CoordTransform trans)
        {
            extractor.rawSamplesPerSelect.Clear();
            extractor.rawSamplesPerSelectSmoothed.Clear();

            extractor.readSkelSelectsToRS(trans);
            extractor.readSkelSelectsSmoothedToRS(trans);


            List<List<SampleExtractor.rawSample>> onlineLearningByDevices = splitter.splitInOnePerDeviceSplits(extractor.rawSamplesPerSelect);
            List<List<SampleExtractor.rawSample>> onlineLearningByDevicesSmoothed = splitter.splitInOnePerDeviceSplits(extractor.rawSamplesPerSelectSmoothed);

            for (int i = 0; i < onlineLearningByDevices.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);
                extractor.calculateAndWriteWallProjectionSamples(sCalculator, "Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);
                extractor.writeNormalSamplesFromRawSamples("Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(sCalculator, "Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
                extractor.calculateAndWriteWallProjectionSamples(sCalculator, "Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
                extractor.writeNormalSamplesFromRawSamples("Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
            }
        }


        public void timeTaking()
        {
            List<double> timeList = new List<double>();
            List<double> trainClassMoreSamples = new List<double>();
            List<double> classificationTimes = new List<double>();

                Stopwatch s = new Stopwatch();
                List<WallProjectionSample> testList = new List<WallProjectionSample>();
                for (int i = 0; i < 50; i++)
                {
                    testList.Add(knnClassifier.samples[i]);
                }
                for (int j = 50; j < knnClassifier.samples.Count(); j++)
                {
                   
                        testList.Add(knnClassifier.samples[j]);

                    s.Start();
                    knnClassifier.trainClassifier(testList);
                    s.Stop();
                    trainClassMoreSamples.Add((double)s.ElapsedMilliseconds);
                    s.Reset();

                    WallProjectionSample wps = new WallProjectionSample(new Point3D(knnClassifier.samples[knnClassifier.samples.Count - 1].x,
                                                                        knnClassifier.samples[knnClassifier.samples.Count - 1].y,
                                                                        knnClassifier.samples[knnClassifier.samples.Count - 1].z)
                        );

                    s.Start();
                    knnClassifier.classify(wps);
                    s.Stop();

                    classificationTimes.Add((double)s.ElapsedMilliseconds);
                    s.Reset();
                }

            


            

            s.Start();
            knnClassifier.trainClassifier(1);
            s.Stop();
            timeList.Add(s.ElapsedMilliseconds);
            

            for (int i = 1; i <= 10; i++)
            {
                s.Restart();
                knnClassifier.trainClassifier(i * 50);
                s.Stop();
                timeList.Add((double)s.ElapsedMilliseconds);
            }

            s.Reset();
            s.Start();
            classify(knnClassifier.samples[0]);
            s.Stop();

            float classtime = s.ElapsedMilliseconds;


            XMLComponentHandler.writeTimeForElapsedTime(timeList, trainClassMoreSamples, classificationTimes, "Classifier");


        }
    }
}
