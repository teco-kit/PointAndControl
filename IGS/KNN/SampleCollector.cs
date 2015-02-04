using HelixToolkit.Wpf;
using IGS.Helperclasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.KNN
{
    public class SampleCollector
    {
        public List<WallProjectionSample> sampleList {get; set;}
        public Room calcRoomModel { get; set; }
        
        public SampleCollector(KNNClassifier handler)
        {
            String[] roomComps = XMLComponentHandler.readRoomComponents();
            calcRoomModel = new Room(float.Parse(roomComps[0]),  float.Parse(roomComps[1]) , float.Parse(roomComps[2]));

            //if (handler.samples.Count != 0)
            //{
            //    new Thread(delegate()
            //    {
            //        calcRoomModel.calculateDeviceAreas(handler);
            //    }).Start();
            //}
        }

        public WallProjectionSample calculateSample(Vector3D[] vectors, String devName)
        {   
            Vector3D direction = Vector3D.Subtract(vectors[3], vectors[2]);
            Ray3D ray = new Ray3D(vectors[2].ToPoint3D(), direction);
            Point3D samplePoint = calcRoomModel.intersectAndTestAllWalls(ray);
            WallProjectionSample sample = new WallProjectionSample(new Point3D(), "nullSample");

            if ((samplePoint.X.Equals(float.NaN) == false))
            {
                if (devName.Equals(""))
                {
                    sample = new WallProjectionSample(samplePoint);   
                }
                else
                {
                    sample = new WallProjectionSample(samplePoint, devName);
                }
                return sample;
            }
            return sample;
        }

        public WallProjectionSample calculateSample(Vector3D direction, Vector3D position, String devName)
        {
            
            Ray3D ray = new Ray3D(position.ToPoint3D(), direction);
            Point3D samplePoint = calcRoomModel.intersectAndTestAllWalls(ray);
            WallProjectionSample sample = new WallProjectionSample(new Point3D(), "nullSample");

            if ((samplePoint.X.Equals(float.NaN) == false))
            {
                if (devName.Equals(""))
                {
                    sample = new WallProjectionSample(samplePoint);
                }
                else
                {
                    sample = new WallProjectionSample(samplePoint, devName);
                }
                return sample;
            }
            return sample;
        }
    }
}
