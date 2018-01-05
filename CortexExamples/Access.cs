using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexExamples
{
    class Access
    {
        public Access() {

            _wSC = new CortexClient();
            _wSC.OnMessageError += MessageErrorRecieved;
            _wSC.OnQuerryHeadsetReceived += QuerryHeadsetReceived;
            _wSC.Open();
        }

        private void QuerryHeadsetReceived(object sender, List<Headset> e)
        {
            Console.WriteLine("QuerryHeadsetReceived");
            HeadsetLists = new List<Headset>(e);
        }


        //Start 
        //public void Start()
        //{
        //     //check opening, if opening will close and re-open
        //    _wSC = new CortexClient();
        //    _wSC.OnMessageError += MessageErrorRecieved;
        //    _wSC.Open();
        //}

        //Close
        public void Stop()
        {
            _wSC.Close();
        }

        //Login
        public void Login(string username, string password, string clientId, string clientSecret)
        {
            if(_isWSCConnected)
            {
                //login
                _wSC.Login(username, password, clientId, clientSecret);
            }
        }

        //Logout
        public void Logout(string username)
        {
            if (_isWSCConnected)
            {
                //login
                _wSC.Logout(username);
            }
        }

        //Query Headset
        public void QueryHeadset()
        {
            _wSC.QueryHeadsets();
        }

        //Recieved error message
        public void MessageErrorRecieved (object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("Message Error recieved from " + sender.ToString());
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError + " at " + e.Method);
        }




        //Field
        private string _stream;
        private string _cortexAccessToken;
        private string _currentUser;
        private string _licenseId;
        private int _selectedHeadsetId; //selected headset
        private int _sessionId;
        private int _experimentId;

        private bool _isWSCConnected = false;

        //List headset

        private bool _isLogin;

        private Session _session;
        private CortexClient _wSC;

        private List<Headset> _headsetLists;
        public List<Headset> HeadsetLists
        {
            get
            {
                return _headsetLists;
            }

            set
            {
                _headsetLists = value;
            }
        }
    }
}
