using System;
/*using System.Text;
using System.Threading.Tasks;*/
using System.Text.Json;
/*using System.Net;
using System.Net.Http;*/
using Crestron.SimplSharp;
using Newtonsoft.Json;
/*using Newtonsoft.Json.Linq;
using System.Threading;*/


namespace PolyDeviceMode
{
    public delegate void InitializedDataHandler(ushort state);
    public delegate void FeedbackHandler(ushort isCommunicatingfb, ushort sessionActivefb, ushort deviceModefb);
    public class DeviceMode
    {
        private static string _ip;
        //private static bool initialized = false;
        private static SessionClient SessionClient;
        private static DeviceClient DeviceClient;

        //Delegates
        public InitializedDataHandler InitializedData { get; set; }
        public FeedbackHandler CrestronFeedback { get; set; }

        //Debugging
        public ushort debug_enable = 0;



        //Constructor
        public DeviceMode() 
        {
            SessionClient.PropChange += new BaseClient.PropertyChangeDelegate(UpdateValues);

        }

        //Initialize Function. 
        public void Initialize(string username, string password, string ipaddress)
        {
            Credentials credentials = new Credentials(username, password);
            _ip = ipaddress;

            Debug.Toolbox("Initializing Module");

            SessionClient = new SessionClient(credentials, "192.168.0.147");
            DeviceClient = new DeviceClient("192.168.0.147");

            //initialized = true;
            InitializedData(Convert.ToUInt16(1));
        }

        public void LoginOn()
        {
            SessionClient.SessionPollEnabled = true;
            SessionClient.SessionPoll();
        }
        public void LoginOff()
        {
            SessionClient.SessionPollEnabled = false;
        }

        public void ModeGet()
        {
            DeviceClient.GetDeviceMode();
        }

        public void ModeOn()
        {
            DeviceClient.SetDeviceModeOn();
        }
        public void ModeOff()
        {
            DeviceClient.SetDeviceModeOff();
        }

        public void UpdateValues()
        {
            ushort comms, session, device; 

            if(SessionClient.IsCommunicating)
            {
                comms = 1;
            }
            else
            {
                comms = 0;
            }

            if(SessionClient.SessionActive)
            {
                session = 1;
            }
            else
            {
                session = 0;
            }

            if(DeviceClient.DeviceModeIs)
            {
                device = 1;
            }
            else
            {
                device = 0;
            }

            CrestronFeedback(comms, session, device);

        }






    }


    public class Credentials
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public Credentials(string user, string password)
        {
            User = user;
            Password = password;
        }

        public string SerializeCredentials()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public static class Debug
    {
        private static ushort _enable = 1;

        public static void Toolbox(string message)
        {
            if (_enable == 1)
            {
                CrestronConsole.PrintLine($"Poly Device Mode --- {message}");
            }
        }
    }
}

