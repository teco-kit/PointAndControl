using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using IGS.Server.IGS;
using System.Threading;

namespace IGS.KNN
{
    public class ClassificationHandler
    {

        public KNNClassifier knnClassifier { get; set; }
        public SampleCollector collector { get; set; }

        public SampleExtractor extractor { get; set; }
        public SampleSplitter splitter { get; set; }

        public ulong deviceClassificationCount { get; set; }
        public ulong deviceClassificationErrorCount { get; set; }

        public ClassificationHandler()
        {
            knnClassifier = new KNNClassifier(XMLComponentHandler.readWallProjectionSamplesFromXML());
            
            extractor = new SampleExtractor("BA_REICHE_LogFile");
            splitter = new SampleSplitter();
            deviceClassificationCount = 0;
            collector = new SampleCollector(knnClassifier);
        }



        public void onlineLearn(User u)
        {
            new Thread(delegate()
            {
                knnClassifier.learnOnline(u.lastClassDevSample);
            }).Start(); 
            
            u.deviceIDChecked = true;
            u.lastClassDevSample = null;
        }

       
        public WallProjectionSample classify(WallProjectionSample sample)
        {
            WallProjectionSample predictedSample = knnClassifier.classify(sample);
            deviceClassificationCount++;

            return predictedSample;
        }

        public void retrainClassifier(List<WallProjectionSample> samples)
        {
            knnClassifier.learnBatch(samples);
        }
    }
}
