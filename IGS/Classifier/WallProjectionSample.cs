
using numl.Model;
using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    public class WallProjectionSample
    {
        [Feature]
        public double x { get; set; }
        [Feature]
        public double y { get; set; }
        [Feature]
        public double z { get; set; }
        [Label]
        public String sampledeviceIdentifier { get; set; }

        public WallProjectionSample(Point3D position)
        {
            this.x = position.X;
            this.y = position.Y;
            this.z = position.Z;
        }

        public WallProjectionSample()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }

        public WallProjectionSample(Point3D position, string deviceIdentifier)
        {
            this.x = position.X;
            this.y = position.Y;
            this.z = position.Z;
            this.sampledeviceIdentifier = deviceIdentifier;
        }

        public WallProjectionSample(WallProjectionAndPositionSample wpaps)
        {
            this.x = wpaps.wallPositionX;
            this.y = wpaps.wallPositionY;
            this.z = wpaps.wallPositionZ;
            this.sampledeviceIdentifier = wpaps.sampledeviceIdentifier;
        }

        public WallProjectionSample(WallProjectionSample wps)
        {
            this.x = wps.x;
            this.y = wps.y;
            this.z = wps.z;

            this.sampledeviceIdentifier = "";
        }

    }
}
