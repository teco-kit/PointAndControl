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
        public List<WallProjectionSample> pendingSamples { get; set; }
        KNNGenerator generator { get; set; }
        LearningModel learned { get; set; }
        public List<deviceRep> devicesRepresentation { get; set; }

        public bool initialized { get; set; }

        public struct deviceRep
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

            pendingSamples = new List<WallProjectionSample>();

            devicesRepresentation = new List<deviceRep>();
            
            foreach (WallProjectionSample sample in samples)
            {
                checkAndWriteColorForDevice(sample.sampleDeviceName);
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

            foreach (deviceRep dColor in devicesRepresentation)
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
            foreach (deviceRep c in devicesRepresentation)
            {
                if (c.deviceName.Equals(deviceName))
                {
                    return;
                }
            }

            deviceRep newColor = new deviceRep();
            newColor.deviceName = deviceName;
            newColor.color = pickRandomColor();
            Console.WriteLine("DeviceName: " + newColor.deviceName + " Color: " + newColor.color.Name);
            Console.WriteLine("");
            devicesRepresentation.Add(newColor);

        }

        public void trainClassifier()
        {
            if (samples.Count > 0)
            {
                generator.K = (int)Math.Sqrt(samples.Count);
                learned = Learner.Learn(samples, 0.80, 1000, generator);
            }
            else Console.WriteLine("Please create samples first!");
        }

        public WallProjectionSample classify(WallProjectionSample sample)
        {
            if (learned != null)
            {
                return learned.Model.Predict(sample);
            }
            else return null;
        }

        public void learnBatch(List<WallProjectionSample> trainingSamples)
        {
            if (trainingSamples == null) return;
            
            if (initialized == true)
            {
                foreach (WallProjectionSample s in trainingSamples)
                {
                    samples.Add(s);
                    checkAndWriteColorForDevice(s.sampleDeviceName);
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

        public Color deviceColorLookupByName(String name)
        {
            String nameLower = name.ToLower();
            foreach (deviceRep device in devicesRepresentation)
            {
                if (nameLower.Equals(device.deviceName.ToLower()))
                {
                    return device.color;
                }
            }
            return Color.White;
        }

        private bool checkIfNewLabel(WallProjectionSample sample)
        {
            bool newLabel = false;

            foreach (deviceRep rep in devicesRepresentation)
            {
                if (rep.deviceName == sample.sampleDeviceName)
                {
                    newLabel = true;
                    return newLabel;
                }
            }

            return newLabel;
        }
        
    }
}