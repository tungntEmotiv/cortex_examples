using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexExamples
{
    // The Access class close to User Interface
    // Reponsible for receive request and handle response before show to user
    class Access
    {
        const string ClientId = "qutpR0xoZAJrqsaag41ZQlGmTVGocUSRX9tE7QAp";
        const string ClientSecret = "Z6Dx6t6BHrFsFFdK4BKyGnrj9X8sGYLW9hCjrrzVu6QZGlUeI6dhPq5Q0BuzZeL24tmFd0dyb9V6hM4gdOgODcqLnr6aglpXZD5mYU6ks6dnD9aMaJ8IvwxOUnjlFohO";

        public Access() {

            _wSC = new CortexClient();

            _wSC.OnConnected += Connected;
            _wSC.OnMessageError += MessageErrorRecieved;
            _wSC.OnGetUserLoginOK += GetUserLoginOK;
            _wSC.OnLoginOK += LoginOK;
            _wSC.OnAuthorizedOK += AuthorizedOK;
            _wSC.OnQuerryHeadsetOK += QuerryHeadsetReceived;
            _wSC.Open();
        }

        private void AuthorizedOK(object sender, string token)
        {
            Console.WriteLine("Authorize successfully");
            CortexAccessToken = token;
        }

        private void LoginOK(object sender, bool isLoginOK)
        {
            if(isLoginOK)
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
            if( username.Count == 0)
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

        private void Connected(object sender, bool isConnected)
        {
            if(isConnected)
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
                _wSC.GetUserLogin();
                _wSC.QueryHeadsets();

            }
            else
            {
                _isWSCConnected = false;
            }
        }

        //Handle received message from Cortex Client
        private void QuerryHeadsetReceived(object sender, List<Headset> headsets)
        {
            Console.WriteLine("QuerryHeadsetReceived");
            HeadsetLists = new List<Headset>(headsets);

            // we take the first headset
            // TODO in a real application, you should ask the user to choose a headset from the list
            SelectedHeadsetId = HeadsetLists[0].HeadsetID;

            Console.WriteLine("Selected HeadsetID " + SelectedHeadsetId);
            // next step: create a session for this headset
            if (_isLogined)
            {
                //
            }
        }
        //Recieved error message
        public void MessageErrorRecieved(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("Message Error recieved from " + sender.ToString());
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError + " at " + e.Method);
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
        //Send login request with usernam and password
        //Wait reponse show message
        public void Login(string username, string password)
        {
            if (_isWSCConnected)
            {
                
                //login
                _wSC.Login(username, password, ClientId, ClientSecret);
            }
        }

        //Get user login
        public string GetUserLogin()
        {
            //send get userlogin
            _wSC.GetUserLogin();

            //resturn userlogin
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

        




        //Field
        private string _stream;
        private string _cortexAccessToken;
        private string _currentUser;
        private string _licenseId;
        private string _selectedHeadsetId; //selected headset
        private int _sessionId;
        private int _experimentId;
        private int _debitNumber = 10; //default

        private bool _isWSCConnected = false;

        //List headset

        private bool _isLogined;

        private Session _session;
        private CortexClient _wSC;

        private List<string> _userLists; //currently, only one user loggined

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

        public int SessionId
        {
            get
            {
                return _sessionId;
            }

            set
            {
                _sessionId = value;
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
    }
}
