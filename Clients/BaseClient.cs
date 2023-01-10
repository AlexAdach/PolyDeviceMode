using System;
/*using System.Collections.Generic;
using System.Linq;*/
using System.Net;
using System.Net.Http;
/*using System.Text;*/
using System.Threading;
using System.Threading.Tasks;

namespace PolyDeviceMode
{
    public abstract class BaseClient : IDisposable
    {

        public delegate void PropertyChangeDelegate();
        public static event PropertyChangeDelegate PropChange;

        //HTTP CLient and handler + locker
        private static object _locker = new object();
        private static volatile HttpClient _client;
        private static volatile HttpClientHandler _handler;
        private static bool _sessionActive = false;
        private static bool _deviceModeIs = false;
        private static bool _isCommunicating = false;
        private static string _sessionId;



        //Properties
        public static bool IsCommunicating
        {
            get
            {
                return _isCommunicating;
            }
            set
            {
                if (_isCommunicating != value)
                {
                    _isCommunicating = value;
                    OnProp();
                }
            }
        }


        
        public static bool SessionActive
        {
            get
            {
                return _sessionActive;
            }

            set
            {
                if(_sessionActive != value)
                {
                    _sessionActive = value;
                    OnProp();
                }
            }
        }

        //Session ID Property
        public static string SessionId
        {
            get
            {
                return _sessionId;
            }
            set
            {
                if (SessionId != value && value != null)
                {
                    _sessionId = value;
                    Debug.Toolbox("New Session ID aquired");
                    SessionActive = true;
                               
                }
            }
        }

        public static bool DeviceModeIs
        {
            get
            {
                return _deviceModeIs;
            }
            set
            {
               if(_deviceModeIs != value)
                {
                    _deviceModeIs = value;
                    OnProp();
                }
                
            }
        }

        protected static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (_locker)
                    {
                        if (_client == null)
                        {
                            _handler = new HttpClientHandler();
                            _handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                            _client = new HttpClient(_handler);
                           
                            Debug.Toolbox("Creating HTTP Client");
                            
                        }
                    }
                }

                return _client;
            }
        }

        protected async Task<string> SendRequest(HttpRequestMessage req)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.CancelAfter(5000);

            Task<HttpResponseMessage> reqT = Client.SendAsync(req, token);

            try
            {

                await reqT;
                if (await ValidateResponse(reqT.Result))
                {
                    IsCommunicating = true;
                    return await reqT.Result.Content.ReadAsStringAsync();
                }
                else
                {
                    IsCommunicating = false;
                    SessionActive = false;
                    return "";
                }
                
            }
            catch (OperationCanceledException)
            {
                Debug.Toolbox("Request Timed Out");
                SessionActive = false;

            }
            catch (WebException e)
            {
                Debug.Toolbox($"Web Exception {e.Message}");
                SessionActive = false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending request: {e.Message}");
                SessionActive = false;
            }
            return "";

        }

        public async Task<bool> ValidateResponse(HttpResponseMessage res)
        {
            string resString;

            //Check to make sure it's actually JSON in the response
            if(res.Content.Headers.ContentType.MediaType != "application/json")
            {
                Debug.Toolbox("Response is not JSON");
                return false;
            }

            //If it is JSON, read the contents of the response.
            try
            {
                var temp = await res.Content.ReadAsStringAsync();
                resString = temp;
            }
            catch (Exception e)
            {
                Debug.Toolbox($"Error reading response content: {e.Message}");
                return false;
            }
            
            //Check for successful code
            if (res.IsSuccessStatusCode)
            {
                Debug.Toolbox("Good Response.");
                return true;
            }
            //if unsuccessful, check to see if you're already logged in.
            else if(!res.IsSuccessStatusCode && resString.Contains("SessionAlreadyActive"))
            {
                Debug.Toolbox("Session Already Active");
                return true;
            }
            else
            {
                Debug.Toolbox("Bad Response");
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    _client.Dispose();
                }

                _client = null;
            }
        }

        protected static void OnProp()
        {
            if (PropChange != null)
            {
                PropChange();
            }
        }
    }
}
