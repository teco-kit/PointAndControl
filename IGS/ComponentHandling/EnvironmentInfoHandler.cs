using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.ComponentHandling
{
    public class EnvironmentInfoHandler
    {

        public EnvironmentInfoHandler() { }

        public void writePWHost(string host)
        {
            Properties.Settings.Default.PWHost = host;
            Properties.Settings.Default.Save();
        }

        public void writePWPort(string port)
        {
            Properties.Settings.Default.PWPort = port;
            Properties.Settings.Default.Save();
            
        }

        public void writePWpath(string path)
        {
            Properties.Settings.Default.PWPath = path;
            Properties.Settings.Default.Save();
        }

        public void writePWcomponents(string host, string port, string path)
        {
            writePWHost(host);
            writePWPort(port);
            writePWpath(path);
  
        }

        public string getPWHost()
        {
            return Properties.Settings.Default.PWHost;
        }

        public string getPWPort()
        {
            return Properties.Settings.Default.PWPort;
        }

        public string getPWPath()
        {
            return Properties.Settings.Default.PWPath;
        }

        public string getPWAdress()
        {
            return buildPlugwiseString(getPWHost(), getPWPort(), getPWPath());
        }

        public double getRoomWidht()
        {
            return Properties.Settings.Default.RoomWidth;
        }

        public void setRoomWidth(double width)
        {
            Properties.Settings.Default.RoomWidth = width;
            Properties.Settings.Default.Save();
        }

        public double getRoomHeight()
        {
            return Properties.Settings.Default.RoomHeight;
        }

        public void setRoomHeight(double height)
        {
            Properties.Settings.Default.RoomHeight = height;
            Properties.Settings.Default.Save();
        }

        public double getRoomDepth()
        {
            return Properties.Settings.Default.RoomDepth;
        }

        public void setRoomDepth(double depth)
        {
            Properties.Settings.Default.RoomDepth = depth;
            Properties.Settings.Default.Save();
        }

        public void setRoomMeasures(double width, double height, double depth)
        {
            setRoomWidth(width);
            setRoomHeight(height);
            setRoomDepth(depth);
        }

        public double getKinectPosX()
        {
            return Properties.Settings.Default.KinectPosX;
        }

        public void setKinectPosX(double posX)
        {
            Properties.Settings.Default.KinectPosX = posX;
            Properties.Settings.Default.Save();
        }

        public double getKinectPosY()
        {
            return Properties.Settings.Default.KinectPosY;
        }

        public void setKinectPosY (double posY)
        {
            Properties.Settings.Default.KinectPosY = posY;
            Properties.Settings.Default.Save();
        }

        public double getKinectPosZ()
        {
            return Properties.Settings.Default.KinectPosZ;
        }

        public void setKinectPosZ(double posZ)
        {
            Properties.Settings.Default.KinectPosZ = posZ;
            Properties.Settings.Default.Save();
        }

        public double getKinectTiltAngle()
        {
            return Properties.Settings.Default.KinectTiltAngle;
        }

        public void setKinectTiltAngle(short tAngle)
        {
            Properties.Settings.Default.KinectTiltAngle = tAngle;
            Properties.Settings.Default.Save();
        }

        public double getKinecHorizontalAngle()
        {
            return Properties.Settings.Default.KinectHorizontalAngle;
        }
        
        public void setKinectHorizontalAnlge(short hAngle)
        {
            Properties.Settings.Default.KinectHorizontalAngle = hAngle;
            Properties.Settings.Default.Save();
        }

        public void setKinectPosZ(short hAngle)
        {
            Properties.Settings.Default.KinectHorizontalAngle = hAngle;
            Properties.Settings.Default.Save();
        }

        public void setCompleteKinect(double kinX, double kinY, double kinZ, short tAngle, short hAngle)
        {
            setKinectPosX(kinX);
            setKinectPosY(kinY);
            setKinectPosZ(kinZ);
            setKinectTiltAngle(tAngle);
            setKinectHorizontalAnlge(hAngle);
        }

       
        /// <summary>
        /// Builds the regular adresses for the plugwises with the passed components
        /// </summary>
        /// <param name="components">The components of the plugwise adressin ascending order: Host, Port, Path</param>
        /// <returns></returns>
        private string buildPlugwiseString(string host, string port, string path)
        {
            string PlugWiseAdress = "http://" + host + ":" + port + "/" + path + "/";

            return PlugWiseAdress;
        }




    }
}
