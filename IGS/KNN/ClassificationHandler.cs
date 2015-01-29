using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGS.Helperclasses;
using System.Windows.Media.Media3D;
using IGS.Server.IGS;

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
            knnClassifier = new KNNClassifier();
            collector = new SampleCollector(knnClassifier);
            extractor = new SampleExtractor("BA_REICHE_LogFile");
            splitter = new SampleSplitter();
            deviceClassificationCount = 0;

        }



        public void onlineLearn(User u)
        {
            knnClassifier.learnOnline(u.lastClassDevSample);
            u.deviceIDChecked = true;
            u.lastClassDevSample = null;
        }

       
        public WallProjectionSample classify(WallProjectionSample sample)
        {
            WallProjectionSample predictedSample = knnClassifier.classify(sample);
            deviceClassificationCount++;

            return predictedSample;
        }
    }
}
