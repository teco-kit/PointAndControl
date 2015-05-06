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
        int distinctLabels { get; set; }
        public bool initialized { get; set; }
        List<String> labels { get; set; }
        public struct deviceRep
        {
            public String deviceName;
            public Color color;
        }


        public KNNClassifier(List<WallProjectionSample> list)
        {
            distinctLabels = 0;
            labels = new List<string>();

            foreach (WallProjectionSample wps in list)
            {
                if(!labels.Contains(wps.sampleDeviceName))
                {
                    labels.Add(wps.sampleDeviceName);
                    distinctLabels++;
                }
            }
            

            pendingSamples = new List<WallProjectionSample>();
            samples = new List<WallProjectionSample>();
            devicesRepresentation = new List<deviceRep>();
            
            var descriptor = Descriptor.Create<WallProjectionSample>();
            generator = new KNNGenerator();
            generator.Descriptor = descriptor;
            
            
            if (list != null && list.Count != 0)
            {
                learnBatch(list);
            }
                       
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
            devicesRepresentation.Add(newColor);
            return;
          

        }

        public void trainClassifier()
        {
           
            if (samples.Count > 0)
            {
                generator.K = (int)Math.Sqrt(samples.Count);
                learned = Learner.Learn(samples, 0.80, 500, generator);
            }
            else Console.WriteLine("Please create samples first!");

        }

        public void trainClassifier(List<WallProjectionSample> trainingSet)
        {
            if (trainingSet.Count > 0)
            {
                generator.K = (int)Math.Sqrt(trainingSet.Count);
                learned = Learner.Learn(trainingSet, 0.80, 500, generator);
            }
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
            if (trainingSamples == null || trainingSamples.Count == 0) { return; }
            
          
                foreach (WallProjectionSample s in trainingSamples)
                {
                    samples.Add(s);
                    checkAndWriteColorForDevice(s.sampleDeviceName);
                }
                Console.WriteLine("Going into trainClassifier");
            trainClassifier();
            
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