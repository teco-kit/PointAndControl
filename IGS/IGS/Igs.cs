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
using IGS.Classifier;
using Microsoft.Kinect;
using System.Threading;



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
            this.classification = new ClassificationHandler(Transformer, Data);
            this.coreMethods = new CollisionMethod(Data, Tracker, Transformer);


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

        ICoreMethods coreMethods { get; set; }




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
                user.AddError("Sie haben den Raum verlassen");
                user.TrackingState = false;
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
        public int SkeletonIdToUser(String wlanAdr)
        {
            User tempUser = Data.GetUserByIp(wlanAdr);
            int id = -1;

            if (tempUser != null)
            {
                id = Tracker.GetSkeletonId(tempUser.SkeletonId);

                if (id >= 0)
                {
                    tempUser.TrackingState = true;
                    Data.SetTrackedSkeleton(wlanAdr, id);
                }
            }

            return id;
        }

        /// <summary>
        ///     Passes the command with the provided ID on to the device.
        ///     <param name="sender">The object which triggered the event.</param>
        ///     <param name="args">Parameter needed for the interpretation.</param>
        /// </summary>
        public String InterpretCommand(object sender, HttpEventArgs args)
        {
            String devId = args.Dev;
            String cmd = args.Cmd;
            String value = args.Val;
            String[] parameters = value.Split(':');
            String wlanAdr = args.ClientIp;

            User user = Data.GetUserByIp(wlanAdr);
            Device device = null;
            String retStr = "";
            String msg = "";
            Boolean success = false;

            if (cmd != "popup")
                XMLComponentHandler.writeLogEntry("Command arrived! devID: " + devId + " cmdID: " + cmd + " value: " + value + " wlanAdr: " + wlanAdr);

            if (devId == "server")
            {
                // return JSON formatted message
                args.P.WriteSuccess("application/json");
                retStr = "{\"cmd\":\"" + cmd + "\"";

                if (cmd != "addUser" && cmd != "popup")
                {
                    // notify online learner that no control command was sent
                    // onlineNoSucces(devId, wlanAdr);
                }

                switch (cmd)
                {
                    case "addUser":
                        success = AddUser(wlanAdr);
                        break;

                    case "close":
                        success = DelUser(wlanAdr);
                        break;

                    case "activateGestureCtrl":
                        if (!Tracker.kinectAvailable)
                        {
                            msg = "Keine Kinect am System angeschlossen.";
                            break;
                        }

                        if (user != null)
                        {
                            int id = SkeletonIdToUser(wlanAdr);

                            if (id >= 0)
                                success = true;
                            else if (id == UserTracker.NO_GESTURE_FOUND)
                                msg = "Kein Nutzer mit Geste identifiziert.";
                            else if (id == UserTracker.NO_BODIES_IN_FRAME)
                                msg = "Keine Nutzer im Bild.";

                            // attach tracking state
                            retStr += ",\"trackingId\":" + id;
                            break;
                        }

                        if (!success)
                            msg = "Aktivierung der Gestenerkennung fehlgeschlagen.";

                        break;

                    case "selectDevice":
                        if (!Tracker.kinectAvailable)
                        {
                            msg = "Keine Kinect am System angeschlossen.";
                            break;
                        }

                        if (user == null || !user.TrackingState)
                        {
                            msg = "Bitte erst registrieren";
                            break;
                        }

                        success = true;
                        retStr += "," + MakeDeviceString(coreMethods.chooseDevice(user));
                        break;

                        
                    case "list":
                        success = true;
                        retStr += "," + MakeDeviceString(Data.Devices);
                        break;

                    case "discoverDevices":
                        success = true;
                        // Data.newDevices = discoverDevices();
                        retStr += "," + MakeDeviceString(Data.newDevices);
                        break;

                    case "addDevice":
                        if (parameters.Length == 4)
                        {
                            success = true;
                            msg = AddDevice(parameters[0], parameters[1], parameters[2], parameters[3]);
                            break;
                        }

                        msg = "Hinzufügen fehlgeschlagen. Falsche Anzahl von Parametern";

                        break;

                    case "addDeviceFromList":
                        // find device in newDevices list
                        device = Data.newDevices.Find(d => d.Id.Equals(parameters[0]));

                        if (device != null)
                        {
                            success = true;

                            String[] type = device.Id.Split('_');
                            String newDeviceId = AddDevice(type[0], parameters[1], device.address, device.port);

                            // attach to return string
                            retStr += ",\"deviceId\":\"" + newDeviceId + "\"";

                            // remove from new devices list
                            Data.newDevices.Remove(device);
                        }

                        break;

                    case "resetDeviceVectorList":
                        device = Data.getDeviceByID(parameters[0]);

                        if (device == null)
                        {
                            msg = "Kein Gerät gefunden";
                            break;
                        }

                        success = true;
                        device.PositionVectors = new List<Vector3D[]>();

                        //attach vector numbers
                        retStr += ",\"vectorCount\":" + device.PositionVectors.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();
                        break;

                    case "addDeviceVector":
                        device = Data.getDeviceByID(parameters[0]);

                        if (device == null)
                        {
                            msg = "Kein Gerät gefunden";
                            break;
                        }

                        if (user == null || !user.TrackingState)
                        {
                            msg = "Bitte erst registrieren";
                            break;
                        }

                        success = true;
                        msg = addDeviceVector(device, user);

                        //attach vector numbers 
                        retStr += ",\"vectorCount\":" + device.PositionVectors.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();
                        break;
                     
                    case "setDevicePosition":
                        device = Data.getDeviceByID(parameters[0]);

                        if (device == null)
                        {
                            msg = "Kein Gerät gefunden";
                            break;
                        }

                        success = true;
                        msg = coreMethods.train(device);
                        break;

                    case "popup":
                        if (user != null)
                        {
                            success = true;
                            msg = user.Errors;
                            user.ClearErrors();

                            // attach tracking state
                            retStr += ",\"trackingId\":" + user.SkeletonId;
                        }
                        break;
                }

                // finalize JSON response
                retStr += ",\"success\":" + success.ToString().ToLower() + ",\"msg\":\"" + msg + "\"}";
                Console.WriteLine(retStr);

                if (cmd != "popup" || msg != "")
                {
                    XMLComponentHandler.writeLogEntry("Response to '" + cmd + "': " + retStr);
                }

                return retStr;

            }
            else if (Data.getDeviceByID(devId) != null && cmd != null)
            {
                switch (cmd)
                {
                    case "getControlPath":
                        //onlineNoSucces(devId, wlanAdr);
                        retStr = getControlPagePathHttp(devId);
                        // redirect to device control path
                        args.P.WriteRedirect(retStr);
                        break;

                    default:
                        // assumes that correct device was selected
                        // executeOnlineLearning(devId, wlanAdr);
                        retStr = Data.getDeviceByID(devId).Transmit(cmd, value);

                        break;
                }

                XMLComponentHandler.writeLogEntry("Response to '" + cmd + "': " + retStr);
                return retStr;
            }
            else
            {
                // TODO: JSON response
                retStr = "Unbekannter Befehl.";
                return retStr;
            }
        }


        private String MakeDeviceString(IEnumerable<Device> devices)
        {
            String result = "\"devices\":[";

            if (devices != null)
            {
                Device[] deviceList = devices.ToArray<Device>();
                for (int i = 0; i < deviceList.Length; i++)
                {
                    if (i != 0)
                        result += ",";
                    result += "{\"id\":\"" + deviceList[i].Id + "\", \"name\":\"" + deviceList[i].Name + "\"}";
                }
            }
            result += "]";
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
        public String AddDevice(String type, String name, String address, String port)
        {
            String retStr = "";

            int count = 1;
            for (int i = 0; i < Data.Devices.Count; i++)
            {
                String[] devId = Data.Devices[i].Id.Split('_');
                if (devId[0] == type)
                    count++;
            }
            string idparam = type + "_" + count;

            // TODO: for testing we do not wand to add the device to XML
            // XMLComponentHandler.addDeviceToXML(parameter, count);

            Type typeObject = Type.GetType("IGS.Server.Devices." + type);
            if (typeObject != null)
            {
                object instance = Activator.CreateInstance(typeObject, name, idparam, new List<Ball>(),
                                                           address, port);
                Data.Devices.Add((Device)instance);
                retStr = idparam;

                Console.WriteLine(retStr);
                return retStr;
            }

            return retStr;
        }


        /// <summary>
        /// this method intializes the representation of the kinect camera used for positioning and 
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


        public String addDeviceVector(Device dev, User user)
        {
            if (dev == null)
                return "Gerät ist unbekannt.";

            if (Tracker.Bodies.Count == 0)
                return "Keine Personen von der Kinect gefunden";

            if (user.SkeletonId < 0)
                return "Bitte erst registrieren";

            Vector3D[] vectors = Transformer.transformJointCoords(Tracker.GetCoordinates(user.SkeletonId));
            dev.PositionVectors.Add(vectors);

            return dev.PositionVectors.Count + " von " + coreMethods.getMinVectorsPerDevice() + " Positionen hinzugefügt";

        }

        public String collectSample(Device dev, String wlanAddr)
        {
            
            User tmpUser = Data.GetUserByIp(wlanAddr);

            if (dev != null)
            {
                if (Tracker.Bodies.Count == 0)
                {
                    return "Keine Personen von der Kinect Kamera gefunden";
                }

                //Vector3D[] vectors = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(tmpUser.SkeletonId));
                Vector3D[] vectors = Transformer.transformJointCoords(Tracker.GetCoordinates(tmpUser.SkeletonId));

                if (classification.calculateWallProjectionSampleAndLearn(vectors, dev.Id) != "Es ist ein Fehler beim Erstellen des Samples aufgetreten, bitte versuchen sie es erneut!")
                {
                    //XMLComponentHandler.writeUserJointsToXmlFile(tmpUser, dev, body);
                    //XMLComponentHandler.writeUserJointsPerSelectClick(body);
                    XMLSkeletonJointRecords.writeClassifiedDeviceToLastSelect(dev);
                    return "Sample hinzugefügt.";
                }
                else
                {
                    XMLSkeletonJointRecords.deleteLastUserSkeletonSelected();
                    return "Keine Wand getroffen.";
                }
            }
            return "Gerät ist unbekannt.";
        }

        public String getControlPagePathHttp(String id)
        {
            String controlPath = "";

            Type t = Data.getDeviceByID(id).GetType();

            controlPath = "http://" + Server.LocalIP + ":8080" + "/" + t.Name + "/" + "index.html?dev=" + id;

            return controlPath;
        }
    }

}