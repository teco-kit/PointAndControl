using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using IGS.Server.WebServer;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using System.Diagnostics;
using IGS.Classifier;
using System.Threading;
using IGS.ComponentHandling;
using Newtonsoft.Json;

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
        public Igs(DataHolder data, UserTracker tracker, HttpServer server, EventLogger eventLogger)
        {
            environmentHandler = new EnvironmentInfoHandler();
            Data = data;
            Tracker = tracker;
            Server = server;
            Server.postRequest += server_Post_Request;
            Server.Request += server_Request;
            Tracker.KinectEvents += UserLeft;
            Tracker.Strategy.TrackingStateEvents += SwitchTrackingState;


            createIGSKinect();

            this.Transformer = new CoordTransform(IGSKinect.tiltingDegree, IGSKinect.roomOrientation, IGSKinect.ball.Center);
            this.classification = new ClassificationHandler(Transformer, Data);
            this.coreMethods = new CollisionMethod(Data, Tracker, Transformer);
            logger = eventLogger;
        
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
        public DevKinect IGSKinect { get; set; }

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

        public EventLogger logger { get; set; }

        public EnvironmentInfoHandler environmentHandler { get; set; }




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
                user.AddError(Properties.Resources.RoomLeft);
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
        ///     This method fetches the id of the skeleton from the user currently perfoming the gesture to register.
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
            Dictionary<String, String> parameters = deserializeValueDict(value);

            String wlanAdr = args.ClientIp;
            String lang = args.Language;

            User user = Data.GetUserByIp(wlanAdr);
            Device device = null;
            String retStr = "";
            String msg = "";
            Boolean success = false;

            String paramDevId;
            String paramDevName;



            if (Thread.CurrentThread.CurrentCulture.Name != lang)
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            }

            if (cmd != "popup" && cmd != "pollDevice")
                logger.enqueueEntry(String.Format("Command arrived! devID: {0}; cmdID: {1}; value: {2}; wlanAdr: {3}", devId, cmd, value, wlanAdr));

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

                        if (Data.GetUserByIp(wlanAdr) != null)
                        {
                            if (Tracker.isKinectAvailable())
                            {
                                retStr += ",\"trackingId\":" + SkeletonIdToUser(wlanAdr);
                            } else
                            {
                                msg = Properties.Resources.NoKinAvailable;
                            }
                        }

                        break;

                    case "close":
                        success = DelUser(wlanAdr);
                        break;

                    case "activateGestureCtrl":
                        if (!Tracker.isKinectAvailable())
                        {
                            msg = Properties.Resources.NoKinAvailable;
                            break;
                        }

                        if (user != null)
                        {
                            int id = SkeletonIdToUser(wlanAdr);

                            if (id >= 0)
                                success = true;
                            else if (id == UserTracker.NO_GESTURE_FOUND)
                                msg = Properties.Resources.NoGestureFound;
                            else if (id == UserTracker.NO_BODIES_IN_FRAME)
                                msg = Properties.Resources.NoUserInImage;

                            // attach tracking state
                            retStr += ",\"trackingId\":" + id;
                            break;
                        }

                        if (!success)
                            msg = Properties.Resources.GesturecontrolError;

                        break;

                    case "pollDevice":
                    case "selectDevice":
                        if (!Tracker.isKinectAvailable())
                        {
                            msg = Properties.Resources.NoKinAvailable;
                            break;
                        }

                        if (user == null || !user.TrackingState)
                        {
                            msg = Properties.Resources.RegistrationRequest;
                            break;
                        }

                        List<Device> foundDevices = coreMethods.chooseDevice(user);
                        if (foundDevices.Count > 0)
                            success = true;
                        retStr += "," + MakeDeviceString(foundDevices);
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
                        if (parameters.Count == 4)
                        {
                            success = true;
                            
                            if(parameters == null)
                            {
                                msg = Properties.Resources.NoValues;
                                break;
                            }
                               

                            msg = AddDevice(parameters);

                            break;
                        }

                        msg = Properties.Resources.AddDeviceError;


                        break;

                    case "addDeviceFromList":
                        // find device in newDevices list
                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if(!getValueFromDict(parameters, "Id", out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.newDevices.Find(d => d.Id.Equals(paramDevId));

                        if (device != null)
                        {
                            success = true;
                            if (!getValueFromDict(parameters, "Name", out paramDevName))
                            {
                                msg = paramDevName;
                                break;
                            }
                            String type = DataHolder.getDeviceType(device);

                            String newDeviceId = AddDevice(type, "", paramDevName, device.Path);

                            // attach to return string
                            retStr += ",\"deviceId\":\"" + newDeviceId + "\"";

                            // remove from new devices list
                            Data.newDevices.Remove(device);
                        }

                        break;

                    case "resetDeviceVectorList":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if (!getValueFromDict(parameters, "Id", out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.getDeviceByID(paramDevId);

                        if (device == null)
                        {
                            msg = Properties.Resources.NoDevFound;
                            break;
                        }

                        success = true;
                        device.skelPositions = new List<Point3D[]>();

                        //attach vector numbers
                        retStr += ",\"vectorCount\":" + device.skelPositions.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();
                        break;

                    case "addDeviceVector":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if (!getValueFromDict(parameters, "Id", out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.getDeviceByID(paramDevId);

                        if (device == null)
                        {
                            msg = Properties.Resources.NoDevFound;
                            break;
                        }

                        if (user == null || !user.TrackingState)
                        {
                            msg = Properties.Resources.RegistrationRequest;
                            break;
                        }
                        
                        success = true;
                        msg = addDeviceVector(device, user);

                        //attach vector numbers 
                        retStr += ",\"vectorCount\":" + device.skelPositions.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();
                        break;
                     
                    case "setDevicePosition":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if(!getValueFromDict(parameters, "Id", out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.getDeviceByID(paramDevId);

                        if (device == null)
                        {
                            msg = Properties.Resources.NoDevFound;
                            break;
                        }

                        success = true;
                        msg = coreMethods.train(device);
                        break;

                    case "deleteDevice":

                        if (!getValueFromDict(parameters, "Id", out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.getDeviceByID(paramDevId);

                        if (device == null)
                        {
                            msg = Properties.Resources.DevNotFoundDeletion;
                            break;
                        }

                        msg = Data.deleteDevice(device.Id);
                        success = true;

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
                

                if ((cmd != "popup" || msg != "") && (cmd != "pollDevice"))
                {
                    logger.enqueueEntry(String.Format("Respronse to '{0}' : {1}", cmd, retStr));
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
                        args.P.WriteRedirect(retStr, 301);
                        break;

                    case "deleteDevice":
                        retStr = Data.deleteDevice(devId);
                        break;

                    default:
                        // assumes that correct device was selected
                        // executeOnlineLearning(devId, wlanAdr);
                        Device dev = Data.getDeviceByID(devId);
                        if (dev.connection != null)
                        {
                            retStr = dev.Transmit(cmd, value);
                        }
                        break;
                }

                logger.enqueueEntry(String.Format("Response to Request {0} : {1} ", cmd, retStr));
                return retStr;
            }
            else
            {
                // TODO: JSON response
                retStr = Properties.Resources.UnknownError;
                logger.enqueueEntry(String.Format("Response to Request {0} : {1}", cmd, retStr));
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
        /// Adds a new coordinates and radius for a specified device by reading the wrist position of the user 
        /// who wants to add them
        /// <param name="devId">the device to which the coordinate and radius should be added</param>
        /// <param name="wlanAdr">The wlan adress of the user who wants to add the coordinates and radius</param>
        /// <param name="radius">The radius specified by the user</param>
        /// <returns>A response string if the add process was successful</returns>
        /// </summary>
        private String AddDeviceCoord(String devId, String wlanAdr, String radius)
        {
            String ret = Properties.Resources.NoCoordAdded;

            double isDouble;
            if (!double.TryParse(radius, out isDouble) || String.IsNullOrEmpty(radius)) return ret += ",\n" + Properties.Resources.NoRadiusWrongFormat;


            if (Tracker.Bodies.Count != 0)
            {
                Point3D wrist = Transformer.transformJointCoords(Tracker.getMedianFilteredCoordinates(Data.GetUserByIp(wlanAdr).SkeletonId))[1];

               ret = Data.addDeviceCoordinates(devId, radius, wrist); 
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
        public String AddDevice(String type, String ID, String name, String path)
        {
            //TODO: make to correct format
            String retStr = "";

            string answer = Data.AddDevice(type,ID, name, path);

            if(answer != "")
            {
                retStr = answer;
            } 

            return retStr;
        }

        public String AddDevice(Dictionary<String, String> values)
        {
            String type;
            String name;
            String id;
            String path;
            String retStr = "";

            if (values.TryGetValue("Type", out type) &&
                values.TryGetValue("Name", out name) &&
                values.TryGetValue("Path", out path))
            {
                if (values.TryGetValue("Id", out id))
                {
                    retStr = Data.AddDevice(type, id, name, path);
                } else
                {
                    retStr = Data.AddDevice(type, "", name, path);
                }

            }
            else
            {
                retStr = Properties.Resources.AddDeviceError;
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

            Point3D kinectCenter = new Point3D(environmentHandler.getKinectPosX(), environmentHandler.getKinectPosY(),environmentHandler.getKinectPosZ());
            Ball kinectBall = new Ball(kinectCenter, ballRad);
            double roomOrientation = environmentHandler.getKinecHorizontalAngle();
            double tiltingDegree = environmentHandler.getKinectTiltAngle();

            
            IGSKinect = new DevKinect("DevKinect", kinectBall, tiltingDegree, roomOrientation);
        }


        public String addDeviceVector(Device dev, User user)
        {
            if (dev == null)
                return Properties.Resources.UnknownDev;
            
            if (Tracker.Bodies.Count == 0)
                return Properties.Resources.NoUserInImage;
            
            if (user.SkeletonId < 0)
                return Properties.Resources.RegistrationRequest;

            Point3D[] vectors = Transformer.transformJointCoords(Tracker.GetCoordinates(user.SkeletonId));
            dev.skelPositions.Add(vectors);

            return String.Format(Properties.Resources.AddDevVec, dev.skelPositions.Count, coreMethods.getMinVectorsPerDevice());

        }

        public String getControlPagePathHttp(String id)
        {
            String controlPath = "";

            Type t = Data.getDeviceByID(id).GetType();

            controlPath = "http://" + Server.LocalIP + ":8080" + "/" + t.Name + "/" + "index.html?dev=" + id;

            return controlPath;
        }

        private Dictionary<String, String> deserializeValueDict(String jsonString)
        {
            try {
                Dictionary<String, String> values = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonString);
                return values;
            } catch (Exception e)
            {
                return null;
            }
        }

        public bool getValueFromDict(Dictionary<String, String> values, String value, out String retVal)
        {
            if (values.TryGetValue(value, out retVal))
            {
                return true;
            }
            else
            {
                retVal = String.Format(Properties.Resources.WrongParameter, value);
                return false;
            }
        }    
    }

}