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




                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    extractor.writeNormalSamplesFromRawSamples("\\" + j, extractor.rawSamplesPerSelect, "AllRest");
                    extractor.writeNormalSamplesFromRawSamples("\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllRestSmoothed");

                    //int d = 5;//extractor.rawSamplesPerSelect.Count / extractor.rawSamplesPerSelect.Count;
                    //int x = 5;//extractor.rawSamplesPerSelect.Count /extractor.rawSamplesPerSelect.Count ;
                    //List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
                    //List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, x);
                    //List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
                    //List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);

                    //for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
                    //{
                    //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + "Online" + "\\" + j + "\\" + "AllRest", onlineLearningSplitMerged[i], "AllRest" + i);
                    //    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + "Online" + "\\" + j + "\\" + "AllRest", onlineLearningSplitMerged[i], "AllRest" + i);
                    //    extractor.writeNormalSamplesFromRawSamples("\\" + "Online" + "\\" + j + "\\" + "AllRest", onlineLearningSplitMerged[i], "AllRest" + i);

                    //}

                    //for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
                    //{
                    //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + "Online" + "\\" + j + "\\" + "AllRestSmoothed", onlineLearningSplitMergedSmoothed[i], "AllRest" + i);
                    //    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + "Online" + "\\" + j + "\\" + "AllRestSmoothed", onlineLearningSplitMergedSmoothed[i], "AllRest" + i);
                    //    extractor.writeNormalSamplesFromRawSamples("\\" + "Online" + "\\" + j + "\\" + "AllRestSmoothed", onlineLearningSplitMergedSmoothed[i], "AllRest" + i);
                    //}

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

                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j, extractor.rawSamplesPerSelect, "AllOne");
                extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j, extractor.rawSamplesPerSelect, "AllOne");
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + j, extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");

                extractor.writeNormalSamplesFromRawSamples("\\" + j , extractor.rawSamplesPerSelect, "AllOne");
                extractor.writeNormalSamplesFromRawSamples("\\" + j , extractor.rawSamplesPerSelectSmoothed, "AllOneSmoothed");


                //int d = 5;//extractor.rawSamplesPerSelect.Count / extractor.rawSamplesPerSelect.Count;
                //int x = 5;// extractor.rawSamplesPerSelectSmoothed.Count / extractor.rawSamplesPerSelectSmoothed.Count;
                //List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
                //List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, x);
                //List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
                //List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);

                //for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
                //{
                //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + "Online" + "\\" +  j + "\\" + "AllOne", onlineLearningSplitMerged[i], "AllOne" + i);
                //    extractor.calculateAndWriteWallProjectionSamples(collector, "\\" + "Online" + "\\"+ j + "\\" + "AllOne", onlineLearningSplitMerged[i], "AllOne" + i);
                //    extractor.writeNormalSamplesFromRawSamples("\\" + "Online" + "\\" + j + "\\" + "AllOne", onlineLearningSplitMerged[i],  "AllOne" + i);

                //}

                //for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
                //{
                //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "\\" + "Online" + "\\"+  j + "\\" + "AllOneSmoothed", onlineLearningSplitMergedSmoothed[i], "AllOne" + i);
                //    extractor.calculateAndWriteWallProjectionSamples(collector,"\\" + "Online" + "\\" + j + "\\" + "AllOneSmoothed", onlineLearningSplitMergedSmoothed[i], "AllOne" + i);
                //    extractor.writeNormalSamplesFromRawSamples("\\" + "Online" + "\\" + j + "\\" + "AllOneSmoothed", onlineLearningSplitMergedSmoothed[i], "AllOne" + i);
                //}

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

           
           
            extractor.calculateAndWriteWallProjectionSamples(collector, "", splittedFrontToBack[0],"frontToBackBack");
            extractor.calculateAndWriteWallProjectionSamples(collector, "", splittedFrontToBack[1],"frontToBackFront");
            extractor.calculateAndWriteWallProjectionSamples(collector,"" , splittedFrontToBackSmoothed[0],"frontToBackSmoothedBack");
            extractor.calculateAndWriteWallProjectionSamples(collector, "", splittedFrontToBackSmoothed[1],"frontToBackSmoothedFront");
            extractor.calculateAndWriteWallProjectionSamples(collector,"" , splittedRightToLeft[0],"rightToLeftRight");
            extractor.calculateAndWriteWallProjectionSamples(collector, "", splittedRightToLeft[1],"rightToLeftLeft");
            extractor.calculateAndWriteWallProjectionSamples(collector, "", splittedRightToLeftSmoothed[0],"rightToLeftSmoothedRight");
            extractor.calculateAndWriteWallProjectionSamples(collector,"" , splittedRightToLeftSmoothed[1],"rightToLeftSmoothedLeft");
            extractor.calculateAndWriteWallProjectionSamples(collector,"" , extractor.rawSamplesPerSelect,"All");
            extractor.calculateAndWriteWallProjectionSamples(collector, "", extractor.rawSamplesPerSelectSmoothed,"AllSmoothed");

         

            
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedFrontToBack[0],"frontToBackBack");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedFrontToBack[1],"frontToBackFront");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedFrontToBackSmoothed[0],"frontToBackSmoothedBack");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedFrontToBackSmoothed[1],"frontToBackSmoothedFront");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedRightToLeft[0],"rightToLeftRight");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedRightToLeft[1],"rightToLeftLeft");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedRightToLeftSmoothed[0],"rightToLeftSmoothedRight");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", splittedRightToLeftSmoothed[1],"rightToLeftSmoothedLeft");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", extractor.rawSamplesPerSelect,"All");
            extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "", extractor.rawSamplesPerSelectSmoothed,"AllSmoothed");

           
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


            //int d = 5;//extractor.rawSamplesPerSelect.Count / extractor.rawSamplesPerSelect.Count;
            //int x = 5;//extractor.rawSamplesPerSelectSmoothed.Count / extractor.rawSamplesPerSelectSmoothed.Count;
            //List<List<SampleExtractor.rawSample>> onlineLearningSplit = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelect, d);
            //List<List<SampleExtractor.rawSample>> onlineLearningSplitSmoothed = splitter.splitRawSamplesRandomForOnline(extractor.rawSamplesPerSelectSmoothed, x);
            //List<List<SampleExtractor.rawSample>> onlineLearningSplitMerged = extractor.rawSampleOnlinePartsMerger(onlineLearningSplit);
            //List<List<SampleExtractor.rawSample>> onlineLearningSplitMergedSmoothed = extractor.rawSampleOnlinePartsMerger(onlineLearningSplitSmoothed);
           
            //for (int i = 0; i < onlineLearningSplitMerged.Count; i++)
            //{
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\All", onlineLearningSplitMerged[i], "All" + i);
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\All", onlineLearningSplitMerged[i], "All" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\All", onlineLearningSplitMerged[i], "All" + i);

            //    List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMerged[i], 0.5);
            //    List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMerged[i], 0.5);

            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\frontToBackBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\frontToBackFront", onlineFrontToBack[1],"frontToBackFront" + i );

            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\frontToBackBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\frontToBackFront", onlineFrontToBack[1], "frontToBackFront" + i);

            //    extractor.writeNormalSamplesFromRawSamples("Online\\frontToBackBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\frontToBackFront", onlineFrontToBack[1], "frontToBackFront" + i);

            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\rightToLeftRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\rightToLeftLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);

            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\rightToLeftRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\rightToLeftLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);

            //    extractor.writeNormalSamplesFromRawSamples("Online\\rightToLeftRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\rightToLeftLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);
            //}

            //for (int i = 0; i < onlineLearningSplitMergedSmoothed.Count; i++)
            //{
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\AllSmoothed", onlineLearningSplitMergedSmoothed[i], "All" + i  );
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\AllSmoothed", onlineLearningSplitMergedSmoothed[i], "All" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\AllSmoothed", onlineLearningSplitMergedSmoothed[i], "All" + i);

            //    List<List<SampleExtractor.rawSample>> onlineFrontToBack = splitter.splitRoomFrontAndBack(onlineLearningSplitMergedSmoothed[i], 0.5);
            //    List<List<SampleExtractor.rawSample>> onlineRightToLeft = splitter.splitRoomRightAndLeft(onlineLearningSplitMergedSmoothed[i], 0.5);

            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\frontToBackSmoothedBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\frontToBackSmoothedFront", onlineFrontToBack[1], "frontToBackFront" + i );

            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\frontToBackSmoothedBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\frontToBackSmoothedFront", onlineFrontToBack[1], "frontToBackFront" + i);

            //    extractor.writeNormalSamplesFromRawSamples("Online\\frontToBackSmoothedBack", onlineFrontToBack[0], "frontToBackBack" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\frontToBackSmoothedFront", onlineFrontToBack[1], "frontToBackFront" + i);

            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\rightToLeftSmoothedRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\rightToLeftSmoothedLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);

            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\rightToLeftSmoothedRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\rightToLeftSmoothedLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);

            //    extractor.writeNormalSamplesFromRawSamples("Online\\rightToLeftSmoothedRight", onlineRightToLeft[0], "rightToLeftRight" + i);
            //    extractor.writeNormalSamplesFromRawSamples("Online\\rightToLeftSmoothedLeft", onlineRightToLeft[1], "rightToLeftLeft" + i);
            //}

        }

        public void createTestSamplesOnePerDevice(CoordTransform trans)
        {
            extractor.rawSamplesPerSelect.Clear();
            extractor.rawSamplesPerSelectSmoothed.Clear();

            extractor.readSkeletonsPerSelectFromXMLAndCreateRawSamples(trans);
            extractor.readSkeletonsPerSelectSmoothedFromXMLAndCreateRawSamples(trans);


            List<List<SampleExtractor.rawSample>> onlineLearningByDevices = splitter.splitInOnePerDeviceSplits(extractor.rawSamplesPerSelect);
            List<List<SampleExtractor.rawSample>> onlineLearningByDevicesSmoothed = splitter.splitInOnePerDeviceSplits(extractor.rawSamplesPerSelectSmoothed);
     
            for (int i = 0; i < onlineLearningByDevices.Count; i++)
            {
                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);
                extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);
                extractor.writeNormalSamplesFromRawSamples("Online\\OnePerDevice", onlineLearningByDevices[i], "OnePerDevice" + i);

                extractor.calculateAndWriteWallProjectionAndPositionSamples(collector, "Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
                extractor.calculateAndWriteWallProjectionSamples(collector, "Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
                extractor.writeNormalSamplesFromRawSamples("Online\\OnePerDeviceSmoothed", onlineLearningByDevicesSmoothed[i], "OnePerDevice" + i);
            }

        }

        public void createSampleFolders()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "NormalSamples" + "\\" + "Online" + "\\" + "All");
            
        }



    }
}
