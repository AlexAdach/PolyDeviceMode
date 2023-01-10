/*using System;
using System.Collections.Generic;
using System.Linq;*/
using System.Text;
/*using System.Net;*/
using System.Net.Http;
using System.Threading.Tasks;
/*using Newtonsoft.Json;*/
using Newtonsoft.Json.Linq;
using System.Threading;

namespace PolyDeviceMode
{
    
    class SessionClient : BaseClient
    {
        //Fields
        private const string sessionPath = "/rest/session";
        private readonly string _URL;
        private static Credentials credentials;

        public bool SessionPollEnabled { get; set; }

        

        //Constructor
        public SessionClient(Credentials Credentials, string IP)
        {
            credentials = Credentials;

            _URL = "https://" + IP + sessionPath;
        }

        private HttpRequestMessage CreatePost()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, _URL);
            var credentialsContent = credentials.SerializeCredentials();
            req.Content = new StringContent(credentialsContent, Encoding.UTF8, "application/json");

            return req;

        }

        public void SessionPoll()
        {
            
            //Task requestTask = MakeRequest();

            Task tpoll = Task.Factory.StartNew(() =>
            {
                do
                {
                    var postSession = CreatePost();
                    Debug.Toolbox("Polling Device Session");
                    var Task = SendRequest(postSession);
                    Task.Wait();

                    if (Task.Result != "")
                    {
                        ParseSessionResponse(Task.Result);
                    }
                    Thread.Sleep(5000);
                    
                } while (SessionPollEnabled);
            }, TaskCreationOptions.LongRunning);
        }

        static void ParseSessionResponse(string response)
        {
            var json = JObject.Parse(response);

            if (json.SelectToken("session.sessionId") != null) //sanity check. is there a good response.
            {
                SessionId = json.SelectToken("session.sessionId").ToString();
            }
        }

        




    }
}
