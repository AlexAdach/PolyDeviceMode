using Newtonsoft.Json.Linq;
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;*/
using System.Net.Http;
/*using System.Text;
using System.Threading;*/
using System.Threading.Tasks;

namespace PolyDeviceMode
{
    class DeviceClient : BaseClient
    {
        private const string devicePath = "/rest/system/mode/device";
        private readonly string _URL;

        public DeviceClient(string IP)
        {
            _URL = "https://" + IP + devicePath;
            
        }

        private HttpRequestMessage CreateGet()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _URL);
            req.Headers.Add("session_id", SessionId);
            return req;

        }
        private HttpRequestMessage CreatePost()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, _URL);
            req.Headers.Add("session_id", SessionId);
            return req;

        }
        private HttpRequestMessage CreateDelete()
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, _URL);
            req.Headers.Add("session_id", SessionId);
            return req;

        }

        public bool GetDeviceMode()
        {

            var getSession = CreateGet();
            Debug.Toolbox("Getting Device Mode Status");
            var Task = SendRequest(getSession);
            

            if (Task.Result != "")
            {
                return ParseDeviceResponse(Task.Result, "GET");

            }
            else
            {
                return false;
            }
        }

        public bool SetDeviceModeOn()
        {

            var getSession = CreatePost();
            Debug.Toolbox("Turning On Device Mode");
            var Task = SendRequest(getSession);
            Task.Wait();

            if (Task.Result != "")
            {
                return ParseDeviceResponse(Task.Result, "POST");
            }
            else
            {
                return false;
            }
        }
        public bool SetDeviceModeOff()
        {

            var getSession = CreateDelete();
            Debug.Toolbox("Turning Off Device Mode");
            var Task = SendRequest(getSession);
            Task.Wait();

            if (Task.Result != "")
            {
                return ParseDeviceResponse(Task.Result, "DELETE");
            }
            else
            {
                return false;
            }
        }

        private bool ParseDeviceResponse(string response, string type)
        {
            var json = JObject.Parse(response);
            

            if (json.SelectToken("result") != null) //sanity check. is there a good response.
            {
                var result = json.SelectToken("result").ToString();
                if(result == "true")
                {
                    DeviceModeIs = true;
                }
                else if(result == "false")
                {
                    DeviceModeIs = false;
                }
            }
            else if(json.SelectToken("success") != null)
            {
                var result = json.SelectToken("success").ToString();
                Debug.Toolbox(result);
                if (result.ToLower() == "true")
                {
                    DeviceModeIs = !DeviceModeIs;
                }
                else
                {
                    DeviceModeIs = false;
                }
            }
            return DeviceModeIs;

        }














    }
}
