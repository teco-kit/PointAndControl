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


        public KNNClassifier()
        {
            var descriptor = Descriptor.Create<WallProjectionSample>();
            generator = new KNNGenerator();
            List<WallProjectionSample> tmpSamples = XMLComponentHandler.readWallProjectionSamplesFromXML();
            labels = new List<string>();
            initialized = false;
            foreach (WallProjectionSample sample in tmpSamples)
            {
                if(labels.Contains(sample.sampleDeviceName) == false)
                {
                    labels.Add(sample.sampleDeviceName);
                }
            }

            deviceSampleColors = new List<deviceColor>();
            filldeviceSampleCOlorsFixed();
            //foreach (String label in labels)
            //{
            //    deviceColor tmp = new deviceColor();
            //    tmp.deviceName = label;
            //    // insert Random color here
            //}

            

            generator.Descriptor = descriptor;
            initialized = true;
        }

        private void filldeviceSampleCOlorsFixed()
        {

            deviceColor exBox = new deviceColor();
            exBox.deviceName = "ExampleBoxee";
            exBox.color = Color.Yellow;

            deviceColor exPlug = new deviceColor();
            exPlug.deviceName = "ExamplePlugwise";
            exPlug.color = Color.Violet;

            deviceColor TV = new deviceColor();
            TV.deviceName = "TV";
            TV.color = Color.Purple;

            deviceColor XBox = new deviceColor();
            XBox.deviceName = "XBoxee";
            XBox.color = Color.Green;

            deviceColor eMarkt = new deviceColor();
            eMarkt.deviceName = "Energiemarkt";
            eMarkt.color = Color.Gray;

            deviceColor drucker = new deviceColor();
            drucker.deviceName = "Drucker";
            drucker.color = Color.Khaki;

            deviceColor lMarkt = new deviceColor();
            lMarkt.deviceName = "LampeMarkt";
            lMarkt.color = Color.Maroon;

            deviceColor lampe = new deviceColor();
            lampe.deviceName = "LEDLampe";
            lampe.color = Color.Black;

            deviceSampleColors.Add(exBox);
            deviceSampleColors.Add(exPlug);
            deviceSampleColors.Add(TV);
            deviceSampleColors.Add(XBox);
            deviceSampleColors.Add(eMarkt);
            deviceSampleColors.Add(drucker);
            deviceSampleColors.Add(lMarkt);
            deviceSampleColors.Add(lampe);

        }

        //public void addSample(KNNSample sample)
        //{
        //    KNNSample[] tmp = new KNNSample[samples.Length + 1];

        //    for (int i = 0; i < samples.Length; i++)
        //    {
        //        tmp[i] = samples[i];
        //    }
        //    tmp[samples.Length + 1] = sample;
        //    samples = tmp;
        //}

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
            }
            else
            {
                samples = trainingSamples;
            }

            trainClassifier();
        }

        public void learnOnline(WallProjectionSample s)
        {
            samples.Add(s);
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