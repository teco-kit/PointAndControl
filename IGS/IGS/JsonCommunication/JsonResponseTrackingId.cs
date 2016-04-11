using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.IGS.JsonCommunication
{
    class JsonResponseTrackingId : JsonResponse
    {
        public JsonResponseTrackingId(String setCmd, bool setSuccess, String setMsg, int trackingId)
            :base(setCmd, setSuccess, setMsg)
        {
            addTrackingId(trackingId);
        }

        public JsonResponseTrackingId()
            : base()
        {

        }

        public JsonResponseTrackingId(int trackingId)
        {
            addTrackingId(trackingId);
        }

        public void addTrackingId(int trackingId)
        {
            container.Add("trackingId", trackingId.ToString());
        }

        public override bool checkForSpecializedComponents()
        {
            if (container.ContainsKey("trackingId"))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
