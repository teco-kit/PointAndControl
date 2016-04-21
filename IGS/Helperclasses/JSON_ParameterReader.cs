using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PointAndControl.Helperclasses
{
    class JSON_ParameterReader
    {
        public JSON_ParameterReader()
        {

        }

        public bool getPlugwiseComponents(Dictionary<String, String> values, out String host, out String port, out String path)
        {
            if (!getValueFromDict(values, "host", out host))
            {
                host = null;
            }

            if (!getValueFromDict(values, "port", out port))
            {
                port = null;
            }

            if (!getValueFromDict(values, "path", out path))
            {
                path = null;
            }

            return true;
        }

        public bool getKinectPosition(Dictionary<String, String> values, out String x, out String y, out String z, out String horizontal, out String tilt)
        {
            if (!getValueFromDict(values, "x", out x))
                x = "NotChanged";

            if (!getValueFromDict(values, "y", out y))
                y = "NotChanged";

            if (!getValueFromDict(values, "z", out z))
                z = "NotChanged";

            if (!getValueFromDict(values, "horizontal", out horizontal))
                horizontal = "NotChanged";

            if (!getValueFromDict(values, "tilt", out tilt))
                tilt = "NotChannged";

            return true;
        }

        public bool getRoomMeasures(Dictionary<String, String> values, out String width, out String height, out String depth)
        {

            if (!getValueFromDict(values, "width", out width))
                width = "NotChanged";

            if (!getValueFromDict(values, "height", out height))
                height = "NotChanged";

            if (!getValueFromDict(values, "depth", out depth))
                depth = "NotChanged";

            return true;
        }


        public bool getDevID(Dictionary<String, String> values, out String paramDevId)
        {
            return getValueFromDict(values, "id", out paramDevId);
        }

        public bool getDevName(Dictionary<String, String> values, out String paramDevName)
        {
            return getValueFromDict(values, "name", out paramDevName);
        }

        public bool getDevNameTypePath(Dictionary<String, String> values, out String type, out String name, out String path)
        {
            if(values.TryGetValue("type", out type) &&
                values.TryGetValue("name", out name) &&
                values.TryGetValue("path", out path))
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
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
