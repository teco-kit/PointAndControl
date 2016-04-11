using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.IGS.JsonCommunication
{
    class JsonResponseDeviceId : JsonResponse
    {
        public JsonResponseDeviceId(String setCmd, bool setSuccess, String setMsg, String deviceId)
            :base(setCmd, setSuccess, setMsg)
        {
            addDeviceId(deviceId);
        }

        public JsonResponseDeviceId()
            :base()
        {

        }

        public JsonResponseDeviceId(String deviceId)
            :base()
        {
            addDeviceId(deviceId);
        }

        public void addDeviceId(String deviceId)
        {
            container.Add("deviceId", deviceId);
        }

        public override bool checkForSpecializedComponents()
        {
            if (container.ContainsKey("deviceId"))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
