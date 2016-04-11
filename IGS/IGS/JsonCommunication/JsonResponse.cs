using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IGS.IGS.JsonCommunication
{
    abstract class JsonResponse
    {

        public Dictionary<String, String> container { get; }

        public JsonResponse(String setCmd, bool setSuccess, String setMsg)
        {
            container = new Dictionary<string, string>();
            container.Add("cmd", setCmd);
            container.Add("success", setSuccess.ToString().ToLower());
            container.Add("msg", setMsg);
        }

        public JsonResponse()
        {
            container = new Dictionary<string, string>();
        }

        private string serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            String serialized = "";

            if(checkForBaseComponents() && checkForSpecializedComponents())
            {
                serialized = JsonConvert.SerializeObject(container, Formatting.Indented, settings);
            }
            return serialized;
        }

        public void addCmd(String cmd)
        {
            container.Add("cmd", cmd);
        }

        public void addSuccess(String success)
        {
            container.Add("success", success);
        }

        public void addMsg(String msg)
        {
            container.Add("msg", msg);
        }

        private bool checkForBaseComponents()
        {
            if(container.ContainsKey("cmd") && container.ContainsKey("success") && container.ContainsKey("msg"))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public abstract bool checkForSpecializedComponents();
    }
}
