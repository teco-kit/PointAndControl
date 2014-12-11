using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using numl;
using numl.Supervised.KNN;
using numl.Model;
using IGS.Helperclasses;


namespace IGS.KNN
{
    public class KNNClassifierHandler
    {
        public List<KNNSample> samples { get; set; }
        KNNGenerator generator { get; set; }
        LearningModel learned { get; set; }

        public KNNClassifierHandler()
        {
            var descriptor = Descriptor.Create<KNNSample>();
            generator = new KNNGenerator();
            samples = XMLComponentHandler.readKNNSamplesFromXML();
           
            generator.Descriptor = descriptor;
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

        public KNNSample classify(KNNSample sample)
        {
            trainClassifier();
            return learned.Model.Predict(sample);
        }


    }
}
