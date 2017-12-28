using System;
using NUnit.Framework;
using System.Threading;
using WebSocket4Net;

namespace CortexExamples
{
    class CortexClient
    {
        private static WebSocket client;
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);

        const string host = "wss://emotivcortex.com:54321";

        protected string m_CurrentMessage = string.Empty;

        public void open()
        {
            client = new WebSocket(host);
            client.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            client.Opened += new EventHandler(webSocketClient_Opened);
            client.Closed += new EventHandler(webSocketClient_Closed);

            client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            //Open websocket
            client.Open();

            if (!m_OpenedEvent.WaitOne(10000))
            {
                Console.WriteLine("Failed to Opened session ontime");
                //Assert.Fail("Failed to Opened session ontime");
            }
            Assert.AreEqual(WebSocketState.Open, client.State);

            //Send message 
            client.Send("Hello World");
            if (!m_MessageReceiveEvent.WaitOne(1000))
                Assert.Fail("Cannot get response in time!");


            //Close websocket

            client.Close();

            if (!m_CloseEvent.WaitOne(1000))
            {
                Assert.Fail("Failed to close session ontime");
            }
            Assert.AreEqual(WebSocketState.Closed, client.State);

        }

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
