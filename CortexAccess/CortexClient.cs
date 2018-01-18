using System;
using NUnit.Framework;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CortexAccess
{
    public class StreamDataEventArgs
    {
        public StreamDataEventArgs(int  streamType, JObject data, int requestType = 0)
        {
            StreamType = streamType;
            RequestType = requestType;
            Data = data;
        }
        public int RequestType { get; private set; }
        public int StreamType { get; private set; }
        public JObject Data { get; private set; }
    }
    public class MessageErrorEventArgs
    {
        public MessageErrorEventArgs(string method, int code, string messageError)
        {
            Method = method;
            Code = code;
            MessageError = messageError;
        }
        public int Code { get; set; }
        public string MessageError { get; set; }
        public string Method { get; set; }
    }

    // CortexClient class work with Cortex Service directly
    public sealed class CortexClient
    {
        const string Url = "wss://emotivcortex.com:54321";

        // Member variables
        private static readonly CortexClient instance = new CortexClient();

        private WebSocket _wSC; // Websocket Client
        private int _nextRequestId; // Unique id for each request
        private Dictionary<int, string> _methodForRequestId; // Map method with requestID
        private string _token;
        private bool _isWSConnected;

        private string m_CurrentMessage = string.Empty;
        //Events
        private AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        private AutoResetEvent m_CloseEvent = new AutoResetEvent(false);


        public event EventHandler<bool> OnConnected;
        public event EventHandler<MessageErrorEventArgs> OnMessageError;
        public event EventHandler<List<Headset>> OnQuerryHeadsetOK;
        public event EventHandler<List<string>> OnGetUserLoginOK;
        public event EventHandler<bool> OnLoginOK;
        public event EventHandler<bool> OnLogoutOK;
        public event EventHandler<string> OnAuthorizedOK;
        public event EventHandler<string> OnCreateSessionOK;
        public event EventHandler<string> OnSubcribeOK;
        public event EventHandler<string> OnUnsubcribeOK;
        public event EventHandler<StreamDataEventArgs> OnStreamDataReceived;
        public event EventHandler<StreamDataEventArgs> OnEventReceived;

        // Constructor
        static CortexClient()
        {

        }
        private CortexClient()
        {
            Console.WriteLine("Cortex Client constructor");
            _nextRequestId = 1;
            _methodForRequestId = new Dictionary<int, string>();

            _wSC = new WebSocket(Url);
            _wSC.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocketClient_Error);
            _wSC.Opened += new EventHandler(WebSocketClient_Opened);
            _wSC.Closed += new EventHandler(WebSocketClient_Closed);

            _wSC.MessageReceived += new EventHandler<MessageReceivedEventArgs>(WebSocketClient_MessageReceived);
        }
        public static CortexClient Instance
        {
            get
            {
                return instance;
            }
        }
        //public CortexClient()
        //{
            
        //}
        // Properties
        public string Token
        {
            get
            {
                return _token;
            }
        }

        public bool IsWSConnected
        {
            get
            {
                return _isWSConnected;
            }
        }

        // Method
        // Send request to Cortex Service
        public int GenerateRequestedID(int streamType, int requestType)
        {
            //int epocTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            string requestID = streamType.ToString() + requestType.ToString() + _nextRequestId.ToString();
            ++_nextRequestId;
            if( _nextRequestId >=100000)
            {
                _nextRequestId = 1; // reset _nextRequestID to avoid overrange Int32 for requestID
            }
            return Int32.Parse(requestID);
        }
        public int SendTextMessage(JObject param, int streamType, string method, bool hasParam = true, int requestType = 0)
        {
            if(!IsWSConnected)
            {
                return -1;
            }
            int requestID = GenerateRequestedID(streamType, requestType);
            JObject request = new JObject(
            new JProperty("jsonrpc", "2.0"),
            new JProperty("id", requestID),
            new JProperty("method", method));

            if (hasParam)
            {
                request.Add("params", param);
            }
            Console.WriteLine("SendRequest method: " + method + " requestID : " + requestID.ToString());
            // send the json message
            _methodForRequestId.Add(requestID, method);

            _wSC.Send(request.ToString());
            return requestID;
        }
        //Open socket
        public void Open()
        {
            //Open websocket
            _wSC.Open();

            if (!m_OpenedEvent.WaitOne(10000))
            {
                Console.WriteLine("Failed to Opened session ontime");
                //Assert.Fail("Failed to Opened session ontime");
            }
            if (_wSC.State == WebSocketState.Open)
            {
                _isWSConnected = true;
                OnConnected(this, true);
            }
            else
            {
                _isWSConnected = false;
                OnConnected(this, false);
            }

        }
        //Close Socket
        public void Close()
        {
            _wSC.Close();

            if (!m_CloseEvent.WaitOne(10000))
            {
                Assert.Fail("Failed to close session ontime");
            }
            Assert.AreEqual(WebSocketState.Closed, _wSC.State);

            _methodForRequestId.Clear();
            _nextRequestId = 1;
        }
        //Query headset
        public void QueryHeadsets()
        {
            SendRequest("queryHeadsets");
        }
        // login
        public void Login(string username, string password, string clientId, string clientSecret)
        {
            JObject param = new JObject(
                    new JProperty("username", username),
                    new JProperty("password", password),
                    new JProperty("client_id", clientId),
                    new JProperty("client_secret", clientSecret)
                );
            SendRequest("login", param);
        }
        //Logout
        public void Logout(string username)
        {
            JObject param = new JObject(
                    new JProperty("username", username)
                );
            SendRequest("logout", param);
        }
        //Get User Login
        public void GetUserLogin()
        {
            SendRequest("getUserLogin");
        }
        //Authorize
        public void Authorize()
        {
            JObject param = new JObject(
                    new JProperty("debit", 0));
            SendRequest("authorize", param);
        }
        public void Authorize(string clientId, string clientSecret, string license, int debitNum)
        {
            JObject param = new JObject(
                    new JProperty("client_id", clientId),
                    new JProperty("client_secret", clientSecret),
                    new JProperty("license", license),
                    new JProperty("debit", debitNum) //set default
                );
            SendRequest("authorize", param);
        }

        // Sessions
        //Create Session
        public void CreateSessions(string headsetId, bool activate)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("headset", headsetId),
                    new JProperty("status", activate ? "active" : "open"));
            SendRequest("createSession", param);
        }
        // Close Session
        public void CloseSessions(string sessionId)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("status", "close"));
            SendRequest("updateSession", param);
        }
        // Update Session
        public void UpdateSession(string sessionId,  string status, string recordingName, string recordingNote, string recordingSubject)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("status", status)
                    );
            if(!String.IsNullOrEmpty(recordingName))
            {
                param.Add("recordingName", recordingName);
            }
            if (!String.IsNullOrEmpty(recordingNote))
            {
                param.Add("recordingNote", recordingNote);
            }
            if (!String.IsNullOrEmpty(recordingSubject))
            {
                param.Add("recordingSubject", recordingSubject);
            }
            SendRequest("updateSession", param);
        }
        // Update SessionNote
        public void UpdateSessionNote(string sessionId, string note, string record)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("note", note),
                    new JProperty("record", record)
                    );
            
            SendRequest("updateNote", param);
        }
        // Query Session
        public void QuerySessions()
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token));
            SendRequest("querySessions", param);
        }
        // SessionStartRecord
        // SessionStopRecord
        // SessionAddTags
        // SessionRemoveTags

        // Subcribe
        public void Subcribe(string sessionId, string stream)
        {
            JArray jStreamArr = new JArray();
            jStreamArr.Add(stream);
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("streams", jStreamArr));
            SendRequest("subscribe", param);
        }
        // Unsubcribe
        public void UnSubcribe(string sessionId, string stream)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("streams", stream));
            SendRequest("unsubscribe", param);
        }
        // Training
        public void Training(string sessionId, string detection, string action, string control)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("session", sessionId),
                    new JProperty("detection", detection),
                    new JProperty("action", action),
                    new JProperty("status", control)
                    );
            SendRequest("training", param);
        }
        // Detection Info
        public void GetDetectionInfo(string detection)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("detection", detection));
            SendRequest("getDetectionInfo", param);
        }
        // Query profile
        public void QueryProfiles()
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token));
            SendRequest("queryProfile", param);
        }
        // Setup profile
        public void SetupProfiles(string headsetId, string profile, string status)
        {
            JObject param = new JObject(
                    new JProperty("_auth", Token),
                    new JProperty("headset", headsetId),
                    new JProperty("profile", profile),
                    new JProperty("status", status)
                     );
            SendRequest("setupProfile", param);
        }

        // Handle receieved message
        // Error message -> emit a error to Access : extract code, message, warning ? 
        // Not error message
        private void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
            Console.WriteLine("Received: " + e.Message);

            JObject response = JObject.Parse(e.Message);

            if(response["warning"] != null)
            {
                JObject warning = (JObject)response["warning"];
                string messageWarning = (string)warning["warning"];
                int code = -1;
                if(warning["code"] != null)
                    code = (int)warning["code"];
                Console.WriteLine("Received: " + messageWarning);

                OnMessageError(this, new MessageErrorEventArgs("", code, messageWarning));
            }
            if(response["sid"] != null)
            {
                string sid = (string)response["sid"];
                double time = (double)response["time"];
                if (response["mot"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.MOTION_STREAM, response));
                }
                else if(response["dev"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.DEVICE_STREAM, response));
                }
                else if (response["met"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.PERF_METRICS_STREAM, response));
                }
                else if (response["com"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.MENTAL_CMD_DATA_STREAM, response));
                }
                else if (response["fac"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.FACIAL_EXP_DATA_STREAM, response));
                }
                else if (response["sys"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.SYS_STREAM, response));
                }
                else
                {
                    Console.WriteLine("Can not detect stream type");
                }
               
            }
            else if (response["id"] != null)
            {
                int id = (int)response["id"];
                if (id != -1)
                {
                    // this is a RPC response, we get the method from the id
                    // we must know the method in order to understand the result
                    string method = _methodForRequestId[id];
                    JToken result = response["result"];
                    JObject error = (JObject)response["error"];

                    // Get stream type from ID which requestID= streamType.ToString() + requestType.ToString() + epocTime.ToString();
                    int streamType = Int32.Parse(id.ToString().Substring(0, 2));
                    int requestType = Int32.Parse(id.ToString().Substring(2, 2));

                    _methodForRequestId.Remove(id);
                    if (error != null)
                    {
                        int code = (int)error["code"];
                        string messageError = (string)error["message"];
                        Console.WriteLine("Received: " + messageError);
                        //Send Error message event
                        OnMessageError(this, new MessageErrorEventArgs(method, code, messageError));
                    }
                    else if (result != null)
                    {
                        //HandleResponse(method, result);
                        // Send Event  Message
                        OnEventReceived(this, new StreamDataEventArgs(streamType, response, requestType));
                    }

                }
            }
        }

        private void WebSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        private void WebSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        private void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }

        // a generic method to send a RPC request to Cortex
        private void SendRequest(string method, JObject param)
        {
            JObject request = new JObject(
                new JProperty("jsonrpc", "2.0"),
                new JProperty("id", _nextRequestId),
                new JProperty("method", method),
                new JProperty("params", param)
                );
            Console.WriteLine("SendRequest method: " + method);
            // send the json message
            _methodForRequestId.Add(_nextRequestId, method);
            ++_nextRequestId;
            string messge = request.ToString();
            _wSC.Send(request.ToString());
        }
        private void SendRequest(string method)
        {
            JObject request = new JObject(
                new JProperty("jsonrpc", "2.0"),
                new JProperty("id", _nextRequestId),
                new JProperty("method", method),
                new JProperty("params", "{}")
                );

            // send the json message
            _methodForRequestId.Add(_nextRequestId, method);
            ++_nextRequestId;
            _wSC.Send(request.ToString());
        }

        //Handle response message which not error
        private void HandleResponse(string method, JToken result)
        {
            Console.WriteLine("method: " + method + " result: " + result.ToString());
            if (method.Equals("queryHeadsets"))
            {
                //send event queryHeadsets OK
                List<Headset> headsetLists = new List<Headset>();
                JArray jHeadsetArr = (JArray)result;

                foreach(JObject item in jHeadsetArr)
                {
                    headsetLists.Add(new Headset(item));
                }
                if(headsetLists.Count > 0)
                {
                    OnQuerryHeadsetOK(this, headsetLists);
                }
                else
                {
                    Console.WriteLine("No headset avaible");
                }

            }
            else if (method.Equals("getUserLogin"))
            {
                JArray jUsernameArr = (JArray)result;
                List<string> usernameList = new List<string>();

                foreach (var item in jUsernameArr)
                {
                    string itemObject = (string)item;
                    usernameList.Add(itemObject);
                }
                OnGetUserLoginOK(this, usernameList);

            }
            else if (method.Equals("login"))
            {
                OnLoginOK(this,true);
            }
            else if (method.Equals("logout"))
            {
                OnLogoutOK(this, true);
            }
            else if (method.Equals("authorize"))
            {
                string token = (string)result["_auth"];
                if(token.Length > 0)
                {
                    _token = token;
                    OnAuthorizedOK(this, token);
                }
            }
            else if (method.Equals("createSession"))
            {
                string sessionId = (string)result["id"];
                //string status = (string)result["status"];
                OnCreateSessionOK(this, sessionId);
            }
            else if (method.Equals("updateSession"))
            {

            }
            else if (method.Equals("subscribe"))
            {
                JArray jArr = (JArray)result;
                string subcribeId = (string)jArr[0]["sid"];
                if (subcribeId.Length > 0)
                {
                    OnSubcribeOK(this, subcribeId);
                }
                else
                {
                    //OnMessageError(this, new MessageErrorEventArgs(method, code, messageError));
                } 
            }
            else if (method.Equals("unsubscribe"))
            {
                JArray jArr = (JArray)result;
                string message = (string)jArr[0]["message"];
                OnUnsubcribeOK(this, message);
               
            }
            else if (method.Equals("getDetectionInfo"))
            {

            }
            else if (method.Equals("training"))
            {

            }
            else
            {
                //unknow method
                Console.WriteLine("unkown RPC method:" + method);
            }
        }
    }
}
