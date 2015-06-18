using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using numl;
using numl.Supervised;
using numl.Supervised.KNN;
using numl.Model;
using IGS.Helperclasses;
using System.Drawing;


namespace IGS.Classifier
{
    public class KNNClassifier
    {
        public List<WallProjectionSample> trainingSamples { get; set; }
        KNNGenerator generator { get; set; }
        LearningModel learned { get; set; }


            
        public int trainingSetSize { get; set; }
       

        public KNNClassifier(List<WallProjectionSample> list, int trainingsSampleCount)
        {
        
            trainingSamples = new List<WallProjectionSample>();
 
            var descriptor = Descriptor.Create<WallProjectionSample>();
            generator = new KNNGenerator();
            generator.Descriptor = descriptor;
            trainingSetSize = trainingsSampleCount;
            
            if (list != null && list.Count != 0)
            {
                learnBatch(list);
            }
                       
        }




        public void trainClassifier()
        {
           
            if (trainingSamples.Count > 0)
            {
                int k = (int)(Math.Floor(Math.Sqrt(trainingSamples.Count)));
                if (k == 0)
                {
                    k = 1;
                }

                generator.K = k;
                
                learned = Learner.Learn(trainingSamples, 0.99, 1, generator);
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
            if (trainingSamples.Count > 0)
            {
                generator.K = (int)Math.Sqrt(trainingSamples.Count);
                learned = Learner.Learn(trainingSamples, 0.99, iterations, generator);
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
                    trainingSamples.Add(s);

                    if (trainingSetSize > 0 && trainingSamples.Count > trainingSetSize)
                    {
                        int diff = Math.Abs(trainingSamples.Count - trainingSetSize);

                        while (diff >= 0)
                        {
                            trainingSamples.RemoveAt(0);

                            diff--;
                        }

                    }
                }
                Console.WriteLine("Going into trainClassifier");
                trainClassifier();
              
        }    
    }
}