using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.KNN
{
    public class SampleCollector
    {
        public List<KNNSample> sampleList {get; set;}
        public Room calcRoomModel { get; set; }
        
        public SampleCollector()
        {
            sampleList = new List<KNNSample>();
            calcRoomModel = new Room();
        }

        public KNNSample calculateSample(Vector3D[] vectors, String devID)
        {   
            Vector3D direction = Vector3D.Subtract(vectors[1], vectors[0]);
            Ray3D ray = new Ray3D(vectors[0].ToPoint3D(), direction);
            Point3D samplePoint = calcRoomModel.intersectAndTestAllWalls(ray);
            KNNSample sample = new KNNSample(new Point3D(), "nullSample");

            if ((samplePoint.X.Equals(float.NaN) == false))
            {
                if (devID.Equals(""))
                {
                    sample = new KNNSample(samplePoint);   
                }
                else
                {
                    sample = new KNNSample(samplePoint, devID);
                }
                return sample;
            }
            return sample;
        }

        public void writeSamplesToXML()
        {

        }
    }
}
