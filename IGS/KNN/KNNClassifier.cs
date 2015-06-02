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
       

        public KNNClassifier(List<WallProjectionSample> list)
        {
        
            

            pendingSamples = new List<WallProjectionSample>();
            samples = new List<WallProjectionSample>();
 
            var descriptor = Descriptor.Create<WallProjectionSample>();
            generator = new KNNGenerator();
            generator.Descriptor = descriptor;
            
            
            if (list != null && list.Count != 0)
            {
                learnBatch(list);
            }
                       
        }




        public void trainClassifier()
        {
           
            if (samples.Count > 0)
            {
                generator.K = (int)Math.Sqrt(samples.Count);
                learned = Learner.Learn(samples, 0.99, 1, generator);
            }
            else Console.WriteLine("Please create samples first!");

        }

        public void trainClassifier(List<WallProjectionSample> trainingSet)
        {
            if (trainingSet.Count > 0)
            {
                generator.K = (int)Math.Sqrt(trainingSet.Count);
                learned = Learner.Learn(trainingSet, 0.99, 1, generator);
            }
        }


        public void trainClassifier(int iterations)
        {
            if (samples.Count > 0)
            {
                generator.K = (int)Math.Sqrt(samples.Count);
                learned = Learner.Learn(samples, 0.99, iterations, generator);
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
                    
                }
                Console.WriteLine("Going into trainClassifier");
            trainClassifier();
            
        }

        public void learnOnline(WallProjectionSample s)
        {
            samples.Add(s);
            trainClassifier();
        }    
        
    }
}