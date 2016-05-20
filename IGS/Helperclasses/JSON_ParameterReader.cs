using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PointAndControl.Helperclasses
{
    class JSON_ParameterReader
    {
        const string HOST = "host";
        const string PORT = "port";
        const string PATH = "path";
        const string KINECT_X = "x";
        const string KINECT_Y = "y";
        const string KINECT_Z = "z";
        const string KINECT_HORIZONTAL = "horizontal";
        const string KINECT_TILT = "tilt";
        const string WIDTH = "width";
        const string HEIGHT = "height";
        const string DEPTH = "depth";
        const string DEV_ID = "id";
        const string DEV_TYPE = "type";
        const string DEV_NAME = "name"; 



        public JSON_ParameterReader() { }


        public bool getPlugwiseComponents(Dictionary<String, String> values, out String host, out String port, out String path)
        {
            if (!getValueFromDict(values, HOST, out host))
            {
                host = null;
            }

            if (!getValueFromDict(values, PORT, out port))
            {
                port = null;
            }

            if (!getValueFromDict(values, PATH, out path))
            {
                path = null;
            }

            return true;
        }

        public bool getKinectPosition(Dictionary<String, String> values, out String x, out String y, out String z, out String horizontal, out String tilt)
        {
            if (!getValueFromDict(values, KINECT_X, out x))
                x = "NotChanged";

            if (!getValueFromDict(values, KINECT_Y, out y))
                y = "NotChanged";

            if (!getValueFromDict(values, KINECT_Z, out z))
                z = "NotChanged";

            if (!getValueFromDict(values, KINECT_HORIZONTAL, out horizontal))
                horizontal = "NotChanged";

            if (!getValueFromDict(values, KINECT_TILT, out tilt))
                tilt = "NotChannged";

            return true;
        }

        public bool getRoomMeasures(Dictionary<String, String> values, out String width, out String height, out String depth)
        {

            if (!getValueFromDict(values, WIDTH, out width))
                width = "NotChanged";

            if (!getValueFromDict(values, HEIGHT, out height))
                height = "NotChanged";

            if (!getValueFromDict(values, DEPTH, out depth))
                depth = "NotChanged";

            return true;
        }


        public bool getDevID(Dictionary<String, String> values, out String paramDevId)
        {
            return getValueFromDict(values, DEV_ID, out paramDevId);
        }

        public bool getDevName(Dictionary<String, String> values, out String paramDevName)
        {
            return getValueFromDict(values, DEV_NAME, out paramDevName);
        }

        public bool getDevNameTypePath(Dictionary<String, String> values, out String type, out String name, out String path)
        {
            if(values.TryGetValue(DEV_TYPE, out type) &&
                values.TryGetValue(DEV_NAME, out name) &&
                values.TryGetValue(PATH, out path))
            {
                return true;
            } else
            {
                type = null;
                name = null;
                path = null;

                return false;
            }
        }



        private bool getValueFromDict(Dictionary<String, String> values, String value, out String retVal)
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

        public Dictionary<String, String> deserializeValueDict(String jsonString)
        {
            try
            {
                Dictionary<String, String> values = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonString);
                return values;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
