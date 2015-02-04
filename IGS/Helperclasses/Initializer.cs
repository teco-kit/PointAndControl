using IGS.Server.Devices;
using IGS.Server.IGS;
using IGS.Server.Kinect;
using IGS.Server.WebServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;

namespace IGS.Helperclasses
{
    static class Initializer
    {
        

        /// <summary>
        /// Initializes the IGS with all and uses the other initialition methods to add the needed Dataholder, Usertracker and HTTP server.
        /// </summary>
        /// <returns>The igs with its needed components</returns>
        public static Igs InitializeIgs()
        {
            Igs igs = new Igs(InitializeDataholder(XMLComponentHandler.readDevices()), InitializeUserTracker(new HandsUp(), new Lfu()), InitializeHttpServer()) { Data = { LocalIp = IsLocalIpAddress("") } };
            return igs;
        }

        /// <summary>
        ///     Initializes the usertracker and activates the kienct.
        ///     Dabei wird aus der Konfigurationsdatei die zuletzt verwendete
        ///     Auswahlgeste und Ersetzungstrategie verwendet.
        ///     <param name='filter'>
        ///         The gesture a user makes to register to the gesture control. 
        ///     </param>
        ///     <param name='replace'>
        ///        The replacement strategy how bodys shall be replaced by another activation of the gesture control.
        ///     </param>
        ///     <returns>the initilized user tracker</returns>
        /// </summary>
        private static UserTracker InitializeUserTracker(GestureStrategy filter, ReplacementStrategy replace)
        {
            UserTracker userTracker = new UserTracker(filter, replace, true);
            userTracker.InitializeSensor();
            return userTracker;
        }

        /// <summary>
        /// Initilizes the Http server and starts the listening thread.
        /// </summary>
        /// <returns>the initialized http server</returns>
        private static HttpServer InitializeHttpServer()
        {
            HttpServer httpServer = new MyHttpServer(8080, IsLocalIpAddress(""));
            Thread thread = new Thread(httpServer.Listen);
            thread.Start();
            return httpServer;
        }

        private static IPAddress IsLocalIpAddress(String host)
        {
            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIp in hostIPs)
                {
                    if (hostIp.AddressFamily == AddressFamily.InterNetwork) // filter out ipv4
                    {
                        // is localhost
                        if (IPAddress.IsLoopback(hostIp)) return hostIp;
                        // is local address
                        foreach (IPAddress localIp in localIPs)
                        {
                            if (hostIp.Equals(localIp))
                            {
                                Debug.WriteLine(localIp.ToString());

                                return localIp;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            return null;
        }

        /// <summary>
        ///     Initializes the dataholder and adds the given device list.
        ///     <param name='devices'>
        ///         The list of the available devices at start of the application
        ///     </param>
        ///     <returns> the initialized dataholder with its devices</returns>
        /// </summary>
  
        private static DataHolder InitializeDataholder(List<Device> devices)
        {
            
            DataHolder data = new DataHolder(devices);
            data.Change_PlugWise_Adress(readAndBuildPlugAdressXML());
            
            return data;
        }


        /// <summary>
        /// Gets the components of the plugwise adress and combines them to the final adress string
        /// </summary>
        /// <returns>the final adress string</returns>
        private static string readAndBuildPlugAdressXML()
        {
            String finalString = "";
            String[] components = XMLComponentHandler.readPlugwiseComponents();
            finalString = "http://" + components[0] + ":" + components[1] + "/" + components[2] + "/";
            return finalString;
        }
        /// <summary>
        /// creates the basic structure of the configuration.xml file and saves it in the base directory.
        /// </summary>
        public static void createXMLFile()
        {

            XElement rootElement = new XElement("config",
                new XElement("deviceConfiguration"),
                new XElement("environment",
                    new XElement("Roomsize",
                        new XElement("width", "0.0"),
                        new XElement("height", "0.0"),
                        new XElement("depth", "0.0")),
                    new XElement("KinectConfiguration",
                        new XElement("X", "0.0"),
                        new XElement("Y", "0.0"),
                        new XElement("Z", "0.0"),
                        new XElement("tiltingAngle", "0.0"),
                        new XElement("HorizontalOrientationAngle", "0.0")),
                    new XElement("PlugwiseAdress",
                        new XElement("host"),
                        new XElement("port"),
                        new XElement("path")))
                        );

            rootElement.Save(AppDomain.CurrentDomain.BaseDirectory + "\\configuration.xml");
        }

        public static void createWallProjectionSampleXMLFile()
        {
            XElement root = new XElement("devices");
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml");
        }

        public static void createWallProjectionAndPositionSampleXMLFile()
        {
            XElement root = new XElement("devices");
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml");
        }

        public static void createSampleXMLFIle()
        {
            XElement root = new XElement("devices");
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "samples.xml");
        }

        public static void createLogFilePerSelect()
        {
            XElement root = new XElement("data");
            root.Add(new XAttribute("Selects", "0"));
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "BA_REICHE_LogFilePerSelect.xml");
        }

        public static void createLogFilePerSelectSmoothed()
        {
            XElement root = new XElement("data");
            root.Add(new XAttribute("Selects", "0"));
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "BA_REICHE_LogFilePerSelectSmoothed.xml");
        }

        public static void createGeneralLogFile()
        {
            XElement root = new XElement("log");
            root.Save(AppDomain.CurrentDomain.BaseDirectory + "program_log.xml");
        }
       

    }
}
