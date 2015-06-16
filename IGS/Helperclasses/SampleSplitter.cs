using IGS.Classifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.Helperclasses
{
    public class SampleSplitter
    {
        public SampleSplitter()
        {
       
        }

        /// <summary>
        /// Splits the given rawsamples with their personpositions in 2 areas. Definition of the Area:
        /// Area:
        ///    1
        /// -------
        ///    0   .origin
        /// 
        /// </summary>
        /// <param name="rawSampleList"></param>
        /// <param name="splitamount"></param>
        /// <returns></returns>
        public List<List<SampleExtractor.rawSample>> splitRoomFrontAndBack(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {

            String[] roomMeasures = XMLComponentHandler.readRoomComponents();

            double depth = double.Parse(roomMeasures[2]);
            double splitDistance = depth * splitamount;

            List<List<SampleExtractor.rawSample>> split = new List<List<SampleExtractor.rawSample>>();

            List<SampleExtractor.rawSample> firstArea = new List<SampleExtractor.rawSample>();
            List<SampleExtractor.rawSample> secondArea = new List<SampleExtractor.rawSample>();

            foreach (SampleExtractor.rawSample sample in rawSampleList)
            {
                if (sample.joints[2].Z <= splitDistance)
                {
                    firstArea.Add(sample);
                }
                else
                {
                    secondArea.Add(sample);
                }
            }

            split.Add(firstArea);
            split.Add(secondArea);

            return split;
        }


        /// <summary>
        /// Splits the given rawsamples with their personpositions in 2 areas. Definition of the Area:
        ///     
        ///    |
        /// 1  |  0
        ///    |    .origin
        /// 
        /// area number:arrayindice |  first area: 0; second area:1;
        /// 
        /// </summary>
        /// <param name="rawSampleList"></param>
        /// <param name="splitamount"></param>
        /// <returns></returns>
        public List<List<SampleExtractor.rawSample>> splitRoomRightAndLeft(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {
            String[] roomMeasures = XMLComponentHandler.readRoomComponents();

            double width = double.Parse(roomMeasures[0]);
            double splitDistance = width * splitamount;

            List<List<SampleExtractor.rawSample>> split = new List<List<SampleExtractor.rawSample>>();

            List<SampleExtractor.rawSample> firstArea = new List<SampleExtractor.rawSample>();
            List<SampleExtractor.rawSample> secondArea = new List<SampleExtractor.rawSample>();

            foreach (SampleExtractor.rawSample sample in rawSampleList)
            {
                if (sample.joints[2].X <= splitDistance)
                {
                    firstArea.Add(sample);
                }
                else
                {
                    secondArea.Add(sample);
                }
            }

            split.Add(firstArea);
            split.Add(secondArea);

            return split;
        }

        public List<List<SampleExtractor.rawSample>> splitInOnePerDeviceSplits(List<SampleExtractor.rawSample> rawSamples)
        {
            List<List<SampleExtractor.rawSample>> sortedByDevice = new List<List<SampleExtractor.rawSample>>();
            List<List<SampleExtractor.rawSample>> resultList = new List<List<SampleExtractor.rawSample>>();
            Random rand = new Random();
            bool found = false;
            int chosenPlace = 0;
           foreach(SampleExtractor.rawSample rs in rawSamples)
           {
               found = false;
                foreach (List<SampleExtractor.rawSample> deviceLists in sortedByDevice)
                {
                    if (deviceLists[0].label == rs.label)
                    {
                        deviceLists.Add(rs);
                        found = true;
                    }
                }
                if (found == false)
                {
                    List<SampleExtractor.rawSample> deviceList = new List<SampleExtractor.rawSample>();
                    deviceList.Add(rs);
                    sortedByDevice.Add(deviceList);
                }
            }


            int minCount = int.MaxValue;

            foreach (List<SampleExtractor.rawSample> devList in sortedByDevice)
            {
                if (devList.Count < minCount)
                {
                    minCount = devList.Count;
                }
            }

            for (int i = 0; i < minCount; i++)
            {
                List<SampleExtractor.rawSample> onlineSplit = new List<SampleExtractor.rawSample>();
                foreach (List<SampleExtractor.rawSample> deviceList in sortedByDevice)
                {
                    chosenPlace = rand.Next(0, deviceList.Count);
                    onlineSplit.Add(deviceList[chosenPlace]);
                    deviceList.Remove(deviceList[chosenPlace]);
                }
                resultList.Add(onlineSplit);
            }

                return resultList;
        }

        
    }
}
