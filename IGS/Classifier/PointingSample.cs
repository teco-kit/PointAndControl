using numl.Model;
using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    class PointingSample
    {

        [Feature]
        public double directionX { get; set; }
        [Feature]
        public double directionY { get; set; }
        [Feature]
        public double directionZ { get; set; }

        [Feature]
        public double shoulderX { get; set; }
        [Feature]
        public double shoulderY { get; set; }
        [Feature]
        public double shoulderZ { get; set; }
        [Label]
        public String sampledeviceIdentifier { get; set; }

        public PointingSample(Point3D shoulder, Vector3D direction)
        {
            directionX = direction.X;
            directionY = direction.Y;
            directionZ = direction.Z;

            shoulderX = shoulder.X;
            shoulderY = shoulder.Y;
            shoulderZ = shoulder.Z;

            sampledeviceIdentifier = "";
        }

        public PointingSample(Point3D shoulder, Vector3D direction, String sampledeviceIdentifier)
        {
            directionX = direction.X;
            directionY = direction.Y;
            directionZ = direction.Z;

            shoulderX = shoulder.X;
            shoulderY = shoulder.Y;
            shoulderZ = shoulder.Z;

            this.sampledeviceIdentifier = sampledeviceIdentifier;
        }
    }
}
