using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.IGS.JsonCommunication
{
    class JsonResponseAddVector : JsonResponse
    {
        public JsonResponseAddVector(String setCmd, bool setSuccess, String setMsg, int vecCount, int vecMin)
            :base(setCmd, setSuccess, setMsg)
        {
            addComponents(vecCount, vecMin);
        }

        public JsonResponseAddVector()
            :base()
        {

        }

        public JsonResponseAddVector(int vecCount, int vecMin)
            : base()
        {
            addComponents(vecCount, vecMin);
        }

        public void addComponents(int vecCount, int vecMin)
        {
            container.Add("vectorCount", vecCount.ToString());
            container.Add("vectorMin", vecMin.ToString());
        }

        public void addVectorCount(int vecCount)
        {
            container.Add("vectorCount", vecCount.ToString());
        }

        public void addVectorMin(int vecMin)
        {
            container.Add("vectorMin", vecMin.ToString());
        }

        public override bool checkForSpecializedComponents()
        {
            if(container.ContainsKey("vectorCount") && container.ContainsKey("vectorMin"))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
