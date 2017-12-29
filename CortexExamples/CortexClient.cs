using System;
using NUnit.Framework;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json;

namespace CortexExamples
{
    class CortexClient
    {

        public CortexClient()
        {
            Console.WriteLine("Cortex Client constructor");
            wsc = new WebSocket(host);
            wsc.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            wsc.Opened += new EventHandler(webSocketClient_Opened);
            wsc.Closed += new EventHandler(webSocketClient_Closed);

            wsc.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
        }

        private static WebSocket wsc;
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);

        const string host = "wss://emotivcortex.com:54321";

        protected string m_CurrentMessage = string.Empty;
        //Open socket
        public void open()
        {
            //Open websocket
            wsc.Open();

            if (!m_OpenedEvent.WaitOne(10000))
            {
                Console.WriteLine("Failed to Opened session ontime");
                //Assert.Fail("Failed to Opened session ontime");
            }
            Assert.AreEqual(WebSocketState.Open, wsc.State);

        }

        //Close Socket
        public void close()
        {
            wsc.Close();

            if (!m_CloseEvent.WaitOne(1000))
            {
                Assert.Fail("Failed to close session ontime");
            }
            Assert.AreEqual(WebSocketState.Closed, wsc.State);
        }

        //Query headset


        // login / logout

        // get an authorization token

        // open a session, so we can then get data from a headset


        // subscribe to a data stream

        // methods for training


        // a generic method to send a RPC request to Cortex
        private void sendRequest(string method, )
        {
            //Send message 
            wsc.Send("Hello World");
            if (!m_MessageReceiveEvent.WaitOne(1000))
                Assert.Fail("Cannot get response in time!");
        }

        // handle the response to a RPC request



        protected void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            Console.WriteLine("Received: " + e.Message);
            m_MessageReceiveEvent.Set();
        }


        protected void webSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        protected void webSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        protected void webSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }
    }
}
