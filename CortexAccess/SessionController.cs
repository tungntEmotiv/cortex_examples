using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CortexAccess
{
    public class SessionController : BaseController
    {
        public enum SessionReqType : int
        {
            CREATE_SESSION = 10,
            UPDATE_SESSION = 11,
            START_RECORD = 12,
            STOP_RECORD = 13,
            INJECT_MARKER = 14,
            UPDATE_NOTE = 15,
            SUBCRIBE_DATA = 16,
            UNSUBCRIBE_DATA = 17,
            SUBCRIBE_ALLDATA = 18,
            UNSUBCRIBE_ALLDATA = 19
        }

        // Member variables
        private Session _currentSession;
        private string _sessionID;
        //private Record _currentRecord;
        private string  _nextStatus;
        private bool _isCreateSession;
        private bool _isRecording;
        private string _recordingName;
        private string _recordingNote;
        private string _recordingSubject;

        // Constructor
        public SessionController() {
            NextStatus = "opened";
            _isCreateSession = false;
        }

        // Properties

        public string NextStatus
        {
            get
            {
                return _nextStatus;
            }

            set
            {
                _nextStatus = value;
            }
        }

        public bool IsCreateSession
        {
            get
            {
                return _isCreateSession;
            }
        }

        public string SessionID
        {
            get
            {
                return _sessionID;
            }

            set
            {
                _sessionID = value;
            }
        }

        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }
        }

        public string RecordingName
        {
            get
            {
                return _recordingName;
            }

            set
            {
                _recordingName = value;
            }
        }

        public string RecordingNote
        {
            get
            {
                return _recordingNote;
            }

            set
            {
                _recordingNote = value;
            }
        }

        public string RecordingSubject
        {
            get
            {
                return _recordingSubject;
            }

            set
            {
                _recordingSubject = value;
            }
        }

        // Method
        // Request 
        // Create Session
        public void CreateSession(string headsetID, string token, int experimentID = 0)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("status", NextStatus));

            if (experimentID > 0)
            {
                param.Add("experimentID", experimentID);
            }

            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "createSession", true, (int)SessionReqType.CREATE_SESSION);
        }

        // Update Session
        // start Record
        // Stop Record
        public void UpdateSession(string token, string recordingName = "", string recordingNote= "", string recordingSubject="")
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("status", NextStatus));

            if (NextStatus == "startRecord" || NextStatus == "stopRecord")
            {
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingName", recordingName);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingSubject", recordingSubject);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingNote", recordingNote);
            }
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateSession", true, (int)SessionReqType.UPDATE_SESSION);
        }
        // Query Sessions

        // Update Note
        public void UpdateNote(string token, string recordID, string note)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("record", recordID),
                    new JProperty("note", note));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateNote", true, (int)SessionReqType.UPDATE_NOTE);
        }

        // Inject markers
        public bool InjectMarker(string token, string port, string label, int value, double epocTime)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("port", port),
                    new JProperty("label", label),
                    new JProperty("value", value),
                    new JProperty("time", epocTime));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "injectMarker", true, (int)SessionReqType.INJECT_MARKER);
            return true;
        }
        // Request Data
        public void RequestData(string token, string stream, bool isReplay, bool isSubcribe)
        {
            JArray jStreamArr = new JArray();
            jStreamArr.Add(stream);

            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("streams", jStreamArr),
                    new JProperty("replay", isReplay));

            if (isSubcribe)
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SUBSCRIBE_DATA, "subscribe", true, (int)SessionReqType.SUBCRIBE_DATA);
            }
            else
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SUBSCRIBE_DATA, "unsubscribe", true, (int)SessionReqType.UNSUBCRIBE_DATA);
            }
        }
        // Request Data
        public void RequestAllData(string token, string sessionId, bool isReplay, bool isSubcribe)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", sessionId),
                    new JProperty("streams", new JArray("eeg", "mot", "dev", "met")),
                    new JProperty("replay", isReplay));

            if (isSubcribe)
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SUBSCRIBE_DATA, "subscribe", true, (int)SessionReqType.SUBCRIBE_ALLDATA);
            }
            else
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SUBSCRIBE_DATA, "unsubscribe", true, (int)SessionReqType.UNSUBCRIBE_ALLDATA);
            }
        }

        // Handle Event Reponse
        public override void ParseData(JObject data, int requestType)
        {
            Console.WriteLine(" Session Controller: parse data ");
            if (data["result"] != null)
            {
                switch (requestType)
                {
                    case (int)SessionReqType.CREATE_SESSION:

                        //send event queryHeadsets OK
                        JToken result = (JToken)data["result"];

                        Console.WriteLine("status " + (string)result["status"]);
                        // Send create session successfully
                        
                        break;
                    case (int)SessionReqType.SUBCRIBE_DATA:
                        //send event queryHeadsets OK
                        JArray jArrResult = (JArray)data["result"];
                        string sid = "";
                        foreach (JObject item in jArrResult)
                        {
                            sid = (string)item["sid"];
                        }
                        Console.WriteLine("sid" + sid);
                        break;

                    case (int)SessionReqType.START_RECORD:
                        break;
                    case (int)SessionReqType.STOP_RECORD:
                        break;
                    case (int)SessionReqType.INJECT_MARKER:
                        break;
                    default:
                        break;
                }
            }
            // throw new NotImplementedException();
        }

        // Set Record Information

        // Get Current Record Information


    }
}
