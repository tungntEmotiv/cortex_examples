using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    // Stream type enum

    

    public class Process
    {
        const string ClientId = "qutpR0xoZAJrqsaag41ZQlGmTVGocUSRX9tE7QAp";
        const string ClientSecret = "Z6Dx6t6BHrFsFFdK4BKyGnrj9X8sGYLW9hCjrrzVu6QZGlUeI6dhPq5Q0BuzZeL24tmFd0dyb9V6hM4gdOgODcqLnr6aglpXZD5mYU6ks6dnD9aMaJ8IvwxOUnjlFohO";

        // Member variables
        private Dictionary<int, string> _streamMap;
        private CortexClient _wSC;
        private AccessController _accessCtr;
        private HeadsetController _headsetCtr;
        private SessionController _sessionCtr;
        private int _experimentID;
        private bool _isLogined;
        private string _currentUser;

        // Headset Controller

        // Session Controller

        // Profile Controller


        private string _stream;
        private string _cortexAccessToken;
        private Dictionary<int, BaseController> _mapControllers;
        
        private string _licenseId;
        private string _selectedHeadsetId; //selected headset
        //private int _experimentId;
        private int _debitNumber; //default
        
        //private Session _session;
        private List<string> _userLists; //currently, only one user loggined
        private List<Headset> _headsetLists;

        

        // Constructor
        public Process()
        {
            _wSC = CortexClient.Instance;
            AccessCtr = AccessController.Instance;
            HeadsetCtr = HeadsetController.Instance;
            _mapControllers = new Dictionary<int, BaseController>();
            LicenseId = "";

            // Event register
            _wSC.OnConnected += Connected;
            _wSC.OnMessageError += MessageErrorRecieved;
            _wSC.OnStreamDataReceived += StreamDataReceived;
            _wSC.OnEventReceived += EventReceived;
            AccessCtr.OnLoginOK += LoginOK;
            AccessCtr.OnAuthorizedOK += AuthorizeOK;
            HeadsetCtr.OnQuerryHeadsetOK += QuerryHeadsetReceived;

            _mapControllers.Add((int)StreamID.AUTHORIZE_STREAM, AccessController.Instance);
            _mapControllers.Add((int)StreamID.QUERY_HEADSETS_STREAM, HeadsetController.Instance);
            _mapControllers.Add((int)StreamID.SESSION_STREAM, new SessionController());
            _wSC.Open();

        } 

        // Properties
        public AccessController AccessCtr
        {
            get
            {
                return _accessCtr;
            }

            set
            {
                _accessCtr = value;
            }
        }

        public HeadsetController HeadsetCtr
        {
            get
            {
                return _headsetCtr;
            }

            set
            {
                _headsetCtr = value;
            }
        }

        public SessionController SessionCtr
        {
            get
            {
                return _sessionCtr;
            }

            set
            {
                _sessionCtr = value;
            }
        }

        public int ExperimentID
        {
            get
            {
                return _experimentID;
            }

            set
            {
                _experimentID = value;
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

        // Method
        public void Login(string username, string password)
        {
            AccessCtr.Login(username, password);
        }
        // Querry User Login
        public void QueryUserLogin()
        {
            AccessCtr.QueryUserLogin();
        }

        // Get current userlogin
        public string  GetUserLogin()
        {
            return AccessCtr.CurrentUserLogin;
        }

        // Change user login
        public void SetUserLogin(string newUsername, string newPassword)
        {
            // Check username existed in userlists

            // Change to username

            // Re-login with new username and new password

        }
        // Logout
        public void Logout(string username)
        {
            AccessCtr.Logout();
        }
        // Authorize license
        public void Authorize(string license = "", int debitNumber = 0)
        {
            AccessCtr.Authorize(license, debitNumber);
        }

        // Get Access Token
        public string GetAccessToken()
        {
            return AccessCtr.CurrentAccessToken;
        }

        //Query Headset
        public void QueryHeadset()
        {
            HeadsetCtr.QueryHeadsets();
        }

        // Get Selected Headset ID
        public string GetSelectedHeadsetId()
        {
            return HeadsetCtr.SelectedHeadsetId;
        }

        // Change Headset
        public void SetHeadsetID(string headsetId)
        {
            // check headsetId existed in lists

            // change to new headsetID

        }

        // Create Session
        public void CreateSession()
        {
            if( SessionCtr == null)
                SessionCtr = new SessionController();
            if (string.IsNullOrEmpty(LicenseId))
                SessionCtr.NextStatus = "opened";
            else
                SessionCtr.NextStatus = "activated";
            SessionCtr.CreateSession(GetSelectedHeadsetId(), GetAccessToken(), ExperimentID);
        }

        // Update Session
        public void UpdateSession()
        {

        }

        // Start Record
        public void StartRecord(string recordName, string recordSubject, string recordNote)
        {
            if(SessionCtr.IsCreateSession)
            {
                SessionCtr.NextStatus = "startRecord";
                SessionCtr.RecordingName = recordName;
                SessionCtr.RecordingSubject = recordSubject;
                SessionCtr.RecordingNote = recordNote;
                SessionCtr.UpdateSession(GetAccessToken(), recordName, recordNote, recordSubject);
            }
        }
        // Stop Record
        public void StopRecord()
        {
            if (SessionCtr.IsRecording)
            {
                SessionCtr.NextStatus = "stopRecord";
                SessionCtr.UpdateSession(GetAccessToken(), SessionCtr.RecordingName, SessionCtr.RecordingNote, SessionCtr.RecordingSubject);
            }
        }
        // Method

        // Query Headset

        // Get userlogin

        // Query username

        // Login(username, password, clientID, clientSecret)

        // Logout(username)

        // Get Access Token

        // Create Session

        // Update Session

        // Subcribe

        // Start Record

        // Update session Note

        // Send marker


        // Stop Record

        // Get EEG data

        // Get Motion data

        // Get Contact Quality

        // Handle Event Response
        private void EventReceived(object sender, StreamDataEventArgs evt)
        {
            // Send to corresponding Controller
            int streamType = evt.StreamType;
            int requestType = evt.RequestType;
            // if stream Type Exist
            if (_mapControllers.ContainsKey(streamType))
            {
                _mapControllers[streamType].ParseData(evt.Data, requestType);

            }
            else
            {
                Console.WriteLine("can not detect stream type" + streamType.ToString());
            }
        }

        private void StreamDataReceived(object sender, StreamDataEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connected(object sender, bool isConnected)
        {
            if (isConnected)
            {
                Console.WriteLine("Websocket Client Connected");

                // After connected to Cortex, client must obtain token for working with cortex, by using one of below actions:
                //  client.getUserLogin(): then logout, then login with username/password;
                //  client.login(): Login with username/password;
                //  client.authorize(): Get token from Cortex
                //  client.authorize(clientId, clientSecrect, liencense);
                //  client.setToken("You token"): Tell client to use old token (which be saved before)

                // first step: get the current user
                // Note: if you already have a token, you can reuse it
                // so you can skip the login procedure and call onAuthorizeOk("your token")

                AccessCtr.QueryUserLogin();
                // Query Headset
                HeadsetCtr.QueryHeadsets();

            }
            else
            {
                Console.WriteLine("Websocket Client disconnect");
            }
        }

        //Recieved error message
        public void MessageErrorRecieved(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("Message Error recieved from " + sender.ToString());
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError + " at " + e.Method);
        }

        private void QuerryHeadsetReceived(object sender, List<Headset> headsets)
        {
            Console.WriteLine("QuerryHeadsetReceived");
        }

        private void AuthorizeOK(object sender, string token)
        {
            Console.WriteLine("Authorize successfully!!!. Access Token " + token);
        }
        private void LoginOK(object sender, bool isLogin)
        {
            Console.WriteLine("Login successfully !!!");
        }


    }
}
