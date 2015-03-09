using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using IGS.Server.IGS;
using System.Threading;

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

        public ClassificationHandler()
        {
            knnClassifier = new KNNClassifier(XMLComponentHandler.readWallProjectionSamplesFromXML());
            
            extractor = new SampleExtractor();
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

        public void createTestLists()
        {
            List<List<SampleExtractor.rawSample>> splittedFrontToBack = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedFrontToBackSmoothed = splitter.splitRoomFrontAndBack(extractor.rawSamplesPerSelectSmoothed, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeft = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelect, 0.5);
            List<List<SampleExtractor.rawSample>> splittedRightToLeftSmoothed = splitter.splitRoomRightAndLeft(extractor.rawSamplesPerSelectSmoothed, 0.5);
            List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, 2);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, 10);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
            List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);

            

            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackBack_WallProjectionSamples", splittedFrontToBack[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackFront_WallProjectionSamples", splittedFrontToBack[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackBackSmoothed_WallProjectionSamples", splittedFrontToBackSmoothed[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "frontToBackFrontSmoothed_WallProjectionSamples", splittedFrontToBackSmoothed[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftRight_WallProjectionSamples", splittedRightToLeft[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftLeft_WallProjectionSamples", splittedRightToLeft[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftRightSmoothed_WallProjectionSamples", splittedRightToLeftSmoothed[0]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "rightToLeftLeftSmoothed_WallProjectionSamples", splittedRightToLeftSmoothed[1]);
            extractor.calculateAndWriteWallProjectionSamples(collector, "All_WallProjectionSamples", extractor.rawSamplesPerSelect);
            extractor.calculateAndWriteWallProjectionSamples(collector, "AllSmoothed_WallProjectionSamples", extractor.rawSamplesPerSelectSmoothed);


            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackBack_WallProjectionAndPositionSamples", splittedFrontToBack[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackFront_WallProjectionAndPositionSamples", splittedFrontToBack[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackBackSmoothed_WallProjectionAndPositionSamples", splittedFrontToBackSmoothed[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "frontToBackFrontSmoothed_WallProjectionAndPositionSamples", splittedFrontToBackSmoothed[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftRight_WallProjectionAndPositionSamples", splittedRightToLeft[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftLeft_WallProjectionAndPositionSamples", splittedRightToLeft[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftRightSmoothed_WallProjectionAndPositionSamples", splittedRightToLeftSmoothed[0]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "rightToLeftLeftSmoothed_WallProjectionAndPositionSamples", splittedRightToLeftSmoothed[1]);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "All_WallProjectionAndPositionSamples", extractor.rawSamplesPerSelect);
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "AllSmoothed_WallProjectionAndPositionSamples", extractor.rawSamplesPerSelectSmoothed);

            extractor.writeNormalSamplesFromRawSamples( "frontToBackBack_Samples", splittedFrontToBack[0]);
            extractor.writeNormalSamplesFromRawSamples( "frontToBackFront_Samples", splittedFrontToBack[1]);
            extractor.writeNormalSamplesFromRawSamples( "frontToBackBackSmoothed_Samples", splittedFrontToBackSmoothed[0]);
            extractor.writeNormalSamplesFromRawSamples( "frontToBackFrontSmoothed_Samples", splittedFrontToBackSmoothed[1]);
            extractor.writeNormalSamplesFromRawSamples( "rightToLeftRight_Samples", splittedRightToLeft[0]);
            extractor.writeNormalSamplesFromRawSamples( "rightToLeftLeft_Samples", splittedRightToLeft[1]);
            extractor.writeNormalSamplesFromRawSamples( "rightToLeftRightSmoothed_Samples", splittedRightToLeftSmoothed[0]);
            extractor.writeNormalSamplesFromRawSamples( "rightToLeftLeftSmoothed_Samples", splittedRightToLeftSmoothed[1]);
            extractor.writeNormalSamplesFromRawSamples( "All_Samples", extractor.rawSamplesPerSelect);
            extractor.writeNormalSamplesFromRawSamples( "AllSmoothed_Samples", extractor.rawSamplesPerSelectSmoothed);

            for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineAll" + "i" + "_WallProjectionAndPositionSamples", onlineLearningSplitMerged[i]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineAll" + "i" + "_WallProjectionSamples", onlineLearningSplitMerged[i]);
                extractor.writeNormalSamplesFromRawSamples("OnlineAll" + "i" + "_Samples", onlineLearningSplitMerged[i]);

                List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMerged[i], 0.5);
                List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMerged[i], 0.5);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackBack" + i + "_WallProjectionSamples", onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinefrontToBackFront" + i + "_WallProjectionSamples", onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackBack" + i + "_WallProjectionAndPositionSamples", onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinefrontToBackFront" + i + "_WallProjectionAndPositionSamples", onlineFrontToBack[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackBack" + i + "_Samples", onlineFrontToBack[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinefrontToBackFront" + i + "_Samples", onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftRight" + i + "_WallProjectionSamples", onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlinerightToLeftLeft" + i + "_WallProjectionSamples", onlineRightToLeft[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftRight" + i + "_WallProjectionAndPositionSamples", onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlinerightToLeftLeft" + i + "_WallProjectionAndPositionSamples", onlineRightToLeft[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftRight" + i + "_Samples", onlineRightToLeft[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlinerightToLeftLeft" + i + "_Samples", onlineRightToLeft[1]);
            }

            for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineSmoothed" + "i" + "_WallProjectionAndPositionSamples", onlineLearningSplitMergedSmoothed[i]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineSmoothed" + "i" + "_WallProjectionSamples", onlineLearningSplitMergedSmoothed[i]);
                extractor.writeNormalSamplesFromRawSamples("OnlineSmoothed" + "i" + "_Samples", onlineLearningSplitMergedSmoothed[i]);

                List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMergedSmoothed[i], 0.5);
                List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMergedSmoothed[i], 0.5);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineSmoothedfrontToBackBack" + i + "_WallProjectionSamples", onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineSmoothedfrontToBackFront" + i + "_WallProjectionSamples", onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineSmoothedfrontToBackBack" + i + "_WallProjectionAndPositionSamples", onlineFrontToBack[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineSmoothedfrontToBackFront" + i + "_WallProjectionAndPositionSamples", onlineFrontToBack[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlineSmoothedfrontToBackBack" + i + "_Samples", onlineFrontToBack[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlineSmoothedfrontToBackFront" + i + "_Samples", onlineFrontToBack[1]);

                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineSmoothedrightToLeftRight" + i + "_WallProjectionSamples", onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionSamples(collector, "OnlineSmoothedrightToLeftLeft" + i + "_WallProjectionSamples", onlineRightToLeft[1]);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineSmoothedrightToLeftRight" + i + "_WallProjectionAndPositionSamples", onlineRightToLeft[0]);
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "OnlineSmoothedrightToLeftLeft" + i + "_WallProjectionAndPositionSamples", onlineRightToLeft[1]);

                extractor.writeNormalSamplesFromRawSamples("OnlineSmoothedrightToLeftRight" + i + "_Samples", onlineRightToLeft[0]);
                extractor.writeNormalSamplesFromRawSamples("OnlineSmoothedrightToLeftLeft" + i + "_Samples", onlineRightToLeft[1]);
            }

        }



    }
}
