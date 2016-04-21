using PointAndControl.Helperclasses;
using PointAndControl.MainComponents;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    public class ClassificationHandler 
    {

        public KNNClassifier knnClassifier { get; set; }
        
        public WPSampleCalculator sCalculator { get; set; }

        public ClassificationHandler(CoordTransform transformer, DataHolder data)
        {

            knnClassifier = new KNNClassifier(XMLComponentHandler.readWallProjectionSamplesFromXML(), -1);
        
            sCalculator = new WPSampleCalculator(data._roomModel);
           
        }

       
        public WallProjectionSample classify(WallProjectionSample sample)
        {
            WallProjectionSample predictedSample = knnClassifier.classify(sample);

            return predictedSample;
        }

        public List<WallProjectionSample> getSamples()
        {
            return knnClassifier.trainingSamples;
        }

        public void retrainClassifier(List<WallProjectionSample> samples)
        {
            knnClassifier.learnBatch(samples);
        }

        public void retrainClassifier()
        {
            knnClassifier.trainClassifier();
        }


        public String calculateWallProjectionSampleAndLearn(Point3D[] vectors, String deviceIdentifier)
        {   
                WallProjectionSample sample = sCalculator.calculateSample(vectors, deviceIdentifier);

                if (sample.sampledeviceIdentifier.Equals("nullSample") == false)
                {

                    XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                    //Point3D p = new Point3D(vectors[2].X, vectors[2].Y, vectors[2].Z);
                    //XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                    //XMLComponentHandler.writeSampleToXML(vectors, sample.sampledeviceIdentifier);

                    knnClassifier.addSampleAndLearn(sample);

                    return "Sample gesammelt und Klassifikator trainiert";

                }


                return "Es ist ein Fehler beim Erstellen des Samples aufgetreten, bitte versuchen sie es erneut!";

            
        }

    }
}
