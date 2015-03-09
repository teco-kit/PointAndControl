using IGS.KNN;
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
        ///    2
        /// -------
        ///    1   .origin
        /// 
        /// area number:arrayindice |  1: 0; 2:1;
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
                if (sample.positionShoulder.Z <= splitDistance)
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
        /// 2  |  1
        ///    |    .origin
        /// 
        /// area number:arrayindice |  1: 0; 2:1;
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
                if (sample.positionShoulder.X <= splitDistance)
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
        /// Splits the number of rawSamples on the given splitamount ( splitamount 9/10 results in a 9/10, 1/10 split)
        /// Returned arrayindice 0 : <= splitamount. 1 : > splitamount
        /// </summary>
        /// <param name="rawSampleList"></param>
        /// <param name="splitamount"></param>
        /// <returns></returns>
        public List<List<SampleExtractor.rawSample>> splitSamplesForParts(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {
            List<List<SampleExtractor.rawSample>> split = new List<List<SampleExtractor.rawSample>>();

            List<SampleExtractor.rawSample> firstPart = new List<SampleExtractor.rawSample>();
            List<SampleExtractor.rawSample> secondPart = new List<SampleExtractor.rawSample>();
            Double splitNumber = Math.Floor(rawSampleList.Count * splitamount);

            for (int i = 0; i < rawSampleList.Count; i++)
            {
                if (i <= splitNumber)
                {
                    firstPart.Add(rawSampleList[i]);
                }
                else
                {
                    secondPart.Add(rawSampleList[i]);
                }
            }

            split.Add(firstPart);
            split.Add(secondPart);

            return split;
        }


        /// <summary>
        /// Create Equal packages of random chosen rawSamples to simulate Onlinelearning
        /// </summary>
        /// <param name="rawSamples"></param>
        /// <param name="packageSize"></param>
        /// <returns></returns>
        public List<List<SampleExtractor.rawSample>> splitRawSamplesRandomForOnline(List<SampleExtractor.rawSample> rawSamples, int packageSize)
        {
            List<List<SampleExtractor.rawSample>> result = new List<List<SampleExtractor.rawSample>>();
            int chosenPlace = 0;
            Random rand = new Random();

            while (rawSamples.Count >= packageSize)
            {
                List<SampleExtractor.rawSample> package = new List<SampleExtractor.rawSample>();
                for (int i = 0; i < packageSize; i++)
                {
                    chosenPlace = rand.Next(0, rawSamples.Count);
                    package.Add(rawSamples[chosenPlace]);
                    rawSamples.RemoveAt(chosenPlace);
                }
                result.Add(package);
            }

            return result;
        }

    }
}
