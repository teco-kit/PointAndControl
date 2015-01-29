using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.Helperclasses
{
    public class SampleSplitter
    {

        public struct deviceCategory
        {
            public String category;
            public List<categorySpec> specs;

        }
        public struct categorySpec
        {
            public String spec;
            public List<String> deviceNames;
        }
        public List<deviceCategory> categories { get; set; }
        public SampleSplitter()
        {
            categories = new List<deviceCategory>();
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
        public List<SampleExtractor.rawSample>[] splitRoomFrontAndBack(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {
            String[] roomMeasures = XMLComponentHandler.readRoomComponents();

            double depth = double.Parse(roomMeasures[2]);
            double splitDistance = depth * splitamount;

            List<SampleExtractor.rawSample>[] split = new List<SampleExtractor.rawSample>[2];

            List<SampleExtractor.rawSample> firstArea = new List<SampleExtractor.rawSample>();
            List<SampleExtractor.rawSample> secondArea = new List<SampleExtractor.rawSample>();

            foreach (SampleExtractor.rawSample sample in rawSampleList)
            {
                if (sample.position.Z <= splitDistance)
                {
                    firstArea.Add(sample);
                }
                else
                {
                    secondArea.Add(sample);
                }
            }

            split[0] = firstArea;
            split[1] = secondArea;

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
        public List<SampleExtractor.rawSample>[] splitRoomRightAndLeft(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {
            String[] roomMeasures = XMLComponentHandler.readRoomComponents();

            double width = double.Parse(roomMeasures[0]);
            double splitDistance = width * splitamount;

            List<SampleExtractor.rawSample>[] split = new List<SampleExtractor.rawSample>[2];

            List<SampleExtractor.rawSample> firstArea = new List<SampleExtractor.rawSample>();
            List<SampleExtractor.rawSample> secondArea = new List<SampleExtractor.rawSample>();

            foreach (SampleExtractor.rawSample sample in rawSampleList)
            {
                if (sample.position.X <= splitDistance)
                {
                    firstArea.Add(sample);
                }
                else
                {
                    secondArea.Add(sample);
                }
            }

            split[0] = firstArea;
            split[1] = secondArea;

            return split;
        }


        /// <summary>
        /// Splits the number of rawSamples on the given splitamount ( splitamount 9/10 results in a 9/10, 1/10 split)
        /// Returned arrayindice 0 : <= splitamount. 1 : > splitamount
        /// </summary>
        /// <param name="rawSampleList"></param>
        /// <param name="splitamount"></param>
        /// <returns></returns>
        public List<SampleExtractor.rawSample>[] splitSamplesForParts(List<SampleExtractor.rawSample> rawSampleList, double splitamount)
        {
            List<SampleExtractor.rawSample>[] split = new List<SampleExtractor.rawSample>[2];

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

            split[0] = firstPart;
            split[1] = secondPart;

            return split;
        }

        public void createNewCategory(String category)
        {
            
            foreach (deviceCategory c in categories)
            {
                if (c.category == category)
                {
                    return;
                }
            }

            deviceCategory cat = new deviceCategory();

            cat.category = category;
            cat.specs = new List<categorySpec>();
            
           

            return ;
        }

        public void createNewCategorySpec(String category, String specName)
        {
            
            foreach (deviceCategory c in categories)
            {
                if (category == c.category)
                {
                    foreach (categorySpec spec in c.specs)
                    {
                        if (spec.spec == specName)
                        {
                            return;
                        }
                    }
                    categorySpec newSpec = new categorySpec();
                    newSpec.spec = specName;
                    newSpec.deviceNames = new List<string>();

                    c.specs.Add(newSpec);
                    return;
                }
            }
        }

        public void addDeviceToSpec(String category, String specName, String deviceName)
        {
            foreach (deviceCategory c in categories)
            {
                if (c.category == category) {
                    foreach (categorySpec s in c.specs)
                    {
                        if (s.spec == specName)
                        {
                            foreach (String d in s.deviceNames)
                            {
                                if (d == deviceName)
                                {
                                    return;
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}
