using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public class Process
    {
        // Member variables
        private CortexClient _wSC;
        private AccessController _accessCtr;
        private HeadsetController _headsetCtr;
        private SessionController _sessionCtr;
        private TrainingController _trainingCtr;
        private int _experimentID;
        private Dictionary<int, BaseController> _mapControllers;
        private string _licenseId;

        // Constructor
        public Process()
        {
            _wSC = CortexClient.Instance;
            AccessCtr = AccessController.Instance;
            HeadsetCtr = HeadsetController.Instance;
            SessionCtr = SessionController.Instance;
            TrainingCtr = TrainingController.Instance;
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
            HeadsetCtr.OnDisconnectHeadset += DisconnectHeadsetReceived;

            _mapControllers.Add((int)StreamID.AUTHORIZE_STREAM, AccessCtr);
            _mapControllers.Add((int)StreamID.HEADSETS_STREAM, HeadsetCtr);
            _mapControllers.Add((int)StreamID.SESSION_STREAM, SessionCtr);
            _mapControllers.Add((int)StreamID.TRAINING_STREAM, TrainingCtr);
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

        public TrainingController TrainingCtr
        {
            get
            {
                return _trainingCtr;
            }

            set
            {
                _trainingCtr = value;
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

        public bool IsCreateSession
        {
            get
            {
                return SessionCtr.IsCreateSession;
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
            LicenseId = license;
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

        // Set status for session
        //public void SetNextStatus(string status)
        //{
        //    SessionCtr.NextStatus = status;
        //}

        // Create Session
        public void CreateSession()
        {
            if (string.IsNullOrEmpty(LicenseId))
                SessionCtr.NextStatus = "opened";
            else
                SessionCtr.NextStatus = "active";
            if (!SessionCtr.IsCreateSession)
                SessionCtr.CreateSession(GetSelectedHeadsetId(), GetAccessToken(), ExperimentID);
        }

        // Query Session
        public void QuerySession()
        {
            SessionCtr.QuerrySession(GetAccessToken());
        }
        // Close Session
        public void CloseSession()
        {
            SessionCtr.CloseSession(GetAccessToken());
        }

        // Get current Session
        public string GetCurrentSessionID()
        {
            return SessionCtr.SessionID;
        }

        // Subcribe data
        public void SubcribeData(string stream)
        {
            SessionCtr.RequestData(GetAccessToken(), stream, false, true);
        }
        // Unsubcribe data
        public void UnSubcribeData(string stream)
        {
            SessionCtr.RequestData(GetAccessToken(), stream, false, false);
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

        // Update Notes
        public void UpdateNote(string note)
        {
            SessionCtr.UpdateNote(GetAccessToken(), SessionCtr.RecordID, note);
        }
        // Inject marker
        public bool InjectMarker(string label, int value, Int64 timeStamp)
        {
            return SessionCtr.InjectMarker(GetAccessToken(), "USB", label, value, timeStamp);
        }
        
        
        // Get Detection Information
        public void QuerryDetectionInfo(string detection)
        {
            TrainingCtr.QuerryDetectionInformation(detection);
        }

        // Profile
        // Querry profile of user
        public void QuerryProfiles()
        {
            TrainingCtr.QuerryProfile(GetAccessToken());
        }

        // Load profile
        public void LoadProfile(string profileName)
        {
            TrainingCtr.CurrentProfileName = profileName;
            TrainingCtr.LoadProfile(GetAccessToken(), GetSelectedHeadsetId(), profileName);
        }
        // Create profile
        public void CreateProfile(string profileName)
        {
            TrainingCtr.CurrentProfileName = profileName;
            TrainingCtr.CreateProfile(GetAccessToken(), profileName);
        }
        // Save a profile
        public void SaveProfile()
        {
            TrainingCtr.SaveProfile(GetAccessToken(), TrainingCtr.CurrentProfileName);
        }
        // Delete a profile
        public void DeleteProfile(string profileName)
        {
            TrainingCtr.DeleteProfile(GetAccessToken(), profileName);
        }
        // Edit a profile
        public void EditProfile(string profileName, string newProfileName)
        {
            TrainingCtr.EditProfile(GetAccessToken(), profileName, newProfileName);
        }
        // Training
        // Start Mental Command Training
        public void StartCmd(string action)
        {
            TrainingCtr.CmdTrainingAction = action;
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.START_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void AcceptCmd()
        {
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.ACCEPT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void RejectCmd()
        {
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.REJECT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void ResetCmd(string action)
        {
            TrainingCtr.CmdTrainingAction = action;
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.RESET_TRAINING, GetCurrentSessionID());
        }

        // Start Facial Expression Training
        public void StartFE(string action)
        {
            TrainingCtr.FETrainingAction = action;
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.START_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void AcceptFE()
        {
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.ACCEPT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void RejectFE()
        {
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.REJECT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void ResetFE(string action)
        {
            TrainingCtr.FETrainingAction = action;
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.RESET_TRAINING, GetCurrentSessionID());
        }

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

        private void StreamDataReceived(object sender, StreamDataEventArgs evt)
        {
            // Send to corresponding Controller
            int streamType = evt.StreamType;
            int requestType = evt.RequestType;
            

            switch (streamType)
            {
                case (int)StreamID.MOTION_STREAM:
                    Console.WriteLine("Motion data");
                    JArray motData = (JArray)evt.Data["mot"];
                    foreach(var item in motData)
                    {
                        Console.Write(item.ToString());
                    }
                    Console.WriteLine("\n");
                    break;
                case (int)StreamID.EEG_STREAM:
                    Console.WriteLine("EEG data");
                    JArray eegData = (JArray)evt.Data["eeg"];
                    foreach (var item in eegData)
                    {
                        Console.Write(item.ToString() + ",");
                    }
                    Console.WriteLine("\n");
                    break;
                case (int)StreamID.SYS_STREAM:
                    Console.WriteLine("System event");
                    Console.WriteLine("\n");
                    break;
                default:
                    break;
            }
                
            
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
                if (!AccessCtr.IsLogin)
                    AccessCtr.QueryUserLogin();
                // Query Headset
                if(!HeadsetCtr.IsConnected)
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
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError);
        }

        private void QuerryHeadsetReceived(object sender, List<Headset> headsets)
        {
            Console.WriteLine("QuerryHeadsetReceived");

            // Create a Session when have new update of headset connection
            if (string.IsNullOrEmpty(GetAccessToken()))
                return;
            if(!SessionCtr.IsCreateSession)
                SessionCtr.CreateSession(GetSelectedHeadsetId(),GetAccessToken(),ExperimentID);
        }

        private void AuthorizeOK(object sender, string token)
        {
            Console.WriteLine("Authorize successfully!!!. Access Token " + token);
        }
        private void LoginOK(object sender, bool isLogin)
        {
            Console.WriteLine("Login successfully !!!");
        }

        private void DisconnectHeadsetReceived(object sender, string headsetID)
        {
            Console.WriteLine("Disconnect headset " + headsetID);
            // Clear session
            //if (SessionCtr.IsCreateSession)
            //    CloseSession();
            SessionCtr.ClearSessionControllerData();
            
        }


    }
}
