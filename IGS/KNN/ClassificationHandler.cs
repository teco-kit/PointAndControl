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

namespace IGS.KNN
{
    public class ClassificationHandler
    {

        public KNNClassifier knnClassifier { get; set; }
        public SampleCollector collector { get; set; }

        public SampleExtractor extractor { get; set; }
        public SampleSplitter splitter { get; set; }

        public ulong deviceClassificationCount { get; set; }
        public ulong deviceClassificationErrorCount { get; set; }

        public ClassificationHandler(CoordTransform transformer)
        {
            knnClassifier = new KNNClassifier(XMLComponentHandler.readWallProjectionSamplesFromXML());
            extractor = new SampleExtractor(transformer);
        
            splitter = new SampleSplitter();
            deviceClassificationCount = 0;
            collector = new SampleCollector(knnClassifier);
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
            deviceClassificationCount++;

            return predictedSample;
        }

        public void retrainClassifier(List<WallProjectionSample> samples)
        {
            knnClassifier.learnBatch(samples);
        }

        /// <summary>
        /// Calculates a WallProjection Sample and adds it to the Pending samples to learn.
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public bool calculateWallProjectionSampleAndLearn(Vector3D[] vectors, String deviceName)
        {
            bool success = false;
            WallProjectionSample sample = collector.calculateWallProjectionSample(vectors, deviceName);

            if (sample.sampleDeviceName.Equals("nullSample") == false)
            {
               knnClassifier.pendingSamples.Add(sample);
               XMLComponentHandler.writeWallProjectionSampleToXML(sample);
              
               Point3D p = new Point3D(vectors[2].X, vectors[2].Y, vectors[2].Z);
               XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
           
               XMLComponentHandler.writeSampleToXML(vectors, sample.sampleDeviceName);
             
              
               success = true;

            }
            return success;
        }


        public void extractAndCreateLists(CoordTransform transform)
        {
           
            createTestLists(transform);
            createTestListsOAAOne(transform, 4);
            createTestListsOAARest(transform, 4);

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


            for (int j = 0; j < persons; j++)
            {
                extractor.rawSamplesPerSelect.Clear();
                extractor.rawSamplesPerSelectSmoothed.Clear();
                
                extractor.readSkeletonsPerSelectFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectPartedOneRest", j.ToString());
                extractor.readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectSmoothedPartedRest", j.ToString());
                Console.WriteLine("1: " + extractor.rawSamplesPerSelect.Count);
                Console.WriteLine("2: " + extractor.rawSamplesPerSelect.Count);
                
                    for (int k = 0; k < persons; k++)
                    {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + k);
                    }
               
                    
                    for (int k = 0; k < persons; k++)
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples" + "\\" + k))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples" + "\\" + k);
                        }
                    }
                
                
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples");
                    for (int k = 0; k < persons; k++)
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + k))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + k);
                        }
                    }


                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" +  "AllRest", extractor.rawSamplesPerSelect);
                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "AllRestSmoothed", extractor.rawSamplesPerSelectSmoothed);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "AllRest", extractor.rawSamplesPerSelect);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "AllRestSmoothed", extractor.rawSamplesPerSelectSmoothed);

                extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "AllRest", extractor.rawSamplesPerSelect);
                extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "AllRestSmoothed", extractor.rawSamplesPerSelectSmoothed);

                int d = extractor.rawSamplesPerSelect.Count / 10;
                List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, d);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);

                for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
                {
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" +  "OnlineAllRest" + i, onlineLearningSplitMerged[i]);
                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" +  "OnlineAllRest" + i, onlineLearningSplitMerged[i]);
                    extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" +  "OnlineAllRest" + i, onlineLearningSplitMerged[i]);

                }

                for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
                {
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "OnlineAllRestSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "OnlineAllRestSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                    extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "OnlineAllRestSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                }
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


            for (int j = 0; j < persons; j++)
            {
                extractor.rawSamplesPerSelect.Clear();
                extractor.rawSamplesPerSelectSmoothed.Clear();

                extractor.readSkeletonsPerSelectFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectPartedOne", j.ToString());
                extractor.readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(trans, "BA_REICHE_LogFilePerSelectSmoothedPartedOne", j.ToString());
          


                for (int k = 0; k < persons; k++)
                {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + k))
                    {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + k);
                    }
                }

                    for (int k = 0; k < persons; k++)
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples" + "\\" + k))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples" + "\\" + k);
                        }
                    }
                
                    
                    for (int k = 0; k < persons; k++)
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + k))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + k);
                        }
                    }
                


                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "AllOne", extractor.rawSamplesPerSelect);
                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "AllOneSmoothed", extractor.rawSamplesPerSelectSmoothed);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "AllOne", extractor.rawSamplesPerSelect);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "AllOneSmoothed", extractor.rawSamplesPerSelectSmoothed);

                extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "AllOne", extractor.rawSamplesPerSelect);
                extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "AllOneSmoothed", extractor.rawSamplesPerSelectSmoothed);

                Console.WriteLine("OnlineLearningCalculation");
                int d = extractor.rawSamplesPerSelect.Count / 10;
                List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, d);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
                List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);

                for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
                {
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "OnlineAllOne" + i, onlineLearningSplitMerged[i]);
                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "OnlineAllOne" + i, onlineLearningSplitMerged[i]);
                    extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "OnlineAllOne" + i, onlineLearningSplitMerged[i]);

                }

                for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
                {
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j + "\\" + "OnlineAllOneSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j + "\\" + "OnlineAllOneSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                    extractor.writeNormalSamplesFromRawSamples("\\" + j + "\\" + "OnlineAllOneSmoothed" + i, onlineLearningSplitMergedSmoothed[i]);
                }
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
            Console.WriteLine("1: " + extractor.rawSamplesPerSelect.Count);
            Console.WriteLine("2: " + extractor.rawSamplesPerSelect.Count);
            Console.WriteLine("BatchCalculation");
            List<List<SampleExtractor.rawSample>> splittedFrontToBack = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedFrontToBackSmoothed = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelectSmoothed, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeft = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeftSmoothed = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelectSmoothed, 0.5);

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples"))
            {
                
                    
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + "All");
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + "All" + "\\" + "frontToBack");
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionSamples" + "\\" + "All" + "\\" + "frontToBack");
            }
           
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackBack", splittedFrontToBack[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackFront", splittedFrontToBack[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackSmoothedBack", splittedFrontToBackSmoothed[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackSmoothedFront", splittedFrontToBackSmoothed[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftRight", splittedRightToLeft[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftLeft", splittedRightToLeft[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftSmoothedRight", splittedRightToLeftSmoothed[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftSmoothedLeft", splittedRightToLeftSmoothed[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "All", extractor.rawSamplesPerSelect);
            extractor.calculateAndWriteWallProjectionSamples(collector, "AllSmoothed", extractor.rawSamplesPerSelectSmoothed);

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "WallProjectionAndPositionSamples");
            }

            
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackBack", splittedFrontToBack[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackFront", splittedFrontToBack[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackSmoothedBack", splittedFrontToBackSmoothed[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackSmoothedFront", splittedFrontToBackSmoothed[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftRight", splittedRightToLeft[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftLeft", splittedRightToLeft[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftSmoothedRight", splittedRightToLeftSmoothed[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftSmoothedLeft", splittedRightToLeftSmoothed[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "All", extractor.rawSamplesPerSelect);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "AllSmoothed", extractor.rawSamplesPerSelectSmoothed);

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples")) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples");
            }
          
            extractor.writeNormalSamplesFromRawSamples("frontToBackBack", splittedFrontToBack[0]);
            extractor.writeNormalSamplesFromRawSamples("frontToBackFront", splittedFrontToBack[1]);
            extractor.writeNormalSamplesFromRawSamples("frontToBackSmoothedBack", splittedFrontToBackSmoothed[0]);
            extractor.writeNormalSamplesFromRawSamples("frontToBackSmoothedFront", splittedFrontToBackSmoothed[1]);
            extractor.writeNormalSamplesFromRawSamples("rightToLeftRight", splittedRightToLeft[0]);
            extractor.writeNormalSamplesFromRawSamples("rightToLeftLeft", splittedRightToLeft[1]);
            extractor.writeNormalSamplesFromRawSamples("rightToLeftSmoothedRight", splittedRightToLeftSmoothed[0]);
            extractor.writeNormalSamplesFromRawSamples("rightToLeftSmoothedLeft", splittedRightToLeftSmoothed[1]);
            extractor.writeNormalSamplesFromRawSamples("All", extractor.rawSamplesPerSelect);
            extractor.writeNormalSamplesFromRawSamples("AllSmoothed", extractor.rawSamplesPerSelectSmoothed);

            Console.WriteLine("OnlineLearningCalculation");
            int d = extractor.rawSamplesPerSelect.Count / 10;
            List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, d);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);
            Console.WriteLine("AfterOLC");
            for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineAll" + i, onlineLearningSplitMerged[i]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineAll" + i , onlineLearningSplitMerged[i]);
                extractor.writeNormalSamplesFromRawSamples("OnlineAll" + i, onlineLearningSplitMerged[i]);

                List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMerged[i], 0.5);
                List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMerged[i], 0.5);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackBack" + i , onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackFront" + i , onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackBack" + i , onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackFront" + i , onlineFrontToBack[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackBack" + i , onlineFrontToBack[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackFront" + i , onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftRight" + i , onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftLeft" + i , onlineRightToLeft[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftRight" + i , onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftLeft" + i , onlineRightToLeft[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftRight" + i  , onlineRightToLeft[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftLeft" + i , onlineRightToLeft[1]);
            }

            for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineAllSmoothed" + i  , onlineLearningSplitMergedSmoothed[i]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineAllSmoothed" + i , onlineLearningSplitMergedSmoothed[i]);
                extractor.writeNormalSamplesFromRawSamples("OnlineAllSmoothed" + i  , onlineLearningSplitMergedSmoothed[i]);

                List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMergedSmoothed[i], 0.5);
                List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMergedSmoothed[i], 0.5);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackSmoothedBack" + i, onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackSmoothedFront" + i, onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackSmoothedBack" + i, onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackSmoothedFront" + i, onlineFrontToBack[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackSmoothedBack" + i, onlineFrontToBack[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackSmoothedFront" + i, onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftSmoothedRight" + i, onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftSmoothedLeft" + i, onlineRightToLeft[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftSmoothedRight" + i, onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftSmoothedLeft" + i, onlineRightToLeft[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftSmoothedRight" + i, onlineRightToLeft[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftSmoothedLeft" + i, onlineRightToLeft[1]);
            }

        }



    }
}
