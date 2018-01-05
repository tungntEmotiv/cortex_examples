using System;
using NUnit.Framework;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CortexExamples
{
    class CortexClient
    {

        //const string host = "wss://emotivcortex.com:54321";

        public CortexClient()
        {
            Console.WriteLine("Cortex Client constructor");
            _nextRequestId = 1;
            _methodForRequestId = new Dictionary<int, string>();

            _wSC = new WebSocket("wss://emotivcortex.com:54321");
            _wSC.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocketClient_Error);
            _wSC.Opened += new EventHandler(WebSocketClient_Opened);
            _wSC.Closed += new EventHandler(WebSocketClient_Closed);

            _wSC.MessageReceived += new EventHandler<MessageReceivedEventArgs>(WebSocketClient_MessageReceived);
        }

        //Websocket Client
        private  WebSocket _wSC;

        protected string m_CurrentMessage = string.Empty;
        private int _nextRequestId; //unique id for each request
        //map method with requestID
        private Dictionary<int, string> _methodForRequestId;

        //Events
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);

        public event EventHandler<MessageErrorEventArgs> OnMessageError;
        public event EventHandler<List<Headset>> OnQuerryHeadsetReceived;

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
            Assert.AreEqual(WebSocketState.Open, _wSC.State);

        }

        //Close Socket
        public void Close()
        {
            _wSC.Close();

            if (!m_CloseEvent.WaitOne(1000))
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
                    new JProperty("username",username),
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
            SendRequest("GetUserLogin");
        }

        //Authorize
        public void Authorize()
        {
            SendRequest("authorize");
        }

        // open a session, so we can then get data from a headset


        // subscribe to a data stream

        // methods for training


        

        //Handle message received
        protected void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            Console.WriteLine("Received: " + e.Message);

            JObject response = JObject.Parse(e.Message);

            int id = (int)response["id"];

            if(id !=-1)
            {
                // this is a RPC response, we get the method from the id
                // we must know the method in order to understand the result
                string method = _methodForRequestId[id];
                JToken result = response["result"];
                JObject error = (JObject)response["error"];

                _methodForRequestId.Remove(id);

                if(error != null )
                {
                    int code = (int)error["code"];
                    string messageError = (string)error["message"];
                    Console.WriteLine("Received: " + messageError);
                    //Send Error message event
                    OnMessageError(this, new MessageErrorEventArgs(method, code, messageError));
                }
                else if (result != null)
                {
                    HandleResponse(method, result);
                }

            }


            m_MessageReceiveEvent.Set();
        }

        protected void WebSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        protected void WebSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        protected void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
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

            // send the json message
            _methodForRequestId.Add(_nextRequestId, method);
            ++_nextRequestId;

            _wSC.Send(request.ToString());
            if (!m_MessageReceiveEvent.WaitOne(10000))
                Assert.Fail("Cannot get response in time!");
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
            if (!m_MessageReceiveEvent.WaitOne(1000))
                Assert.Fail("Cannot get response in time!");

        }

        //Handle response message which not error
        private void HandleResponse(string method, JToken result)
        {
            Console.WriteLine("method: " + method);
            Console.WriteLine("result: " + result.ToString());
            if (method.Equals("queryHeadsets"))
            {
                //send event queryHeadsets OK
                List<Headset> headsetLists = new List<Headset>();
                JArray jHeadsetArr = (JArray)result;

                foreach(JObject item in jHeadsetArr)
                {
                    headsetLists.Add(new Headset(item));
                }

                OnQuerryHeadsetReceived(this, new List<Headset>(headsetLists));

            }
            else if (method.Equals("GetUserLogin"))
            {

            }
            else if (method.Equals("login"))
            {

            }
            else if (method.Equals("logout"))
            {

            }
            else if (method.Equals("authorize"))
            {

            }
            else if (method.Equals("createSession"))
            {

            }
            else if (method.Equals("updateSession"))
            {

            }
            else if (method.Equals("subscribe"))
            {

            }
            else if (method.Equals("unsubscribe"))
            {

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
            }
        }
    }

    internal class MessageErrorEventArgs
    {
        public MessageErrorEventArgs(string method, int code, string messageError)
        {
            Method = method;
            Code = code;
            MessageError = messageError;
        }
        public int Code { get; set; }
        public  string MessageError { get; set; }
        public  string Method { get; set; }
    }
}
