
using numl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.KNN
{
    public class KNNSample
    {
        [Feature]
        public double x { get; set; }
        [Feature]
        public double y { get; set; }
        [Feature]
        public double z { get; set; }
        [Label]
        public String sampleDeviceName { get; set; }

        public KNNSample(Point3D position)
        {
            this.x = position.X;
            this.y = position.Y;
            this.z = position.Z;
        }

        public KNNSample()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }

        public KNNSample(Point3D position, string deviceName)
        {
            this.x = position.X;
            this.y = position.Y;
            this.z = position.Z;
            sampleDeviceName = deviceName;
        }

        public void labelSample(string label)
        {

        }
    }
}
