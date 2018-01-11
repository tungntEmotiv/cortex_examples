using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    // The Access class close to User Interface
    // Reponsible for receive request and handle response before show to user
    public class Access
    {
        const string ClientId = "qutpR0xoZAJrqsaag41ZQlGmTVGocUSRX9tE7QAp";
        const string ClientSecret = "Z6Dx6t6BHrFsFFdK4BKyGnrj9X8sGYLW9hCjrrzVu6QZGlUeI6dhPq5Q0BuzZeL24tmFd0dyb9V6hM4gdOgODcqLnr6aglpXZD5mYU6ks6dnD9aMaJ8IvwxOUnjlFohO";
        private string _stream;
        private string _cortexAccessToken;
        private string _currentUser;
        private string _licenseId;
        private string _selectedHeadsetId; //selected headset
        private string _sessionId;
        //private int _experimentId;
        private int _debitNumber; //default
        private bool _isWSCConnected;
        private bool _isLogined;
        //private Session _session;
        private CortexClient _wSC;
        private List<string> _userLists; //currently, only one user loggined
        private List<Headset> _headsetLists;

        // Constructor
        public Access() { }
        public Access(string stream, string license) {

            CortexAccessToken = "";
            _isLogined = false;
            SelectedHeadsetId = "";
            Stream = stream;
            LicenseId = license;
            _debitNumber = 1;
            _isWSCConnected = false;
            _sessionId = "";

            _wSC = new CortexClient();
            _wSC.OnConnected += Connected;
            _wSC.OnMessageError += MessageErrorRecieved;
            //_wSC.OnMessageError += MessageErrorRecieved;
            _wSC.OnGetUserLoginOK += GetUserLoginOK;
            _wSC.OnLoginOK += LoginOK;
            _wSC.OnAuthorizedOK += AuthorizedOK;
            _wSC.OnQuerryHeadsetOK += QuerryHeadsetReceived;
            _wSC.OnCreateSessionOK += CreateSessionOK;
            _wSC.OnSubcribeOK += SubcribeOK;
            _wSC.OnStreamDataReceived += StreamDataReceived;
            _wSC.Open();
        }
        // Properties
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

        public List<string> UserLists
        {
            get
            {
                return _userLists;
            }

            set
            {
                _userLists = value;
            }
        }

        public string SelectedHeadsetId
        {
            get
            {
                return _selectedHeadsetId;
            }

            set
            {
                _selectedHeadsetId = value;
            }
        }

        public string CurrentUser
        {
            get
            {
                return _currentUser;
            }

            set
            {
                _currentUser = value;
            }
        }

        public string Stream
        {
            get
            {
                return _stream;
            }

            set
            {
                _stream = value;
            }
        }

        public string LicenseId
        {
            get
            {
                return _licenseId;
            }
            set
            {
                _licenseId = value;
            }
        }

        public int DebitNumber
        {
            get
            {
                return _debitNumber;
            }

            set
            {
                _debitNumber = value;
            }
        }

        public string CortexAccessToken
        {
            get
            {
                return _cortexAccessToken;
            }

            set
            {
                _cortexAccessToken = value;
            }
        }

        public bool IsLogined
        {
            get
            {
                return _isLogined;
            }
        }

        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }

        // Method
        //Login
        //Send login request with usernam and password
        public void Login(string username, string password)
        {
            if (_isWSCConnected)
            {
                //login
                if (!IsLogined)
                {
                    _wSC.Login(username, password, ClientId, ClientSecret);
                }
                else
                {
                    Console.WriteLine("Someone logined, Please log out before login");
                }

            }
        }
        //Get User Login
        public void QueryUserLogin()
        {
            _wSC.GetUserLogin();
        }

        //Get current user
        public string GetCurrentUser()
        {
            return CurrentUser;
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

        //Create Session
        public void CreateSession()
        {
            if (IsLogined && !string.IsNullOrEmpty(SelectedHeadsetId) && !string.IsNullOrEmpty(CortexAccessToken))
            {
                _wSC.CreateSessions(SelectedHeadsetId, !string.IsNullOrEmpty(LicenseId));
            }
            else
            {
                Console.WriteLine("Please authorize before Create session");
            }
        }
        //Close
        public void Stop()
        {
            _wSC.Close();
        }

        // Handle response message from cortex client
        private void Connected(object sender, bool isConnected)
        {
            if (isConnected)
            {
                // After connected to Cortex, client must obtain token for working with cortex, by using one of below actions:
                //  client.getUserLogin(): then logout, then login with username/password;
                //  client.login(): Login with username/password;
                //  client.authorize(): Get token from Cortex
                //  client.authorize(clientId, clientSecrect, liencense);
                //  client.setToken("You token"): Tell client to use old token (which be saved before)

                // first step: get the current user
                // Note: if you already have a token, you can reuse it
                // so you can skip the login procedure and call onAuthorizeOk("your token")

                _isWSCConnected = true;
                _wSC.QueryHeadsets();
                _wSC.GetUserLogin();


            }
            else
            {
                _isWSCConnected = false;
            }
        }
        private void LoginOK(object sender, bool isLoginOK)
        {
            if (isLoginOK)
            {
                _isLogined = isLoginOK;
                if (String.IsNullOrEmpty(LicenseId))
                {
                    _wSC.Authorize();
                }
                else
                {
                    _wSC.Authorize(ClientId, ClientSecret, LicenseId, DebitNumber);
                }

            }
            else
            {
                _isLogined = false;
                Console.WriteLine("Login unsuccessfully");
            }
        }

        private void GetUserLoginOK(object sender, List<string> username)
        {
            if (username.Count == 0)
            {
                // no one is logged in, you can login now
                Console.WriteLine("No one is logged in");
                _isLogined = false;

            }
            else
            {
                _isLogined = true;
                UserLists = new List<string>(username);
                CurrentUser = UserLists[0];
                LoginOK(sender, true);
            }
        }
        private void QuerryHeadsetReceived(object sender, List<Headset> headsets)
        {
            Console.WriteLine("QuerryHeadsetReceived");
            if (headsets.Count > 0)
            {
                HeadsetLists = new List<Headset>(headsets);

                // we take the first headset
                // TODO in a real application, you should ask the user to choose a headset from the list
                SelectedHeadsetId = HeadsetLists[0].HeadsetID;

                Console.WriteLine("Selected HeadsetID " + SelectedHeadsetId);
            }
            else
            {
                Console.WriteLine("Please connect to Headset");
            }

        }
        private void AuthorizedOK(object sender, string token)
        {
            Console.WriteLine("Authorize successfully");
            CortexAccessToken = token;
        }
        private void CreateSessionOK(object sender, string sessionId)
        {
            _sessionId = sessionId;

            // next step: subscribe to a data stream
            _wSC.Subcribe(_sessionId, Stream);
        }

        
        private void StreamDataReceived(object sender, StreamDataEventArgs data)
        {

            Console.Write(data.Stream);
            Console.WriteLine(data.Data);
            Console.WriteLine("sid " + data.SID);
        }

        private void SubcribeOK(object sender, string subcribeId)
        {
            Console.WriteLine("Subscription successful, sid " + subcribeId);
        }

        //Recieved error message
        public void MessageErrorRecieved(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("Message Error recieved from " + sender.ToString());
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError + " at " + e.Method);
        }


        
    }
}
