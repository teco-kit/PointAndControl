using PointAndControl.ComponentHandling;
using PointAndControl.Kinect;
using PointAndControl.WebServer;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Linq;

namespace PointAndControl.MainComponents
{
    static class Initializer
    {


        /// <summary>
        /// Initializes the IGS with all and uses the other initialition methods to add the needed Dataholder, Usertracker and HTTP server.
        /// </summary>
        /// <returns>The pncMain with its needed components</returns>
        public static PointAndControlMain InitializeIgs()
        {
            xmlFilesControl();
            EventLogger logger = new EventLogger();
            PointAndControlMain igs = new PointAndControlMain(InitializeDataholder(logger), InitializeUserTracker(new HandsUp(), new Lfu()), InitializeHttpServer(),logger);
            igs.isRunning = true;
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
                            if (hostIp.Equals(localIp)) return localIp;
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

        private static DataHolder InitializeDataholder(EventLogger logger)
        {
            DataHolder data = new DataHolder(logger);
            return data;
        }

        /// <summary>
        /// Checks if basic configuration files are present and creates missing files
        /// </summary>
        private static void xmlFilesControl()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionSamples.xml"))
            {
                Initializer.createWallProjectionSampleXMLFile();
            }
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\samples.xml"))
            //{
            //    Initializer.createSampleXMLFIle();
            //}

            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\WallProjectionAndPositionSamples.xml"))
            //{
            //    Initializer.createWallProjectionAndPositionSampleXMLFile();
            //}
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelect.xml"))
            //{
            //    Initializer.createLogFilePerSelect();
            //}
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BA_REICHE_LogFilePerSelectSmoothed.xml"))
            //{
            //    Initializer.createLogFilePerSelectSmoothed();
            //}
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


    }
}
