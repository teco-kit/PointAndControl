using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using IGS.Server.WebServer;
using IGS.Server.Devices;
using IGS.Server.Kinect;
using System.Diagnostics;
using System.Threading;
using IGS.ComponentHandling;
using IGS.Helperclasses;
using Newtonsoft.Json;

namespace IGS.Server.IGS
{

    public class dev
    {
        public String id { get; set; }
        public String name { get; set; }
        public dev (Device d)
        {
            id = d.Id;
            name = d.Name;
        }
    }
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
            json_paramReader = new JSON_ParameterReader();
            this.Transformer = new CoordTransform(IGSKinect.tiltingDegree, IGSKinect.roomOrientation, IGSKinect.ball.Center);
            
            
            logger = eventLogger;
            this.coreMethods = new CollisionMethod(Data, Tracker, Transformer, logger);
            isRunning = true;
            
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

 

        ICoreMethods coreMethods { get; set; }

        public EventLogger logger { get; set; }

        public EnvironmentInfoHandler environmentHandler { get; set; }

        public bool isRunning { get; set; }

        public bool cancellationRequest { get; set; }

        private JSON_ParameterReader json_paramReader { get; set; }



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

            if (cancellationRequest)
                shutDown();
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

        public string AddUser(String wlanAdr, out bool success)
        {
            return Data.AddUser(wlanAdr, out success);
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
            Dictionary<String, String> parameters = json_paramReader.deserializeValueDict(value);
            JsonResponse response = new JsonResponse();
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

                response.addCmd(cmd);

                switch (cmd)
                {
                    case "addUser":
                        //success = AddUser(wlanAdr);
                        AddUser(wlanAdr, out success);
                        if (Data.GetUserByIp(wlanAdr) != null)
                        {
                            if (Tracker.isKinectAvailable())
                            {
                                response.addTrackingId(SkeletonIdToUser(wlanAdr));

                                //retStr += ",\"trackingId\":" + SkeletonIdToUser(wlanAdr);
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
                            //retStr += ",\"trackingId\":" + id;
                            response.addTrackingId(id);
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
                        //retStr += "," + MakeDeviceString(foundDevices);
                        response.addDevices(MakeDeviceString(foundDevices));
                        break;

                        
                    case "list":
                        success = true;
                        //retStr += "," + MakeDeviceString(Data.Devices);
                        response.addDevices(MakeDeviceString(Data.Devices));
                        break;

                    case "discoverDevices":
                        success = true;
                        // Data.newDevices = discoverDevices();
                        //retStr += "," + MakeDeviceString(Data.newDevices);
                        response.addDevices(MakeDeviceString(Data.newDevices));
                        break;

                    case "addDevice":
                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        msg = AddDevice(parameters);
                        success = true;

                        break;

                    case "addDeviceFromList":
                        // find device in newDevices list
                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if(!json_paramReader.getDevID(parameters, out paramDevId))
                        {
                            msg = paramDevId;
                            break;
                        }

                        device = Data.newDevices.Find(d => d.Id.Equals(paramDevId));

                        if (device != null)
                        {
                            success = true;
                            if (!json_paramReader.getDevName(parameters, out paramDevName))
                            {
                                msg = paramDevName;
                                break;
                            }
                            String type = DataHolder.getDeviceType(device);

                            String newDeviceId = AddDevice(type, "", paramDevName, device.Path);

                            // attach to return string
                            //retStr += ",\"deviceId\":\"" + newDeviceId + "\"";

                            
                            
                            // remove from new devices list
                            Data.newDevices.Remove(device);
                            response.addDeviceId(newDeviceId);
                        }

                        break;

                    case "resetDeviceVectorList":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if (!json_paramReader.getDevID(parameters, out paramDevId))
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
                        //retStr += ",\"vectorCount\":" + device.skelPositions.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();

                        response.addVectorMinAndCount(device.skelPositions.Count, coreMethods.getMinVectorsPerDevice());
                        break;

                    case "addDeviceVector":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if (!json_paramReader.getDevID(parameters, out paramDevId))
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
                        //retStr += ",\"vectorCount\":" + device.skelPositions.Count + ",\"vectorMin\":" + coreMethods.getMinVectorsPerDevice();
                        response.addVectorMinAndCount(device.skelPositions.Count, coreMethods.getMinVectorsPerDevice());
                        break;
                     
                    case "setDevicePosition":

                        if (parameters == null)
                        {
                            msg = Properties.Resources.NoValues;
                            break;
                        }

                        if(!json_paramReader.getDevID(parameters, out paramDevId))
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

                        if (!json_paramReader.getDevID(parameters, out paramDevId))
                        {
                            msg = Properties.Resources.DevNotFoundDeletion;
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
                            //retStr += ",\"trackingId\":" + user.SkeletonId;
                            response.addTrackingId(user.SkeletonId);
                        }
                        break;
                    case "setPlugwisePath":
                        success = setPlugwiseComponents(parameters);
                        if (success)
                        {
                            msg = Properties.Resources.PWCompChange;
                        } else
                        {
                            msg = Properties.Resources.UnknownError;
                        }
                        break;
                    case "setKinectComponents":
                        success = setKinectPositionWithDict(parameters);
                        if (success)
                        {
                            msg = Properties.Resources.KinectPlacementChanged;
                        } else
                        {
                            msg = "";
                        }
                        break;
                    case "setRoomMeasures":
                        success = setRoomMeasures(parameters);
                        if (success)
                        {
                            msg = Properties.Resources.RoommeasuresChanged;
                        } else
                        {
                            msg = "";
                        }
                        break;

                    case "shutDown":
                        success = true;

                        msg = Properties.Resources.ServerShutDown;

                        break;



                }
                response.addSuccess(success);
                response.addMsg(msg);
                // finalize JSON response
                //retStr += ",\"success\":" + success.ToString().ToLower() + ",\"msg\":\"" + msg + "\"}";
                

                if ((cmd != "popup" || msg != "") && (cmd != "pollDevice"))
                {
                    logger.enqueueEntry(String.Format("Respronse to '{0}' : {1}", cmd, retStr));
                }

                //return retStr;
                return response.serialize();

            }
            else if (Data.getDeviceByID(devId) != null && cmd != null)
            {
                switch (cmd)
                {
                    case "getControlPath":

                        response.addReturnString(getControlPagePathHttp(devId));
                        //retStr = getControlPagePathHttp(devId);
                        // redirect to device control path
                        //args.P.WriteRedirect(retStr, 301);
                        args.P.WriteRedirect(response.getReturnString(), 301);
                        break;

                    case "deleteDevice":
                        //retStr = Data.deleteDevice(devId);
                        response.addReturnString(devId);
                        break;

                    default:
                        Device dev = Data.getDeviceByID(devId);
                        if (dev.connection != null)
                        {
                            //retStr = dev.Transmit(cmd, value);
                            response.addReturnString(dev.Transmit(cmd, value));
                        }
                        break;
                }

                logger.enqueueEntry(String.Format("Response to Request {0} : {1} ", cmd, retStr));
                //return retStr;
                return response.serialize();
            }
            else
            {
                retStr = Properties.Resources.UnknownError;
                logger.enqueueEntry(String.Format("Response to Request {0} : {1}", cmd, retStr));
                //return retStr;

                response.addReturnString(retStr);

                return response.serialize();


            }
            
        }


        private String MakeDeviceString(IEnumerable<Device> devices)
        {
            String result = "[";
            List<dev> d = new List<dev>();
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


            if (json_paramReader.getDevNameTypePath(values, out type, out name, out path))
            {
                if (json_paramReader.getDevID(values, out id) && !id.Equals(""))
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

            String t = DataHolder.getDeviceType(Data.getDeviceByID(id));

            if (t.Equals("ExternalDevice"))
            {
                controlPath = Data.getDeviceByID(id).Path;
            }
            else
            {
                controlPath = "http://" + Server.LocalIP + ":8080" + "/" + t + "/" + "index.html?dev=" + id;
            }

            return controlPath;
        }

        private bool setPlugwiseComponents(Dictionary<string, string> values)
        {
            String host;
            String port;
            String path;

            json_paramReader.getPlugwiseComponents(values, out host, out port, out path);

            Data.change_PlugWise_Adress(host, port, path);

            return true;
        }

        private bool setKinectPositionWithDict(Dictionary<String,String> values)
        {
            String x;
            String y;
            String z;
            String horizontal;
            String tilt;

            json_paramReader.getKinectPosition(values, out x, out y, out z, out horizontal, out tilt);

            return setKinect(x, y, z, horizontal, tilt);

           
        }

        public bool setKinect(String x, String y, String z, String horizontal, String tilt)
        {
            double parsedX; 
            double parsedY;
            double parsedZ;
            double parsedHorizontal;
            double parsedTilt;
            bool changed = false;


            if(Double.TryParse(x, out parsedX) &&
               Double.TryParse(y, out parsedY) &&
               Double.TryParse(z, out parsedZ))
            {
                Point3D newCenter = new Point3D(parsedX, parsedY, parsedZ);

                IGSKinect.setKinectCoords(newCenter);

                Transformer.transVector = (Vector3D)newCenter;
                Data._environmentHandler.setKinectCoordsOnly(parsedX, parsedY, parsedZ);
                changed = true;
            }

            if(Double.TryParse(horizontal, out parsedHorizontal))
            {
                IGSKinect.roomOrientation = parsedHorizontal;
                Data._environmentHandler.setKinectHorizontalAnlge(parsedHorizontal);
                changed = true;
            }

            if(Double.TryParse(tilt, out parsedTilt))
            {
                IGSKinect.tiltingDegree = parsedTilt;
                Data._environmentHandler.setKinectTiltAngle(parsedTilt);
                changed = true;
            }


            if (Tracker.Sensor != null && changed)
            {
                Transformer.calculateRotationMatrix(IGSKinect.tiltingDegree, IGSKinect.roomOrientation);
            }

            logger.enqueueEntry(String.Format("Placement of Kinect Changed| X:{0}, Y:{1}, Z:{2}, Horizontal:{3}, Tilt:{4}", x, y, z, horizontal, tilt));

            return changed;
        }

        public bool setRoomMeasures(Dictionary<String, String> values)
        {
            String width;
            String height;
            String depth;

            json_paramReader.getRoomMeasures(values, out width, out height, out depth);

            Data.changeRoomSizeRemote(width, height, depth);
            logger.enqueueEntry(String.Format("Roomsize Changed| width:{0}, height:{1}, depth:{2}", width, height, depth));

            return true;
        }

        public bool shutDown()
        {
            Tracker.ShutDown();
            isRunning = false;

            return true;
        }

    }

}