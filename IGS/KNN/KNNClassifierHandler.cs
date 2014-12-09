using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using numl;
using numl.Supervised.KNN;
using numl.Model;


namespace IGS.KNN
{
    public class KNNClassifierHandler
    {
        public KNNSample[] samples { get; set; }
        KNNGenerator generator { get; set; }
        LearningModel learned { get; set; }

        public KNNClassifierHandler()
        {
            var descriptor = Descriptor.Create<KNNSample>();
            generator = new KNNGenerator();
            samples = new KNNSample[1];
           
            generator.Descriptor = descriptor;
        }


        public void addSample(KNNSample sample)
        {
            KNNSample[] tmp = new KNNSample[samples.Length + 1];
        
            for (int i = 0; i < samples.Length; i++)
            {
                tmp[i] = samples[i];
            }
            tmp[samples.Length + 1] = sample;
            samples = tmp;
        }

        public void trainClassifier()
        {
            learned = Learner.Learn(samples, 0.80, 1000, generator);
            generator.K = (int)Math.Sqrt(samples.Length);
        }

        public KNNSample classify(KNNSample sample)
        {
            return learned.Model.Predict(sample);
        }


    }
}
