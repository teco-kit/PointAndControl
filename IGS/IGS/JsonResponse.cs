using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IGS.Server.IGS
{
    public class JsonResponse
    {
        const string CMD = "cmd";
        const string SUCCESS = "success";
        const string MSG = "msg";
        const string VEC_COUNT = "vectorCount";
        const string VEC_MIN = "vectorMin";
        const string DEVICE_ID = "deviceId";
        const string TRACKING_ID = "trackingId";
        const string RETURN_VALUE = "returnString";
        const string DEVICES = "deviceString";

        public Dictionary<String, String> container { get; }
        
        public JsonResponse(String setCmd, bool setSuccess, String setMsg)
        {
            container = new Dictionary<string, string>();
            container.Add(CMD, setCmd);
            container.Add(SUCCESS, setSuccess.ToString().ToLower());
            container.Add(MSG, setMsg);
        }

        public JsonResponse()
        {
            container = new Dictionary<string, string>();
        }

        public string serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            String serialized = "";

            
            serialized = JsonConvert.SerializeObject(container, Formatting.Indented, settings);
            
            return serialized;
        }

        public void addCmd(String cmd)
        {
            if(!container.ContainsKey(CMD))
                container.Add(CMD, cmd);
        }

        public void addSuccess(bool success)
        {
            if(!container.ContainsKey(SUCCESS))
                container.Add(SUCCESS, success.ToString().ToLower());
        }

        public void addMsg(String msg)
        {
            if(!container.ContainsKey(MSG))
                container.Add(MSG, msg);
        }



        private bool checkForBaseComponents()
        {
            if(container.ContainsKey(CMD) && container.ContainsKey(SUCCESS) && container.ContainsKey(MSG))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public void addVectorMinAndCount(int vecCount, int vecMin)
        {
            if(!container.ContainsKey(VEC_COUNT) && !container.ContainsKey(VEC_MIN))
            {
                addVectorCount(vecCount);
                addVectorMin(vecMin);
            }
        }

        public void addVectorCount(int vecCount)
        {
            if (!container.ContainsKey(VEC_COUNT))
                container.Add(VEC_COUNT, vecCount.ToString());
            
        }

        public void addVectorMin(int vecMin)
        {
            if (!container.ContainsKey(VEC_MIN))
                container.Add(VEC_MIN, vecMin.ToString());
            
        }
        public void addDeviceId(String deviceId)
        {
            if (!container.ContainsKey(DEVICE_ID))
                container.Add(DEVICE_ID, deviceId);
            
        }

        public void addTrackingId(int trackingId)
        {
            if (!container.ContainsKey(TRACKING_ID))
                container.Add(TRACKING_ID, trackingId.ToString());
            
            
        }

        public void addReturnString(String value)
        {
            if (!container.ContainsKey(RETURN_VALUE))
                container.Add(RETURN_VALUE, value);
        }

        public String getReturnString()
        {
            String retStr = "";
            container.TryGetValue(RETURN_VALUE, out retStr);

            return retStr;
        }        

        public void addDevices(String devices)
        {
            if (!container.ContainsKey(DEVICES))
                container.Add(DEVICES, devices);
        }

    }
}
