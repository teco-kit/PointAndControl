using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using IGS.Server.WebServer;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using System.Diagnostics;
using System.IO;
using IGS.Helperclasses;
using System.Net;
using System.Text;
using IGS.KNN;
using Microsoft.Kinect;
using System.Threading;
using IGS.IGS;

namespace IGS.Server.IGS
{
    /// <summary>
    ///     This class takes place of the design pattern fassade. It encapsulates the different subsystems and combines the different interfaces which can be called by the HttpServer.
    ///     The IGS is the central control unit and passes on the tasks.
    ///     Contains the observer for the HttpEvents as well as KinectEvents.
    ///     @author Sven Ochs, Frederik Reiche
    /// </summary>
    public class Igs
    {


        /// <summary>
        ///     Constructor for the IGS.
        ///     Among other things it creates a concrete observer for HttpEven and KinectEvent.
        ///     <param name="data">The Dataholder</param>
        ///     <param name="tracker">The Usertracker</param>
        ///     <param name="server">The HTTP server</param>
        /// </summary>
        public Igs(DataHolder data, UserTracker tracker, HttpServer server)
        {

            Data = data;
            Tracker = tracker;
            Server = server;
            Server.postRequest += server_Post_Request;
            Server.Request += server_Request;
            Tracker.KinectEvents += UserLeft;
            Tracker.Strategy.TrackingStateEvents += SwitchTrackingState;

            

            createIGSKinect();


            this.Transformer = new CoordTransform(IGSKinect.tiltingDegree, IGSKinect.roomOrientation, IGSKinect.ball.Centre);
            this.classification = new ClassificationHandler(Transformer);
            this.chooseDeviceMethod = new DevChooseMethodKNN(classification);
           
            
        }


        /// <summary>
        ///     With the "set"-method the DataHolder can be set.
        ///     With the "get"-method the DataHolder can be returned.
        /// </summary>
        public DataHolder Data { get; set; }


        /// <summary>
        ///     With the "set"-method the UserTracker can be set.
        ///     With the "get"-method the UserTracker can be returned.
        /// </summary>
        public UserTracker Tracker { get; set; }

        /// <summary>
        ///     With the "set"-method the HTTP-Server can be set.
        ///     With the "get"-method the HTTP-Server can be returned.
        /// </summary>
        public HttpServer Server { get; set; }
        /// <summary>
        ///     With the "set"-method the IGSKinect can be set.
        ///     With the "get"-method the IGSKinect can be returned.
        /// </summary>
        public devKinect IGSKinect { get; set; }

        /// <summary>
        /// Marks if the devices are initialized or not.
        /// With the "set"-method the devInit can be set.
        /// With the "get"-method the devInit can be returned.
        /// </summary>
        public bool devInit { get; set; }

        /// <summary>
        /// With the "set"-method the CoordTransform can be set.
        /// With the "get"-method the CoordTransform can be returned.
        /// </summary>
        public CoordTransform Transformer { get; set; }

        public ClassificationHandler classification { get; set; }

        public ChooseDeviceMethod chooseDeviceMethod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SwitchTrackingState(object sender, TrackingStateEventArgs args)
        {
            if (Data.GetUserBySkeleton(args.SkeletonId) != null)
            {
                Data.GetUserBySkeleton(args.SkeletonId).TrackingState = false;
            }
        }


        /// <summary>
        ///     Part of the design pattern: observer(HttpEvent).
        ///     Takes place for the update-method in the observer design pattern.
        /// </summary>


        private void server_Post_Request(object sender, HttpEventArgs e)
        {
            String str = "";
            Server.SendResponse(e.P, str);
        }
        private void server_Request(object sender, HttpEventArgs e)
        {
            Debug.WriteLine("server_Request");
            String str = InterpretCommand(sender, e);

            Server.SendResponse(e.P, str);
        }

        /// <summary>
        ///     Part of the design pattern: observer(KinectEvent).
        ///     Takes place for the update-method in the observer design pattern.
        ///     In the case that a user left the kinects field of view his skeleton ID in his user-object will be deleted and the gesture control deactivated.
        ///     The user will be notified with the next to the server.
        ///     <param name="sender">Object which triggered the event</param>
        ///     <param name="args">The KinectUserEvent with the information about the user</param>
        /// </summary>
        public void UserLeft(object sender, KinectUserEventArgs args)
        {
            User user = Data.GetUserBySkeleton(args.SkeletonId);
            if (user != null)
            {
                user.AddError("You left the room!");
            }
            Data.DelTrackedSkeleton(args.SkeletonId);
        }

        /// <summary>
        ///     This method adds a new user to the DataHolder with his registered wlan adress.
        ///     <param name="wlanAdr">Wlan adress of the new registered user</param>
        /// </summary>
        public bool AddUser(String wlanAdr)
        {
            return Data.AddUser(wlanAdr);
        }

        /// <summary>
        ///     This method fetches the id of the skeleton from the user currently perfoming the gesture to choose a device.
        ///     This id will be set in the UserObject which is through its WLAN-adress unique.
        ///     If this procedure is finished successfully, the gesture control is for the user active and can be used.
        ///     <param name="wlanAdr">WLAN-Adress of the user wanting to activate gesture control</param>
        /// </summary>
        public bool SkeletonIdToUser(String wlanAdr)
        {
            User tempUser = Data.GetUserByIp(wlanAdr);
            int id = -1;
            if (tempUser != null)
            {
                int sklId = tempUser.SkeletonId;

                id = Tracker.GetSkeletonId(sklId);

                if (id != -1)
                    tempUser.TrackingState = true;
            }

            return id >= 0 && Data.SetTrackedSkeleton(wlanAdr, id);
        }

        /// <summary>
        ///     Passes the command with the provided ID on to the device.
        ///     <param name="sender">The object which triggered the event.</param>
        ///     <param name="args">Parameter needed for the interpretation.</param>
        /// </summary>
        public String InterpretCommand(object sender, HttpEventArgs args)
        {
            String devId = args.Dev;
            String cmdId = args.Cmd;
            String value = args.Val;
            String wlanAdr = args.ClientIp;
            String retStr = "";
            if (cmdId != "popup")
                XMLComponentHandler.writeLogEntry("Command arrived! devID: " + devId + " cmdID: " + cmdId + " value: " + value + " wlanAdr: " + wlanAdr);



            if (devId == "server")
            {
                //if (cmdId != "addUser" && cmdId != "popup")
                //{
                //    onlineNotSucces(devId, wlanAdr);
                //}
                switch (cmdId)
                {
                    case "addUser":

                        retStr = AddUser(wlanAdr).ToString();
                        XMLComponentHandler.writeLogEntry("Response to 'addUser': " + retStr);
                        return retStr;

                    case "close":
                        retStr = DelUser(wlanAdr).ToString();
                        XMLComponentHandler.writeLogEntry("Response to 'close': " + retStr);
                        return retStr;

                    case "activateGestureCtrl":

                        if (Data.GetUserByIp(wlanAdr) != null)
                        {
                            retStr = SkeletonIdToUser(wlanAdr).ToString();
                            XMLComponentHandler.writeLogEntry("Response to 'activateGestureCtrl': " + retStr);
                            return retStr;

                        }

                        retStr = "Aktivierung nicht möglich.\nBitte starten Sie die App neu.";

                        return retStr;
                    case "selectDevice":
                        if (Data.GetUserByIp(wlanAdr).TrackingState)
                        {
                            retStr = MakeDeviceString(chooseDeviceMethod.chooseDevice(wlanAdr, Transformer, Tracker, Data));
                            
                            XMLComponentHandler.writeLogEntry("Response to 'selectDevice': " + retStr);
                            return retStr;
                        }
                        Server.SendResponse(args.P, "ungueltiger Befehl");
                        break;
                    case "list":

                        retStr = MakeDeviceString(Data.Devices);
                        XMLComponentHandler.writeLogEntry("Response to 'list': " + retStr);
                        return retStr;
                    case "addDevice":
                        string[] parameter = value.Split(':');
                        if (parameter.Length == 4)
                        {

                            retStr = AddDevice(parameter);


                            XMLComponentHandler.writeLogEntry("Response to 'addDevice': " + retStr);
                            return retStr;
                        }

                        retStr = "Kein Gerät hinzugefügt. Paramter Anzahl nicht Korrekt";

                        return retStr;
                    case "collectDeviceSample":
                        Console.WriteLine("collect kam an!" + "Value:" + value);
                        retStr = collectSample(wlanAdr, value);
                        XMLComponentHandler.writeLogEntry("Response to 'collectDeviceSample': " + retStr);

                        return retStr;

                    case "popup":
                        String msg = "";
                        if (Data.GetUserByIp(wlanAdr) != null)
                        {
                            msg = Data.GetUserByIp(wlanAdr).Errors;
                            Data.GetUserByIp(wlanAdr).ClearErrors();
                        }

                        retStr = msg;
                        if (msg != "")
                        {
                            XMLComponentHandler.writeLogEntry("Response to 'popup': " + retStr);
                        }
                        return retStr;
                }
            }
            else
            {
                //if (cmdId == "addDeviceCoord")
                //{

                //    retStr = AddDeviceCoord(devId, wlanAdr, value);
                //    XMLComponentHandler.writeLogEntry("Response to 'addDeviceCoord': " + retStr);

                //    return retStr;

                //}
                if (devId != null && cmdId == "getControlPath" && Data.getDeviceByID(devId) != null)
                {
                    //onlineNotSucces(devId, wlanAdr);
                    retStr = getControlPagePathHttp(devId);
                    XMLComponentHandler.writeLogEntry("Response to 'getControlPath': " + retStr);
                    return retStr;
                }
                else if (devId != null && cmdId != null && Data.getDeviceByID(devId) != null)
                {
                    //executeOnlineLearning(devId, wlanAdr);
                    retStr = Data.getDeviceByID(devId).Transmit(cmdId, value);
                    XMLComponentHandler.writeLogEntry("Response to 'control device': " + retStr);
                    return retStr;
                }

                Server.SendResponse(args.P, "ungueltiger Befehl");
            }
            return null;
        }


        private String MakeDeviceString(IEnumerable<Device> devices)
        {
            String result = "";
            //if (devices != null && devices.Count() == 1)
            //{
            //    foreach (Device d in devices)
            //    {
            //        return getControlPagePath(d.Id);
            //    }

            //}
            if (devices != null)
            {
                result = devices.Aggregate(result, (current, dev) => current + (dev.Id + "\t" + dev.Name + "\n"));
            }
            return result;
        }



        /// <summary>
        ///     This method deletes the user who closed his app.
        ///     <param name="wlanAdr">wlan adress of the user who closed his app</param>
        /// </summary>
        public bool DelUser(String wlanAdr)
        {
            return Data.DelUser(wlanAdr);
        }

        /// <summary>
        ///     Calculates the possible device the user want to choose per gesture control
        ///     <param name="wlanAdr">wlan adress of the user</param>
        ///     <returns>list with the possiible devices</returns>
        /// </summary>
        public List<Device> ChooseDevice(String wlanAdr)
        {
            List<Device> dev = new List<Device>();
            User tempUser = Data.GetUserByIp(wlanAdr);
            Vector3D[] vecs = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(tempUser.SkeletonId));
            //Vector3D[] vecs = Transformer.transformJointCoords(Tracker.GetCoordinates(tempUser.SkeletonId));
            if (tempUser != null)
            {

                WallProjectionSample sample = classification.collector.calculateWallProjectionSample(vecs, "");

                sample = classification.classify(sample);

                Console.WriteLine("Classified: " + sample.sampleDeviceName);
                XMLComponentHandler.writeLogEntry("Device classified to" + sample.sampleDeviceName);
                Body body = Tracker.GetBodyById(tempUser.SkeletonId);
                //XMLSkeletonJointRecords.writeUserJointsToXmlFile(tempUser, Data.GetDeviceByName(sample.sampleDeviceName), body);
                //XMLComponentHandler.writeUserJointsPerSelectClick(body);
                classification.deviceClassificationCount++;

                Device device = Data.GetDeviceByName(sample.sampleDeviceName);
                sample.sampleDeviceName = device.Name;



                if (sample != null)
                {
                    foreach (Device d in Data.Devices)
                    {
                        if (d.Name.ToLower() == sample.sampleDeviceName.ToLower())
                        {
                            //XMLComponentHandler.writeWallProjectionSampleToXML(sample);
                            Point3D p = new Point3D(vecs[2].X, vecs[2].Y, vecs[2].Z);
                            //XMLComponentHandler.writeWallProjectionAndPositionSampleToXML(new WallProjectionAndPositionSample(sample, p));
                            //XMLComponentHandler.writeSampleToXML(vecs, sample.sampleDeviceName);
                            //XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(d);
                            dev.Add(d);
                            //tempUser.lastChosenDeviceID = d.Id;
                            //tempUser.lastClassDevSample = sample;
                            //tempUser.deviceIDChecked = false;
                            return dev;
                        }
                    }
                }
            }
            return dev;
        }

        /// <summary>
        /// Adds a new coordinates and radius for a specified device by reading the right wrist position of the user 
        /// who wants to add them
        /// <param name="devId">the device to which the coordinate and radius should be added</param>
        /// <param name="wlanAdr">The wlan adress of the user who wants to add the coordinates and radius</param>
        /// <param name="radius">The radius specified by the user</param>
        /// <returns>A response string if the add process was successful</returns>
        /// </summary>
        private String AddDeviceCoord(String devId, String wlanAdr, String radius)
        {
            String ret = "keine Koordinaten hinzugefügt";

            double isDouble;
            if (!double.TryParse(radius, out isDouble) || String.IsNullOrEmpty(radius)) return ret += ",\nRadius fehlt oder hat falsches Format";


            if (Tracker.Bodies.Count != 0)
            {
                Vector3D rightWrist = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(Data.GetUserByIp(wlanAdr).SkeletonId))[3];
                Ball coord = new Ball(rightWrist, float.Parse(radius));
                Data.getDeviceByID(devId).Form.Add(coord);
                ret = XMLComponentHandler.addDeviceCoordToXML(devId, radius, coord);
            }

            return ret;
        }

        /// <summary>
        ///     Adds a new device to the device list and updates the deviceConfiguration part of the config.xml.
        ///     <param name="parameter">
        ///         Parameter of the device which should be added.
        ///         Parameter: Type, Name, Id, Form, Address
        ///     </param>
        ///     <returns>returns a response string what result the process had</returns>
        /// </summary>
        public String AddDevice(String[] parameter)
        {
            String retStr = "";

            int count = 1;
            for (int i = 0; i < Data.Devices.Count; i++)
            {
                String[] devId = Data.Devices[i].Id.Split('_');
                if (devId[0] == parameter[0])
                    count++;
            }
            string idparams = parameter[0] + "_" + count;


            XMLComponentHandler.addDeviceToXML(parameter, count);

            Type typeObject = Type.GetType("IGS.Server.Devices." + parameter[0]);
            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, parameter[1], idparams, new List<Ball>(),
                                                           parameter[2], parameter[3]);
                Data.Devices.Add((Device)instance);
                retStr = "Device added to deviceConfiguration.xml and devices list";

                Console.WriteLine(retStr);
                return retStr;
            }

            retStr = "Device added to deviceConfiguration but not to devices list";

            return retStr;
        }


        /// <summary>
        /// this method intiializes the representation of the kinect camera used for positioning and 
        /// visualization by reading the information out of the config.xml
        /// </summary>
        public void createIGSKinect()
        {
            float ballRad = 0.4f;

            String[] kinParamets = XMLComponentHandler.readKinectComponents();
            Vector3D kinectCenter = new Vector3D(double.Parse(kinParamets[0]), double.Parse(kinParamets[1]), double.Parse(kinParamets[2]));
            Ball kinectBall = new Ball(kinectCenter, ballRad);
            double roomOrientation = double.Parse(kinParamets[4]);
            double tiltingDegree = double.Parse(kinParamets[3]);


            IGSKinect = new devKinect("devKinect", kinectBall, tiltingDegree, roomOrientation);
        }

        public String collectSample(String wlan, String devID)
        {
          
            User tmpUser = Data.GetUserByIp(wlan);

            Device dev = Data.GetDeviceByName(devID);
            if (dev != null)
            {
                if (Tracker.Bodies.Count == 0)
                {
                    return "No bodys found by kinect";
                }
                
                Vector3D[] vectors = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(tmpUser.SkeletonId));
                //Vector3D[] vectors = Transformer.transformJointCoords(Tracker.GetCoordinates(tmpUser.SkeletonId));
             
                if (classification.calculateWallProjectionSampleAndLearn(vectors, dev.Name) == true)
                {
                    //XMLComponentHandler.writeUserJointsToXmlFile(tmpUser, dev, body);
                    //XMLComponentHandler.writeUserJointsPerSelectClick(body);
                    XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(dev);
                    return "sample added";
                }
                else
                {
                    XMLSkeletonJointRecords.deleteLastUserSkeletonSelected();
                    return "direction didn't hit a wall";
                }
            }
            return "Sample not added, deviceID not found";
        }

        public String getControlPagePathHttp(String id)
        {
            String controlPath = "";

            Type t = Data.getDeviceByID(id).GetType();

            controlPath = "Http://" + Server.LocalIP + ":8080" + "/" + t.Name + "/" + "index.html";

            return controlPath;
        }

        //public void executeOnlineLearning(String devId, String wLanAdr)
        //{
        //    User tmpUser = Data.GetUserByIp(wLanAdr);

        //    if (devId == tmpUser.lastChosenDeviceID)
        //    {
        //        if (tmpUser.deviceIDChecked == false && tmpUser.lastClassDevSample != null)
        //        {
        //            Device dev = Data.getDeviceByID(devId);
        //            classification.onlineLearn(tmpUser);
        //            XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(dev);
        //            XMLComponentHandler.writeLogEntry("Executed OnlineLearning: Result: Device was classified correctly");
        //            return;
        //        }
        //        return;
        //    }
        //    //onlineNotSucces(devId, wLanAdr);

        //}

        //public void onlineNotSucces(String devId, String wLanAdr)
        //{
        //    User tmpUser = Data.GetUserByIp(wLanAdr);


        //    if (tmpUser != null && tmpUser.deviceIDChecked == false && tmpUser.lastClassDevSample != null)
        //    {

        //        Device userDev = Data.getDeviceByID(tmpUser.lastChosenDeviceID);
        //        if (devId != userDev.Id)
        //        {
        //            classification.deviceClassificationErrorCount++;
        //            XMLSkeletonJointRecords.deleteLastUserSkeletonFromLogXML(userDev);
        //            XMLComponentHandler.deleteLastSampleFromSampleLogs(userDev);
        //            XMLSkeletonJointRecords.deleteLastUserSkeletonSelected();
        //            tmpUser.deviceIDChecked = true;
        //            tmpUser.lastClassDevSample = null;
        //            tmpUser.lastChosenDeviceID = "";


        //            Console.WriteLine("Wrong Device!");
        //            XMLComponentHandler.writeLogEntry("Executed OnlineLearning: Result: Device was classified wrong");
                    
        //            return;
        //        }
        //    }
        //}









    }
}