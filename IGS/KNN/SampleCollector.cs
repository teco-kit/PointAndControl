using HelixToolkit.Wpf;
using IGS.Helperclasses;
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
        
        public SampleCollector(KNNClassifierHandler handler)
        {
            String[] roomComps = XMLComponentHandler.readRoomComponents();
            calcRoomModel = new Room(float.Parse(roomComps[0]), float.Parse(roomComps[2]), float.Parse(roomComps[1]));
            calcRoomModel.calculateDeviceAreas(handler);
            
        }

        public KNNSample calculateSample(Vector3D[] vectors, String devName)
        {   
            Vector3D direction = Vector3D.Subtract(vectors[3], vectors[2]);
            Ray3D ray = new Ray3D(vectors[2].ToPoint3D(), direction);
            Point3D samplePoint = calcRoomModel.intersectAndTestAllWalls(ray);
            KNNSample sample = new KNNSample(new Point3D(), "nullSample");

            if ((samplePoint.X.Equals(float.NaN) == false))
            {
                if (devName.Equals(""))
                {
                    sample = new KNNSample(samplePoint);   
                }
                else
                {
                    sample = new KNNSample(samplePoint, devName);
                }
                return sample;
            }
            return sample;
        }
    }
}
