﻿using numl.Model;
using System;
using System.Windows.Media.Media3D;

namespace PointAndControl.Classifier
{
    public class WallProjectionAndPositionSample 
    {
        [Feature]
        public double wallPositionX { get; set; }
        [Feature]
        public double wallPositionY { get; set; }
        [Feature]
        public double wallPositionZ { get; set; }

        [Feature]
        public double personPositionX { get; set; }
        [Feature]
        public double personPositionY { get; set; }
        [Feature]
        public double personPositionZ { get; set; }
        [Label]
        public String sampledeviceIdentifier { get; set; }

        public WallProjectionAndPositionSample (Point3D wallPosition, Point3D personPosition)
        {
            this.wallPositionX = wallPosition.X;
            this.wallPositionY = wallPosition.Y;
            this.wallPositionZ = wallPosition.Z;
            this.personPositionX = personPosition.X;
            this.personPositionY = personPosition.Y;
            this.personPositionZ = personPosition.Z;
        }

        public WallProjectionAndPositionSample()
        {
            this.wallPositionX = 0;
            this.wallPositionY = 0;
            this.wallPositionZ = 0;
            this.personPositionX = 0;
            this.personPositionY = 0;
            this.personPositionZ = 0;
        }
        public WallProjectionAndPositionSample(Point3D wallPosition, Point3D personPosition, string deviceIdentifier)
        {
            this.wallPositionX = wallPosition.X;
            this.wallPositionY = wallPosition.Y;
            this.wallPositionZ = wallPosition.Z;
            this.personPositionX = personPosition.X;
            this.personPositionY = personPosition.Y;
            this.personPositionZ = personPosition.Z;
            sampledeviceIdentifier = deviceIdentifier;
        }

        public WallProjectionAndPositionSample(WallProjectionSample wps, Point3D personPosition, string deviceIdentifier)
        {
            this.wallPositionX = wps.x;
            this.wallPositionY = wps.y;
            this.wallPositionZ = wps.z;
            this.personPositionX = personPosition.X;
            this.personPositionY = personPosition.Y;
            this.personPositionZ = personPosition.Z;
            sampledeviceIdentifier = deviceIdentifier;
        }

        public WallProjectionAndPositionSample(WallProjectionSample wps, Point3D personPosition)
        {
            this.wallPositionX = wps.x;
            this.wallPositionY = wps.y;
            this.wallPositionZ = wps.z;
            this.personPositionX = personPosition.X;
            this.personPositionY = personPosition.Y;
            this.personPositionZ = personPosition.Z;
            this.sampledeviceIdentifier = wps.sampledeviceIdentifier;
        }

    }
}
