using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using numl;
using numl.Supervised.KNN;
using numl.Model;
using IGS.Helperclasses;
using System.Drawing;


namespace IGS.KNN
{
    public class KNNClassifier
    {
        public List<WallProjectionSample> samples { get; set; }
        public List<String> labels { get; set; }
        KNNGenerator generator { get; set; }
        LearningModel learned { get; set; }
        public List<deviceColor> deviceSampleColors { get; set; }

        public bool initialized { get; set; }

        public struct deviceColor
        {
            public String deviceName;
            public Color color;
        }


        public KNNClassifier(List<WallProjectionSample> list)
        {
       

            var descriptor = Descriptor.Create<WallProjectionSample>();
            generator = new KNNGenerator();
            generator.Descriptor = descriptor;
            
            initialized = false;

            learnBatch(list);
            trainClassifier();

            labels = new List<string>();

            deviceSampleColors = new List<deviceColor>();
            
            foreach (WallProjectionSample sample in samples)
            {
                if(labels.Contains(sample.sampleDeviceName) == false)
                {
                    labels.Add(sample.sampleDeviceName);
                    checkAndWriteColorForDevice(sample.sampleDeviceName);
                }
            }
            
            
           
           
           
            initialized = true;
        }


        public Color pickRandomColor()
        {
            bool uniqueFound = true;
            
            Random random = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[random.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);

            foreach (deviceColor dColor in deviceSampleColors)
            {
                if(dColor.color.Equals(randomColor))
                {
                    uniqueFound = false;
                }
            }
            if (uniqueFound == false)
            {
                randomColor = pickRandomColor();
            }
            
            return randomColor;
        }

        public void checkAndWriteColorForDevice(String deviceName)
        {
            foreach (deviceColor c in deviceSampleColors)
            {
                if (c.deviceName.Equals(deviceName))
                {
                    return;
                }
            }

            deviceColor newColor = new deviceColor();
            newColor.deviceName = deviceName;
            newColor.color = pickRandomColor();
            Console.WriteLine("DeviceName: " + newColor.deviceName + " Color: " + newColor.color.Name);
            Console.WriteLine("");
            deviceSampleColors.Add(newColor);

        }

        public void trainClassifier()
        {
            generator.K = (int)Math.Sqrt(samples.Count);
            learned = Learner.Learn(samples, 0.80, 1000, generator);
        }

        public WallProjectionSample classify(WallProjectionSample sample)
        {
       
            return learned.Model.Predict(sample);
        }

        public void learnBatch(List<WallProjectionSample> trainingSamples)
        {
            if (initialized == true)
            {
                foreach (WallProjectionSample s in trainingSamples)
                {
                    samples.Add(s);
                    XMLComponentHandler.writeWallProjectionSampleToXML(s);
                }

                trainClassifier();
            }
            else
            {
                samples = trainingSamples;
            }
        }

        public void learnOnline(WallProjectionSample s)
        {
            samples.Add(s);
            checkAndWriteColorForDevice(s.sampleDeviceName);
            trainClassifier();
        }

        public Color deviceColorLookup(String name)
        {
            String nameLower = name.ToLower();
            foreach (deviceColor device in deviceSampleColors)
            {
                if (nameLower.Equals(device.deviceName.ToLower()))
                {
                    return device.color;
                }
            }
            return Color.White;
        }

        
    }
}